using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlowII.IntegrationTests
{
    public class IntegrationTestBase : IAsyncLifetime
    {
        protected HttpClient _client = null;
        private readonly SqlServerContainerFixture _fixture;

        public IntegrationTestBase(SqlServerContainerFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.client;
           
        }
        protected HttpClient Client => _fixture.client;

        public async Task InitializeAsync() => await _fixture.ResetDatabaseAsync();



        public Task DisposeAsync() =>  Task.CompletedTask;
        
    }
}
