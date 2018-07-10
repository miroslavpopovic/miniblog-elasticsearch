using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniBlogElasticsearch.Models;
using Nest;

namespace MiniBlogElasticsearch
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(
            this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["elasticsearch:url"];
            var defaultIndex = configuration["elasticsearch:index"];

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex)
                .DefaultMappingFor<Post>(m => m
                    .Ignore(p => p.IsPublished)
                    .PropertyName(p => p.ID, "id")
                )
                .DefaultMappingFor<Comment>(m => m
                    .Ignore(c => c.Email)
                    .Ignore(c => c.IsAdmin)
                    .PropertyName(c => c.ID, "id")
                );

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
        }
    }
}