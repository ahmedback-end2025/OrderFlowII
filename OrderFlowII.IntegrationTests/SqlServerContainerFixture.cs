using Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;

namespace OrderFlowII.IntegrationTests
{
    public sealed class SqlServerContainerFixture : IAsyncLifetime
    {

        private readonly MsSqlContainer _sqlServer =
            new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

        public string ConnectionString => _sqlServer.GetConnectionString();

        public CustomWebApplicationFactory factory = default!;

        public HttpClient client { get; set; } = null;

        private Respawner _respawner = default;

        public async Task InitializeAsync()
        {
            await _sqlServer.StartAsync();
            factory = new CustomWebApplicationFactory(_sqlServer.GetConnectionString());
            client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            await db.Database.MigrateAsync();

            //respawner
            await using var connection =
            new SqlConnection(ConnectionString);

            await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,

                TablesToIgnore = new Table[]
              {
                    "__EFMigrationsHistory"
              }
            });

        }


        public async Task ResetDatabaseAsync()
        {
            await using var connection =
                new SqlConnection(ConnectionString);

            await connection.OpenAsync();

            await _respawner.ResetAsync(connection);
        }

        public async Task DisposeAsync()
        {
            await _sqlServer.DisposeAsync();
            await factory.DisposeAsync();
            client.Dispose();

        }

    }
}