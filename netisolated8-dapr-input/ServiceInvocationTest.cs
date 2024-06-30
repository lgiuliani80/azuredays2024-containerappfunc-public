namespace NetIsolated8DaprInput;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
using Microsoft.Extensions.Logging;

public static class ServiceInvocationTest
{
    [Function("createorder")]
    public static string Run(
        [DaprServiceInvocationTrigger] string payload,
        FunctionContext functionContext)
    {
        var log = functionContext.GetLogger("ServiceInvocaion");
        log.LogInformation("Azure function triggered by Dapr Service Invocation Trigger.");
        log.LogInformation($"Dapr service invocation trigger payload: {payload}");

        return "Completed " + DateTime.Now;
    }
}
