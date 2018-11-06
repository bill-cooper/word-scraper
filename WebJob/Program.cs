using System;
using System.Threading.Tasks;
using AngleSharp.Network;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RussianWordScraper.Document;
using Words;
using Words.Document;

namespace WebJob
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices()
                    .AddAzureStorage();
                })
                .ConfigureLogging((context, b) =>
                {
                    b.SetMinimumLevel(LogLevel.Debug);
                    b.AddConsole();


                })
                .ConfigureServices(services =>
                {
                    // add some sample services to demonstrate job class DI
                    services.AddTransient<Functions, Functions>();
                    services.AddTransient<IWordRepository, WordRepository>();
                    services.AddScoped<IWordBank, WordBank>();
                    services.AddScoped<ISentenceProvider, TatoebaSentenceProvider>();
                    services.AddScoped<IWordProvider, WordProvider>();
                    services.AddScoped<ISecretProvider, SecretProvider>();
                    services.AddScoped<ITranslator, Translator>();
                    services.AddScoped<IRequester, CacheEnabledRequester>();
                    services.AddTransient<ContentSegment, ContentSegment>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
