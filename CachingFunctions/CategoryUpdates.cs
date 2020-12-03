using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CachingFunctions.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CachingFunctions
{
    public static class CategoryUpdates
    {
        [FunctionName("CategoryUpdates")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "movies",
                collectionName: "Category",
                ConnectionStringSetting = "CosmosDBConnectionString",
                CreateLeaseCollectionIfNotExists = true,
                LeaseCollectionName = "category-update-leases")]IReadOnlyList<Document> input, 
            [CosmosDB(
                databaseName: "movies",
                collectionName: "MaterializedViews",
                ConnectionStringSetting = "CosmosDBConnectionString",
                Id = "Categories",
                PartitionKey = "Categories"
            )] CategoriesItem categories,
            [CosmosDB(
                databaseName: "movies",
                collectionName: "MaterializedViews",
                ConnectionStringSetting = "CosmosDBConnectionString"
            )] IAsyncCollector<object> docs,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
               log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);

                // Find the updated category in the materialized view
                foreach (var doc in input)
                {                    
                    var updatedCategory = JsonConvert.DeserializeObject<Category>(doc.ToString());

                    // Find the category that needs to be updated 
                    var c = categories.Details.FirstOrDefault(x => x.CategoryId == updatedCategory.CategoryId);
                    c.CategoryName = updatedCategory.CategoryName;
                }

                await docs.AddAsync(categories);

            }
        }
    }
}
