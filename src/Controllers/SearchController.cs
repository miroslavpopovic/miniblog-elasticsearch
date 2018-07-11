using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MiniBlogElasticsearch.Models;
using Nest;

namespace MiniBlogElasticsearch.Controllers
{
    public class SearchController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly IElasticClient _elasticClient;
        private readonly IOptionsSnapshot<BlogSettings> _settings;

        public SearchController(IBlogService blogService, IElasticClient elasticClient, IOptionsSnapshot<BlogSettings> settings)
        {
            _blogService = blogService;
            _elasticClient = elasticClient;
            _settings = settings;
        }

        [Route("/search")]
        public async Task<IActionResult> Find(string query, int page = 1, int pageSize = 5)
        {
            var response = await _elasticClient.SearchAsync<Post>(
                s => s.Query(q => q.QueryString(d => d.Query(query)))
                    .From((page - 1) * pageSize)
                    .Size(pageSize));

            ViewData["Title"] = _settings.Value.Name + " - Search Results";
            ViewData["Description"] = _settings.Value.Description;

            if (!response.IsValid)
            {
                // We could handle errors here by checking response.OriginalException or response.ServerError properties
                return View("Results", new Post[] { });
            }

            if (page > 1)
            {
                ViewData["prev"] = GetSearchUrl(query, page - 1, pageSize);
            }

            if (response.IsValid && response.Total > page * pageSize)
            {
                ViewData["next"] = GetSearchUrl(query, page + 1, pageSize);
            }

            return View("Results", response.Documents);
        }

        [Authorize]
        [Route("/search/reindex")]
        public async Task<IActionResult> ReIndex()
        {
            await _elasticClient.DeleteByQueryAsync<Post>(q => q.MatchAll());

            var allPosts = (await _blogService.GetPosts(int.MaxValue)).ToArray();

            foreach (var post in allPosts)
            {
                await _elasticClient.IndexDocumentAsync(post);
            }

            return Ok($"{allPosts.Length} post(s) reindexed");
        }

        private static string GetSearchUrl(string query, int page, int pageSize)
        {
            return $"/search?query={Uri.EscapeDataString(query ?? "")}&page={page}&pagesize={pageSize}/";
        }
    }
}