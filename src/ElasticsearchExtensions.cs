using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace MiniBlogElasticsearch
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(
            this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new ConnectionSettings(new Uri(configuration["elasticsearch:url"]))
                .DefaultIndex(configuration["elasticsearch:index"]);
            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
        }
    }
}