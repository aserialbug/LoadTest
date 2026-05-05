using Balance.Domain;
using Balance.Interfaces;
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
                                        from balances b left join operations o on b.id = o.balance_id where b.id = @id";
    private const string AddBalanceSql = "insert into balances (id, balance, created, updated, version) values" +
                                         " (@id, @balance, @created, @updated, @version)";
    
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
        
        if (!await reader.ReadAsync())
        {
            throw new ArgumentException($"Balance {id} was not found");
        }

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
            var operationType = reader.GetFieldValue<OperationType>(4);
            var operationAmount = reader.GetDouble(5);
            var operationSequenceNumber = reader.GetInt32(6);
            var operationCreated = reader.GetDateTime(7);
            opList.Add(new Operation(operationId, operationType, operationAmount, operationSequenceNumber, operationCreated));
        }

        if (!balanceAmount.HasValue)
            throw new KeyNotFoundException($"Balance {id} was not found");

        return new Domain.Balance(id, opList, balanceCreated, balanceUpdated);
    }

    public async Task Add(Domain.Balance balance)
    {
        await using var command = _dbContext.DataSource.CreateCommand(AddBalanceSql);
        command.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, balance.Id);
        command.Parameters.AddWithValue("balance", NpgsqlDbType.Double, balance.Amount);
        command.Parameters.AddWithValue("created", NpgsqlDbType.Timestamp, balance.CreatedAt);
        command.Parameters.AddWithValue("updated", NpgsqlDbType.Timestamp, balance.UpdatedAt);
        command.Parameters.AddWithValue("version", NpgsqlDbType.Integer, balance.Version);

        await command.ExecuteNonQueryAsync();
    }
}