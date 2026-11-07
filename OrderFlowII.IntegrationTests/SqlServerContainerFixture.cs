using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace OrderFlowII.IntegrationTests
{
    public sealed class SqlServerContainerFixture : IAsyncLifetime
    {
        
        private readonly MsSqlContainer _sqlServer =
            new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

        public async Task DisposeAsync()
        {
            await _sqlServer.DisposeAsync();   
        }

        public async Task InitializeAsync()
        {
            await _sqlServer.StartAsync();
        }
    }
}
