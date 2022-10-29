using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Microsoft.Azure.Cosmos;
using System.Collections.Generic;


namespace Imronet.Function
{
    public static class TodoApp
    {
        [FunctionName("GetTodos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);

            // New instance of CosmosClient class using a connection string
            using CosmosClient _client = new(
                connectionString: connectionString
            );

            Container _container = _client.GetDatabase("TestDB").GetContainer("todos");
            

            List<Todo> todoDataArray = new List<Todo>();

            // Build query definition
            var parameterizedQuery = new QueryDefinition(
                query: "SELECT * FROM todos c"
            );
               

            // Query multiple items from container
            using FeedIterator<Todo> todoData = _container.GetItemQueryIterator<Todo>(
                queryDefinition: parameterizedQuery
            );

            // Iterate query result pages
            while (todoData.HasMoreResults)
            {
                FeedResponse<Todo> todos = await todoData.ReadNextAsync();

                // Iterate query results
                foreach (Todo todo in todos)
                {
                    todoDataArray.Add(todo);
                }
            }

            return new OkObjectResult(todoDataArray);
        }

        [FunctionName("GetTodo")]
        public static async Task<IActionResult> Task(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);                   
                   
             // New instance of CosmosClient class using a connection string
            using CosmosClient _client = new(
                connectionString: connectionString
            );

            Container _container = _client.GetDatabase("TestDB").GetContainer("todos");

            string id = req.Query["id"]; // get todo id from the url query 


            // Build query definition
            var parameterizedQuery = new QueryDefinition(
                query: "SELECT * FROM todos c WHERE c.id = @id"
            ).WithParameter("@id", id);


            // Query multiple items from container
            using FeedIterator<Todo> todoData = _container.GetItemQueryIterator<Todo>(
                queryDefinition: parameterizedQuery
            );

            FeedResponse<Todo> todo = await todoData.ReadNextAsync();
        
            return new OkObjectResult(todo);
        }
    }


    public class Todo {
        public String id {get; set;}
        public String title  {get; set;}
        public String description  {get; set;}

        public Boolean isCompleted  {get; set;}

    }
}
