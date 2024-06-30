namespace NetInprocess6DaprOutput;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Dapr;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class DaprOutputBindingTest
{
    [FunctionName(nameof(DaprOutputBindingTest))]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        [DaprBinding(BindingName = "pgdb")] IAsyncCollector<JObject> pgdb,
        ILogger log)
    {
        log.LogInformation("Function invoked");

        JObject jo = new()
        {
            ["operation"] = "exec",
            ["data"] = "-",
            ["metadata"] = new JObject()
        };
        jo["metadata"]["sql"] = "INSERT INTO executions (exec_date, host_name) VALUES ($1, $2)";
        jo["metadata"]["params"] = $"[ \"{DateTime.Now:o}\", \"{Environment.MachineName}\" ]";

        await pgdb.AddAsync(jo);

        return new OkObjectResult("Done");
    }
}
