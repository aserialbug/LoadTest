using Balance.Domain;
using Balance.Dto;
using Balance.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace Balance.Dao;

public class BalanceDao : IBalanceDao
{
    private const string GetByIdSql = @"select 
                                            b.balance as balance_amount,
                                            b.created as balance_created,
                                            b.updated as balance_updated,
                                            o.id as operation_id,
                                            o.type as operation_type,
                                            o.amount as operation_amount,
                                            o.seq_n as operation_sequence_number,
                                            o.created as operation_id
                                        from balances b left join operations o on b.id = o.balance_id where b.id = @id;";

    private const string AddOperationSql = @"insert into operations 
                                                    (id, balance_id, type, amount, seq_n, created) 
                                                values (@id, @balance_id, @type, @amount, @seq_n, @created) 
                                                on conflict (id) do nothing;";

    private const string AddBalanceSql = @"insert into balances 
                                                (id, balance, created, updated) 
                                            values (@id, @balance, @created, @updated)
                                            on conflict (id) do update 
                                                set balance = @balance, updated = @updated;";

    private const string DeleteOperationByIdSql = @"delete from operations where id=@id;";
    private const string GetBalanceListSql = @"select id, balance, created, updated from balances";
    
    
    private readonly LoadTestPostgresContext _dbContext;

    public BalanceDao(LoadTestPostgresContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Balance> GetById(Guid id)
    {
        await using var command = _dbContext.DataSource.CreateCommand(GetByIdSql);
        command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, id);
        await using var reader = await command.ExecuteReaderAsync();

        double? balanceAmount = null;
        DateTime balanceCreated = DateTime.MinValue;
        DateTime balanceUpdated = DateTime.MinValue;
        var opList = new List<Operation>();
        while (await reader.ReadAsync())
        {
            balanceAmount = reader.GetDouble(0);
            balanceCreated = reader.GetDateTime(1);
            balanceUpdated = reader.GetDateTime(2);
            var operationId = reader.GetGuid(3);
            Enum.TryParse<OperationType>(reader.GetString(4), out var operationType);
            var operationAmount = reader.GetDouble(5);
            var operationSequenceNumber = reader.GetInt32(6);
            var operationCreated = reader.GetDateTime(7);
            opList.Add(new Operation(operationId, operationType, operationAmount, operationSequenceNumber, operationCreated));
        }

        if (!balanceAmount.HasValue)
            throw new KeyNotFoundException($"Balance {id} was not found");

        return new Domain.Balance(id, opList, balanceCreated, balanceUpdated);
    }

    public async Task<IEnumerable<BalanceListDto>> GetBalanceList()
    {
        await using var command = _dbContext.DataSource.CreateCommand(GetBalanceListSql);
        await using var reader = await command.ExecuteReaderAsync();
        var balances = new List<BalanceListDto>();
        while (await reader.ReadAsync())
        {
            balances.Add(new BalanceListDto(
                reader.GetGuid(0),
                reader.GetDouble(1),
                reader.GetDateTime(2),
                reader.GetDateTime(3)));
        }

        return balances;
    }

    public async Task Upsert(Domain.Balance balance)
    {
        await using var connection = await _dbContext.DataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using var balanceCommand = new NpgsqlCommand(AddBalanceSql);
            balanceCommand.Transaction = transaction;
            balanceCommand.Connection = connection;
            balanceCommand.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, balance.Id);
            balanceCommand.Parameters.AddWithValue("balance", NpgsqlDbType.Double, balance.Amount);
            balanceCommand.Parameters.AddWithValue("created", NpgsqlDbType.Timestamp, balance.CreatedAt);
            balanceCommand.Parameters.AddWithValue("updated", NpgsqlDbType.Timestamp, balance.UpdatedAt);
            await balanceCommand.ExecuteNonQueryAsync();

            if (balance.GetTrimmedOperations().Any())
            {
                await using var deleteOperationCommands =  new NpgsqlBatch();
                deleteOperationCommands.Transaction = transaction;
                deleteOperationCommands.Connection = connection;

                foreach (var trimmedOperation in balance.GetTrimmedOperations())
                {
                    var batchCommand = new NpgsqlBatchCommand(DeleteOperationByIdSql);
                    batchCommand.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, trimmedOperation.Id);
                    deleteOperationCommands.Connection = connection;
                    deleteOperationCommands.BatchCommands.Add(batchCommand);
                }
            
                await deleteOperationCommands.ExecuteNonQueryAsync();
            }

            await using var operationCommands =  new NpgsqlBatch();
            operationCommands.Transaction = transaction;
            operationCommands.Connection = connection;

            foreach (var operation in balance.Operations)
            {
                var batchCommand = new NpgsqlBatchCommand(AddOperationSql);
                batchCommand.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, operation.Id);
                batchCommand.Parameters.AddWithValue("balance_id", NpgsqlDbType.Uuid, balance.Id);
                batchCommand.Parameters.Add(new NpgsqlParameter
                    { ParameterName = "type", DataTypeName = "op_type", Value = operation.Type.ToString() });
                batchCommand.Parameters.AddWithValue("amount", NpgsqlDbType.Double, operation.Amount);
                batchCommand.Parameters.AddWithValue("seq_n", NpgsqlDbType.Integer, operation.SequenceNumber);
                batchCommand.Parameters.AddWithValue("created", NpgsqlDbType.Timestamp, operation.Created);
                operationCommands.BatchCommands.Add(batchCommand);
            }


            await operationCommands.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}