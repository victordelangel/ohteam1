using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChangeFeedFunctions.Models;
using Newtonsoft.Json;

namespace ChangeFeedFunctions
{
    public static class Analytics
    {
        [FunctionName("Analytics")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "movies",
            collectionName: "OrderEvents",
            ConnectionStringSetting = "CosmosDBConnectionString",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionName = "movie-analytics-leases")]IReadOnlyList<Document> input,
            [EventHub("orders", Connection = "EHUBEUROPE")] IAsyncCollector<Order> orderEvents,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);

                foreach (var doc in input)
                {
                    var order = JsonConvert.DeserializeObject<Order>(doc.ToString());
                    await orderEvents.AddAsync(order);
                }
            }
        }
    }
}
