using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace netisolated8_openai
{
    public class TestOpenAI
    {
        private readonly ILogger<TestOpenAI> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TestOpenAI(ILogger<TestOpenAI> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [Function(nameof(TestOpenAI))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var cli = _httpClientFactory.CreateClient();

            var httpResponse = await cli.PostAsJsonAsync("http://localhost:3500/v1.0/bindings/openai", new
            {
                operation = "chat-completion",
                data = new
                {
                    deploymentId = "gpt-4o",
                    messages = new[] { 
                        new { 
                            role = "system", 
                            message = "You are a chatbot." 
                        },
                        new
                        {
                            role = "user",
                            message = $"Find a suitable short definition for this city: {req.Query["city"]}.\r\n" +
                                      $"Respond in language specified by this ISO code: {req.Query["lang"]}"
                        }
                    },
                    maxTokens = 200
                }
            });

            var responseText = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonDocument.Parse(responseText);

            _logger.LogInformation("Response from OpenAI: {openai}", responseText);

            return new ContentResult
            {
                Content = response.RootElement[0].GetProperty("message").GetProperty("content").GetString(),
                ContentType = "text/plain"
            };
        }
    }
}
