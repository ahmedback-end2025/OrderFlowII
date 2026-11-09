using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        public async Task DisposeAsync()
        {
            await _sqlServer.DisposeAsync();
            await factory.DisposeAsync();
            client.Dispose();

        }

        public async Task InitializeAsync()
        {
            await _sqlServer.StartAsync();
            factory = new CustomWebApplicationFactory(_sqlServer.GetConnectionString());
            client = factory.CreateClient();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            await db.Database.MigrateAsync();

        }
    }
}
