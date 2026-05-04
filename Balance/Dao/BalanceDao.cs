using Balance.Interfaces;
using NpgsqlTypes;

namespace Balance.Dao;

public class BalanceDao : IBalanceDao
{
    private const string GetByIdSql = "select balance, created, updated, version from balances where id = @id";
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
        
        var balance = reader.GetDouble(0);
        var created = reader.GetDateTime(1);
        var updated = reader.GetDateTime(2);
        var version = reader.GetInt32(3);

        return new Domain.Balance(id, balance, created, updated, version);
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