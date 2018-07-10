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
                .DefaultIndex(defaultIndex);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);

            CreateIndex(client, defaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<Post>(m => m
                    .Ignore(p => p.IsPublished)
                    .PropertyName(p => p.ID, "id")
                )
                .DefaultMappingFor<Comment>(m => m
                    .Ignore(c => c.Email)
                    .Ignore(c => c.IsAdmin)
                    .PropertyName(c => c.ID, "id")
                );
        }

        private static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.CreateIndex(indexName, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .CharFilters(cf => cf
                            .Mapping("programming_language", mcf => mcf
                                .Mappings(
                                    "c# => csharp",
                                    "C# => Csharp"
                                )
                            )
                        )
                        .Analyzers(an => an
                            .Custom("content", ca => ca
                                .CharFilters("html_strip", "programming_language")
                                .Tokenizer("standard")
                                .Filters("standard", "lowercase", "stop")
                            )
                            .Custom("categories", ca => ca
                                .CharFilters("programming_language")
                                .Tokenizer("standard")
                                .Filters("standard", "lowercase")
                            )
                        )
                    )
                )
                .Mappings(m => m
                    .Map<Post>(x => x
                        .AutoMap()
                        .Properties(p => p
                            .Text(t => t
                                .Name(n => n.Title)
                                .Boost(3)
                            )
                            .Text(t => t
                                .Name(n => n.Content)
                                .Analyzer("content")
                                .Boost(1)
                            )
                            .Text(t => t
                                .Name(n => n.Excerpt)
                                .Boost(2)
                            )
                            .Text(t => t
                                .Name(n => n.Categories)
                                .Analyzer("categories")
                                .Boost(2)
                            )
                            .Nested<Comment>(np => np
                                .AutoMap()
                                .Name(nn => nn.Comments)
                                .Properties(cp => cp
                                    .Text(t => t
                                        .Name(n => n.Author)
                                        .Boost(0.6)
                                    )
                                    .Text(t => t
                                        .Name(n => n.Content)
                                        .Boost(0.5)
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}