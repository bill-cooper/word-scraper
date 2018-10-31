using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using Words;

namespace SampleBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //var repo = new WordRepository();

            //repo.ClearCache();

            ////noun
            //var word = repo.GetWords("жизнь").Result;
            ////adjective
            //word = repo.GetWords("глухо́й").Result;
            ////adverb 
            //word = repo.GetWords("глу́хо").Result;



            //var words = repo.GetAll().Result;
            //var wordFormWithSamples = words.SelectMany(word => word.WordForms).Where(wordForm => wordForm.Samples.Count > 0);

            //int count = 0;
            //foreach (var wordForm in wordFormWithSamples)
            //{
            //    items.AddSamples(wordForm);
            //    count++;
            //    if (count > 10) break;
            //}


           // var words = repo.GetWords("глухо́й").Result;

            //var words = repo.GetAll().Result;
            //foreach (var word in words) {
            //    if (word.WordType == "verb")
            //        items = items.BuildVerbSample(word);
            //}

            ////Example of how to build a noun sample
            //var word = repo.GetWord("стол").Result;
            //if (word.WordType == "noun")
            //    items = items.BuildNounSample(word);

            ////Example of how to build a verb sample
            //word = repo.GetWord("уви́деть").Result;
            //if (word.WordType == "verb")
            //    items = items.BuildVerbSample(word);


            //var output = JsonConvert.SerializeObject(items);
            //File.WriteAllText(@"app_data\samples.json", output);

            //var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=storagegen56;AccountKey=yYqgt8DoXJu3qzSR6n34/E/oECAmmYa4oEKxsTyrzmIn6s6F2cmKwIr1wCScm1y7lW1fsk5Q4dAHdnf9yTYLiw==;EndpointSuffix=core.windows.net");
            //var blobClient = storageAccount.CreateCloudBlobClient();
            //var container = blobClient.GetContainerReference("samples");

            //// Create the container if it doesn't already exist.
            //container.CreateIfNotExistsAsync();
            //container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            ////var result = GetBlobSasUri(container);

            //var blockBlob = container.GetBlockBlobReference("samples.json");

            //using (var fileStream = System.IO.File.OpenRead(@"app_data\samples.json"))
            //{
            //    var task = blockBlob.UploadFromStreamAsync(fileStream);
            //    task.Wait();
            //}

        }

        static string GetBlobSasUri(CloudBlobContainer container)
        {
            //Get a reference to a blob within the container.
            CloudBlockBlob blob = container.GetBlockBlobReference("exclusions.json");

            //Upload text to the blob. If the blob does not yet exist, it will be created.
            //If the blob does exist, its existing content will be overwritten.
            using (var fileStream = System.IO.File.OpenRead(@"app_data\exclusions.json"))
            {
                var task = blob.UploadFromStreamAsync(fileStream);
                task.Wait();
            }

            //Set the expiry time and permissions for the blob.
            //In this case, the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddYears(2);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }
    }

    public static class Extensions
    {

        public static List<Item> BuildNounSample(this List<Item> items, WordDefinition word)
        {

            foreach (var wordFormType in new[] { "nominative", "genitive", "dative", "accusative", "instrumental", "prepositive" })
            {

                var wordFormSingular = word.WordForms.First(f => f.FormDescription == $"singular {wordFormType}");
                var wordFormPlural = word.WordForms.First(f => f.FormDescription == $"plural {wordFormType}");
                items.Add(new Item
                {
                    Text = $@"
                        <h5><i>{wordFormSingular.FormDescription}</i></h5>
                        <h1>{wordFormSingular.StressedWord}</h1>
                        <h5><i>{wordFormPlural.FormDescription}</i></h5>
                        <h1>{wordFormPlural.StressedWord}</h1>
                        <footer class='text-center'>{string.Join(", ", word.Translations)}</footer>
                        ",
                    Word = word.Word.Word,
                    Id = wordFormSingular.Key
                });
                items.AddPause(2);
                if (wordFormType == "nominative")
                    items.ReadEnglishTranslation(word);

 
                items.PlayRussianAudios(wordFormSingular, word);
                items.PlayRussianAudios(wordFormPlural, word);

                if (wordFormSingular.Samples.Count > 0)
                {
                    items.AddSamples(wordFormSingular, word);
                }
                if (wordFormPlural.Samples.Count > 0)
                {
                    items.AddSamples(wordFormPlural, word);
                }
            }
            return items;
        }

        public static List<Item> BuildVerbSample(this List<Item> items, WordDefinition word)
        {
            items.Add(new Item
            {
                Text = $@"
                        <h5><span class='text-muted small'>{word.WordType} - infinitive form - {string.Join(", ", word.Tags)}</span></h5>
                        <h1>{word.Word.StressedWord}</h1>
                        <footer class='text-center'>{string.Join(", ", word.Translations)}</footer>
                        ",
                Word = word.Word.Word,
                Id = word.Word.Key
            });
            items.PlayRussianAudios(word);
            items.ReadEnglishTranslation(word);

            foreach (var wordForm in word.WordForms)
            {
                
                items.Add(new Item
                {
                    Text = $@"
                        <h5><span class='text-muted small'>{word.WordType} {wordForm.FormDescription} - {string.Join(", ", word.Tags)}</span></h5>
                        <h1>{wordForm.StressedWord}{(wordForm.AudioSources.Count() == 0? "<span class='text-muted small'>(no audio)</span>" : "")}</h1>
                        <footer class='text-center'>{string.Join(", ", word.Translations)}</footer>
                        ",
                    Word = word.Word.Word,
                    Id = wordForm.Key
                });

                if (wordForm.AudioSources.Count() == 0) {
                    items.AddPause(2);
                }


                items.PlayRussianAudios(wordForm, word);

                if (wordForm.Samples.Count > 0)
                {
                    items.AddSamples(wordForm, word);
                }
            }
            return items;
        }
        public static List<Item> AddPause(this List<Item> items, int pause = 2)
        {
            items.Add(new Item { Pause = pause });
            return items;
        }
        public static List<Item> AddWordIntro(this List<Item> items, WordDefinition word)
        {
            items.Add(new Item
            {
                Text = $"<h1>{word.Word.StressedWord}</h1><h2>{string.Join(", ", word.Translations)}</h2>",
                Word = word.Word.Word,
                Id = word.Key
            });
            return items;
        }
        public static List<Item> AddWordIntro(this List<Item> items, WordDefinition word, WordForm wordForm)
        {
            items.Add(new Item
            {
                Text = $"<h1>{wordForm.StressedWord}</h1><h2>{string.Join(", ", word.Translations)}</h2>",
                Word = word.Word.Word,
                Id = wordForm.Key
            });
            return items;
        }
        public static List<Item> ReadEnglishTranslation(this List<Item> items, WordDefinition word)
        {
            items.Add(new Item
            {
                Read = word.Translations[0]
            });
            return items;
        }
        public static List<Item> ReadEnglishFormDescription(this List<Item> items, WordForm wordForm, WordDefinition word)
        {
            items.Add(new Item
            {
                Read = wordForm.FormDescription + " form"
            });
            return items;
        }

        public static List<Item> PlayRussianAudios(this List<Item> items, WordDefinition word)
        {

            for (int i = 0; i < word.Word.AudioSources.Count; i++)
            {
                items.PlayRussianAudio(word, i);
            }
            return items;
        }
        public static List<Item> PlayRussianAudio(this List<Item> items, WordDefinition word, int index = 0)
        {
            var formDescription = word.Word.FormDescription ?? "";
            items.Add(new Item
            {
                Text = $@"
                        <h5><span class='text-muted small'>{word.WordType} {formDescription} - {string.Join(", ", word.Tags)}</span></h5>
                        <h1>{word.Word.StressedWord}</h1>
                        <footer class='text-center'>{string.Join(", ", word.Translations)}</footer>
                        ",
                Audio = word.Word.AudioSources[index].Uri.AbsoluteUri,
                Word = word.Word.Word,
                Id = word.Word.AudioSources[index].Key
            });
            return items;
        }
        public static List<Item> PlayRussianAudios(this List<Item> items, WordForm wordForm, WordDefinition word) {

            for (int i = 0; i < 3 && i < wordForm.AudioSources.Count; i++)
            {
                items.PlayRussianAudio(wordForm, word, i).AddPause(1);
            }
            return items;
        }
        public static List<Item> PlayRussianAudio(this List<Item> items, WordForm wordForm, WordDefinition word, int index = 0)
        {
            items.Add(new Item
            {
                Text = $@"
                        <h5><span class='text-muted small'>{word.WordType} {wordForm.FormDescription} - {string.Join(", ", word.Tags)}</span></h5>
                        <h1>{wordForm.StressedWord}</h1>
                        <footer class='text-center'>{string.Join(", ", word.Translations)}</footer>
                        ",
                Audio = wordForm.AudioSources[index].Uri.AbsoluteUri,
                Word = wordForm.Word,
                Id = wordForm.AudioSources[index].Key
            });
            return items;
        }

        public static List<Item> AddSamples(this List<Item> items, WordForm wordForm, WordDefinition word)
        {

            for (int i = 0; i < wordForm.Samples.Count; i++)
            {
                items.AddSample(wordForm, word, i).AddPause(2);
            }
            return items;
        }
        public static List<Item> AddSample(this List<Item> items, WordForm wordForm, WordDefinition word, int index = 0)
        {
            items.Add(new Item
            {
                Text = $@"  <h5>{wordForm.StressedWord} <i>({word.Translations.FirstOrDefault()})</i> <span class='text-muted small'>{wordForm.FormDescription}</span></h5>
                            <h1>{wordForm.Samples[index].SampleText.Replace(wordForm.Word, $"<b><u>{wordForm.StressedWord}</u></b>")}</h1>
                            <footer class='text-center'>{wordForm.Samples[index].Translation}</footer>
                        ",
                Audio = wordForm.Samples[index].AudioSource.Uri.AbsoluteUri,
                Word = wordForm.Word,
                Id = wordForm.Samples[index].Key
            });

            items.Add(new Item
            {
                Text = $@"  <h5>{wordForm.StressedWord} <i>({word.Translations.FirstOrDefault()})</i> <span class='text-muted small'>{wordForm.FormDescription}</span></h5>
                            <h1>{wordForm.Samples[index].SampleText.Replace(wordForm.Word, $"<b><u>{wordForm.StressedWord}</u></b>")}</h1>
                            <footer class='text-center'>{wordForm.Samples[index].Translation}</footer>
                        ",
                Read = wordForm.Samples[index].Translation,
                Word = wordForm.Word,
                Id = wordForm.Samples[index].Key
            });
            items.Add(new Item
            {
                Text = $@"  <h5>{wordForm.StressedWord} <i>({word.Translations.FirstOrDefault()})</i> <span class='text-muted small'>{wordForm.FormDescription}</span></h5>
                            <h1>{wordForm.Samples[index].SampleText.Replace(wordForm.Word, $"<b><u>{wordForm.StressedWord}</u></b>")}</h1>
                            <footer class='text-center'>{wordForm.Samples[index].Translation}</footer>
                        ",
                Audio = wordForm.Samples[index].AudioSource.Uri.AbsoluteUri,
                Word = wordForm.Word,
                Id = wordForm.Samples[index].Key
            });
            return items;
        }
    }
}
