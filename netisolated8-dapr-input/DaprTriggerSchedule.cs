namespace NetIsolated8DaprInput;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
using Microsoft.Extensions.Logging;

public static class DaprTriggerSchedule
{
    [Function("DaprTriggerSchedule")]
    public static void Run(
        [DaprBindingTrigger(BindingName = "daprschedule")] string payload,
        FunctionContext functionContext)
    {
        var log = functionContext.GetLogger("DaprTriggerSchedule");
        log.LogInformation("Azure function triggered by Dapr Input Binding.");
        log.LogInformation($"Input payload: {payload}");
    }
}
