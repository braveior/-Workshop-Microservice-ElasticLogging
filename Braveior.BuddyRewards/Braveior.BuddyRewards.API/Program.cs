using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Entities;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;

namespace Braveior.BuddyRewards.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureLogging();
            DB.InitAsync("buddyrewards", GetEnvironmentVariable("mongoip"), 27017).GetAwaiter().GetResult();
            CreateHostBuilder(args).Build().Run();
        }

        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name.ToLower()) ?? Environment.GetEnvironmentVariable(name.ToUpper());
        }

        private static void ConfigureLogging()
        {
            var configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .Build();

            Log.Logger = new LoggerConfiguration()
              .WriteTo.Console()
              .WriteTo.Elasticsearch(ConfigureElasticSink())
              .ReadFrom.Configuration(configuration)
              .CreateLogger();
        }
        private static ElasticsearchSinkOptions ConfigureElasticSink()
        {
            return new ElasticsearchSinkOptions(new Uri("http://elasticsearchoss-master:9200"))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"Braveior-BuddyRewards-{DateTime.UtcNow:yyyy-MM}"
            };
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog(Log.Logger);
    }
}
