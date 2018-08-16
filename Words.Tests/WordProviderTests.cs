using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RussianWordScraper;
using System.Linq;
using System.Threading.Tasks;


namespace Words.Tests
{
    [TestClass]
    public class WordProviderTests
    {
        [TestMethod]
        public async Task ProvideNounAsync()
        {
            var translator = new Mock<ITranslator>();
            translator.Setup(t => t.Translate(It.IsAny<string>())).Returns(Task.FromResult("-- translation --"));
            var provider = new WordProvider(translator.Object);
            var words = await provider.GetWords("жизнь", getSamples: false);

            Assert.IsTrue(words.Count() == 1, "Expected 1 words in the collection");
            Assert.IsTrue(words.First().WordType == "noun");
            Assert.IsTrue(words.First().WordForms.Count == 12);
            Assert.IsTrue(words.First().Tags.Contains("inanimate"));
            Assert.IsTrue(words.First().Tags.Contains("female"));
            Assert.IsTrue(words.First().Rank == 69);

        }

        [TestMethod]
        public async Task ProvideMultiFormWordAsync()
        {
            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2();
            cert.Import(@"d:\temp\RussianWordApp.pfx", "1ma_Bi11",System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.DefaultKeySet);
            var translator = new Mock<ITranslator>();
            translator.Setup(t => t.Translate(It.IsAny<string>())).Returns(Task.FromResult("-- translation --"));
            var provider = new WordProvider(translator.Object);
            var words = await provider.GetWords("знать");

            Assert.IsTrue(words.Count() == 2, "Expected 2 words in the collection");
            Assert.IsTrue(words.First().WordType == "verb", $"For the value of word.WordType, 'verb' was expected, but value is '{words.First().WordType}'");
            Assert.IsTrue(words.First().WordForms.Count == 18, $"Expected words.First().WordForms.Count == 18, but value is {words.First().WordForms.Count}");
            Assert.IsTrue(words.First().Tags.Contains("imperfective"));
            Assert.IsTrue(words.First().Tags.Count() == 1);
            Assert.IsTrue(words.First().Rank == 40);
            Assert.IsTrue(words.First().Translations.Count == 9);
            Assert.IsTrue(words.First().Translations.Contains("know"));

            Assert.IsTrue(words.Last().WordType == "noun", $"For the value of word.WordType, 'noun' was expected, but value is '{words.First().WordType}'");
            Assert.IsTrue(words.Last().WordForms.Count ==  6);
            Assert.IsTrue(words.Last().Tags.Contains("inanimate"));
            Assert.IsTrue(words.Last().Tags.Contains("female"));
            Assert.IsTrue(words.Last().Rank == 40);



        }
    }
}
