using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChangeFeedFunctions.Models;
using System.Linq;

namespace ChangeFeedFunctions
{
    public static class TopTenMoviesMaterializedView
    {
        [FunctionName("TopTenMoviesMaterializedView")]
        public static void Run([CosmosDBTrigger(
            databaseName: "movies",
            collectionName: "OrderEvents",
            ConnectionStringSetting = "CosmosDBConnectionString",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionName = "top-ten-movie-leases")]IReadOnlyList<Document> input,
            [CosmosDB(
                databaseName: "movies",
                collectionName: "MaterializedViews",
                ConnectionStringSetting = "CosmosDBConnectionString",
                Id = "MoviesBySales",
                PartitionKey = "MoviesBySales")]TopMovies topmovies,
            [CosmosDB(
                databaseName: "movies",
                collectionName: "MaterializedViews",
                ConnectionStringSetting = "CosmosDBConnectionString")] IAsyncCollector<object> docs, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
                foreach (var doc in input)
                {
                    var order = JsonConvert.DeserializeObject<Order>(doc.ToString());
                    foreach (OrderDetail detail in order.Details)
                    {
                        foreach (var movie in topmovies.Details.Where(w => w.ProductId == detail.ProductId))
                        {
                            movie.SalesQuantity = movie.SalesQuantity + detail.Quantity;
                        }
                    }
                }

                TopMovies toptenmovies = new TopMovies();
                toptenmovies.id = "TopTenMovies";

                toptenmovies.Details = topmovies.Details.OrderBy(w => w.SalesQuantity).Take(10).ToList();

                docs.AddAsync(topmovies);
                docs.AddAsync(toptenmovies);
            }
        }
    }
}
