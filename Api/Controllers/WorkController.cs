using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Words;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    public class WorkController : Controller
    {
        private readonly ILogger _logger;
        private readonly IWordRepository _wordRepository;
        private readonly IWordBank _wordBank;
        public WorkController(ILogger<ValuesController> logger) {
            _logger = logger;
        }


        // POST api/values
        [HttpPost]
        public string Post([FromBody]WorkItem workItem)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=wordapp123;AccountKey=TCc6OhzaZVvP1F9NO5FRHGGykmIlIsprigNqP7Ud+6KrdqUNSaVRWtOIFLSmtiqmy1RTpMRQg8vAsFOuWJdXmA==;EndpointSuffix=core.windows.net");
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("work");
            queue.CreateIfNotExistsAsync();
            CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(workItem));
            queue.AddMessageAsync(message);

            return "work created";
        }


    }
}
