using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;

namespace CodeCodeChallenge.Tests.Integration.Helpers
{
    public class TestServer : IDisposable, IAsyncDisposable
    {
        public WebApplicationFactory<Program> ApplicationFactory { get; init; }

        public TestServer()
        {
            ApplicationFactory = new WebApplicationFactory<Program>();
        }

        public HttpClient NewClient()
        {
            return ApplicationFactory.CreateClient();
        }


        public ValueTask DisposeAsync()
        {
            return ((IAsyncDisposable)ApplicationFactory).DisposeAsync();
        }

        public void Dispose()
        {
            ((IDisposable)ApplicationFactory).Dispose();
        }
    }
}
