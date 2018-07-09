using System;

namespace MiniBlogElasticsearch.Models
{
    public class IndexedComment
    {
        public string Id { get; set; }

        public string Author { get; set; }

        public string Content { get; set; }

        public DateTime PubDate { get; set; }

        public static IndexedComment FromComment(Comment comment)
        {
            return new IndexedComment
            {
                Id = comment.ID,
                Author = comment.Author,
                Content = comment.Content,
                PubDate = comment.PubDate
            };
        }
    }
}