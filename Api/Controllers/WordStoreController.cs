using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Words;

namespace Api.Controllers
{
    public class WordStoreController : Controller
    {
        private readonly ILogger _logger;
        private readonly IWordRepository _wordRepository;
        public WordStoreController(ILogger<WordStoreController> logger, IWordRepository wordRepository) {
            _logger = logger;
            _wordRepository = wordRepository;
        }
        // GET api/values
        [HttpGet]
        [Route("api/wordstore/keys")]
        public IEnumerable<string> GetKeys()
        {
            return _wordRepository.GetCacheKeys();
        }

        [HttpGet]
        [Route("api/wordstore/keys/{key}")]
        public async Task<IEnumerable<WordDefinition>> GetKey(string key)
        {
            return await _wordRepository.GetWordFromCache(key);
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
