using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RussianWordScraper;
using System.Linq;
using System.Threading.Tasks;


namespace Words.Tests
{
    [TestClass]
    public class SentenceProviderTests
    {
        [TestMethod]
        public async Task TestTatoebaSentenceProviderAsync()
        {

            var word =  new WordForm { Word = "учиться" };

            var sentenceProvider = new TatoebaSentenceProvider();
            var sentences = await sentenceProvider.GetSentences(word);

            //Assert.IsTrue(words.Count() == 1, "Expected 1 words in the collection");
            //Assert.IsTrue(words.First().WordType == "noun");
            //Assert.IsTrue(words.First().WordForms.Count == 12);
            //Assert.IsTrue(words.First().Tags.Contains("inanimate"));
            //Assert.IsTrue(words.First().Tags.Contains("female"));
            //Assert.IsTrue(words.First().Rank == 69);

        }


    }
}
