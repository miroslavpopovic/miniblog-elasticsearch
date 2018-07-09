using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniBlogElasticsearch.Models
{
    public class IndexedPost 
    {
        public string ID { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Excerpt { get; set; }

        public string Content { get; set; }

        public DateTime PubDate { get; set; }

        public DateTime LastModified { get; set; }

        public IEnumerable<IndexedComment> Comments { get; set; }

        public static IndexedPost FromPost(Post post)
        {
            return new IndexedPost 
            {
                ID = post.ID,
                Title = post.Title,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                Content = post.Content,
                PubDate = post.PubDate,
                LastModified = post.LastModified,
                Comments = post.Comments.Select(
                    comment => IndexedComment.FromComment(comment))                
            };
        }
    }
}