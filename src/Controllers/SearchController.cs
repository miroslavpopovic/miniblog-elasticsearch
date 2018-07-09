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
            var response = await _elasticClient.SearchAsync<IndexedPost>(
                s => s.Query(q => q.QueryString(d => d.Query(query)))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize));

            ViewData["Title"] = _settings.Value.Name + " - Search Results";
            ViewData["Description"] = _settings.Value.Description;

            if (page > 1)
            {
                ViewData["prev"] = $"/search?query={Uri.EscapeDataString(query)}&page={page - 1}&pagesize={pageSize}/";
            }

            if (response.Total > page * pageSize)
            {
                ViewData["next"] = $"/search?query={Uri.EscapeDataString(query)}&page={page + 1}&pagesize={pageSize}/";
            }

            return View("Results", response.Documents);
        }

        [Authorize]
        [Route("/search/reindex")]
        public async Task<IActionResult> ReIndex()
        {
            await _elasticClient.DeleteByQueryAsync<IndexedPost>(q => q.MatchAll());

            var allPosts = (await _blogService.GetPosts(int.MaxValue)).ToArray();

            foreach (var post in allPosts)
            {
                var indexedPost = IndexedPost.FromPost(post);
                await _elasticClient.IndexDocumentAsync(indexedPost);
            }

            return Ok($"{allPosts.Length} post(s) reindexed");
        }
    }
}