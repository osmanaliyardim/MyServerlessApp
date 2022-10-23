using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MyServerlessApp;

public static class MyFirstServerlessApp
{
    #region With an attribute
    /* 
    This is the fuction name on Azure (MyFirstServerlessApp).
    It enqueues the incoming request from HttpTrigger with ServiceBusBindings.
    */
    [FunctionName("MyFirstServerlessApp")]
    [return: ServiceBus("ServerlessQueue", Connection = "ServiceBusConnection")]
    public static string Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string name = req.Query["name"];

        return name;
    }
    #endregion  

    #region With a parameter in ServiceBus
    // This is the alternative way of the above (Output Binding version).
    [FunctionName("MyFirstServerlessApp2")]
    public static async Task<IActionResult> Run2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [ServiceBus("ServerlessQueue2", Connection = "ServiceBusConnection2")] IAsyncCollector<dynamic> outputServiceBus,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string name = req.Query["name"];

        await outputServiceBus.AddAsync(name);

        return new OkResult();
    }
    #endregion

    #region Input Binding
    // This is the alternative way of the above (Input Binding version).
    [FunctionName("MyFirstServerlessApp3")]
    public static void Run3(
        [QueueTrigger("ServerlessQueue3", Connection = "ConnectionString")] ToDoItem queueToDoItem,
        [CosmosDB(
            databaseName: "ToDoItems",
            collectionName: "Items",
            ConnectionStringSetting = "CosmosDBConnectionString",
            Id = "{Id}",
            PartitionKey = "{PartValue}")] ToDoItem toDoItem,
        ILogger log)
    {
        log.LogInformation($"C# QueueTrigger function processed Id={queueToDoItem?.Id} Key={queueToDoItem?.PartValue}");

        if(toDoItem == null)
        {
            log.LogInformation("ToDo item not found");
        }
        else
        {
            log.LogInformation($"Found ToDo item, PartValue={toDoItem.PartValue}");
        }
    }

    // Incoming json model example
    public class ToDoItem
    {
        public Guid Id { get; set; }

        public string PartValue { get; set; }
    }
    #endregion
}