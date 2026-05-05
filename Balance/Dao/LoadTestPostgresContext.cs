using Balance.Domain;
using Npgsql;

namespace Balance.Dao;

public class LoadTestPostgresContext : IAsyncDisposable
{
    public NpgsqlDataSource DataSource { get; }

    public LoadTestPostgresContext(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        var connectionString = configuration.GetConnectionString(nameof(LoadTestPostgresContext));
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseLoggerFactory(loggerFactory);
        dataSourceBuilder.MapEnum<OperationType>("op_type");
        DataSource = dataSourceBuilder.Build();
    }
    
    public async ValueTask DisposeAsync()
    {
        await DataSource.DisposeAsync();
    }
}