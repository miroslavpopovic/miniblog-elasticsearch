using System;

namespace MiniBlogElasticsearch.Models
{
    public class IndexedComment
    {
        public string ID { get; set; }

        public string Author { get; set; }

        public string Content { get; set; }

        public DateTime PubDate { get; set; }

        public static IndexedComment FromComment(Comment comment)
        {
            return new IndexedComment
            {
                ID = comment.ID,
                Author = comment.Author,
                Content = comment.Content,
                PubDate = comment.PubDate
            };
        }
    }
}