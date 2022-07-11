using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IoTHubQuery
{
    public static class IoTHubQuery
    {
        private static string s_connectionString = "IOTHUB_CONNECTION_STRING";//Environment.GetEnvironmentVariable("IOTHUB_CONNECTION_STRING")"";

        [FunctionName("IoTHubQuery")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string sql = req.Query["query"];

            if (string.IsNullOrEmpty(sql))
            {
                sql = "SELECT DeviceId, connectionState, LastActivityTime FROM devices WHERE status = 'enabled' AND connectionState = 'Disconnected'";
            }

            using var registryManager = RegistryManager.CreateFromConnectionString(s_connectionString);
            var query = registryManager.CreateQuery(sql, 100);

            string responseMessage = "{\"data\":[";

            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsJsonAsync();
                foreach (var twin in page)
                {
                    responseMessage += $"{twin},";
                }
            }

            responseMessage = responseMessage.Remove(responseMessage.LastIndexOf(',')) + "]}";

            return new OkObjectResult(responseMessage);
        }
    }
}
