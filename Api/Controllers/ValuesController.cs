using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Words;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ILogger _logger;
        private readonly IWordRepository _wordRepository;
        private readonly IWordBank _wordBank;
        public ValuesController(ILogger<ValuesController> logger, IWordRepository wordRepository, IWordBank wordBank) {
            _logger = logger;
            _wordRepository = wordRepository;
            _wordBank = wordBank;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("Request recieved...");

            
            Task.Run(async () =>
            {
                _logger.LogInformation("Running Task...");
                var words = await _wordBank.GetWordByRank(50);

                foreach (var word in words.Where(w => w.Length > 2))
                {
                    await _wordRepository.GetWords(word);
                }
                _logger.LogInformation("Task Complete...");
            }).Wait();



            return new string[] { "value1", "value2", "value3" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            System.Diagnostics.Trace.TraceError("TraceError Request recieved for get...");
            System.Diagnostics.Trace.TraceInformation("TraceInformation Request recieved for get...");
            System.Diagnostics.Trace.TraceWarning(" TraceWarningRequest recieved for get...");
            System.Diagnostics.Trace.WriteLine("WriteLine Request recieved for get...");
            return "something different";

            
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
