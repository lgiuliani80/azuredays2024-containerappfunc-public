using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
using Microsoft.Extensions.Logging;

namespace NetIsolated8DaprInput
{
    public class HttpFuncTest
    {
        private readonly ILogger<HttpFuncTest> _logger;

        public HttpFuncTest(ILogger<HttpFuncTest> logger)
        {
            _logger = logger;
        }

        [Function(nameof(HttpFuncTest))]
        [DaprStateOutput("azstatestore", Key = "lastExecution")]

        public string Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("HttpFuncTest called");
            //return new OkObjectResult("Welcome to Azure Functions!");
            return "{ \"value\": \"" + DateTime.Now.ToString("O") + "\" }";
        }
    }
}
