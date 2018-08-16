using Google.Apis.Services;
using Google.Apis.Translate.v2;
using System.Threading.Tasks;

namespace Words
{
    public interface ITranslator {
        Task<string> Translate(string phrase);
    }
    public class Translator: ITranslator
    {
        private const string ApplicationName = "Project Name";
        private readonly ISecretProvider _secretProvider;

        public Translator(ISecretProvider secretProvider)
        {
            _secretProvider = secretProvider;
        }
        public async Task<string> Translate(string phrase)
        {
            var apiKey = _secretProvider.GetSecret("GoogleTranslateApiKey/bce3d012310e4196a430a8d731990f9c");
            using (var service = new TranslateService(new BaseClientService.Initializer { ApiKey = apiKey, ApplicationName = ApplicationName }))
            {
                var response = await service.Translations.List(new[] { phrase }, "en").ExecuteAsync();

                if (response.Translations.Count > 0)
                    return CleanTranslation(response.Translations[0].TranslatedText);

                return string.Empty;
            }
        }
        private string CleanTranslation(string phrase) {
            return phrase
                .Replace("&#39;", "'")
                .Replace("&quot;", "\"");
        }
    }


}
