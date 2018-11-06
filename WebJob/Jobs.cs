using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Words;

namespace WebJob
{
    public class Functions
    {
        private readonly IWordRepository _wordRepository;
        private readonly IWordBank _wordBank;
        public Functions(IWordRepository wordRepository, IWordBank wordBank)
        {
            _wordRepository = wordRepository;
            _wordBank = wordBank;
        }
        public async void ProcessWorkItem([QueueTrigger("work")] WorkItem workItem, ILogger logger)
        {
            logger.LogInformation($"Processing work item: worktype: {workItem.WorkType}, details: {workItem.Details}");

            if (workItem.WorkType == "fetch-word")
            {
                logger.LogInformation($"Handling work item type: {workItem.WorkType}");

                var repoWords = await _wordRepository.GetWords(workItem.Details);
                foreach (var repoWord in repoWords)
                {
                    logger.LogInformation($"word fetched: {repoWord.Word} {repoWord.Key}");
                }

            }
            else if (workItem.WorkType == "fetch-word-list")
            {
                logger.LogInformation($"Handling work item type: {workItem.WorkType}");

                var words = await _wordBank.GetWordByRank(int.Parse(workItem.Details));

                foreach (var word in words.Where(w => w.Length > 2))
                {
                    await Task.Delay(10000);
                    var fetchWordItem = new WorkItem {
                        WorkType = "fetch-word",
                        Details = word
                    };
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=wordapp123;AccountKey=TCc6OhzaZVvP1F9NO5FRHGGykmIlIsprigNqP7Ud+6KrdqUNSaVRWtOIFLSmtiqmy1RTpMRQg8vAsFOuWJdXmA==;EndpointSuffix=core.windows.net");
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                    CloudQueue queue = queueClient.GetQueueReference("work");
                    CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(fetchWordItem));
                    await queue.AddMessageAsync(message);

                }

            }
            else {
                logger.LogInformation($"Unknown work item type.  No action taken");
            }

        }
    }
}
