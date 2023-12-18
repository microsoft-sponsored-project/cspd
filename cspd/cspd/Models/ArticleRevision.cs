using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Company_Software_Project_Documentation.Models
{
    public class ArticleRevision
    {
        [Key]
        public int Id { get; set; }

        public int ArticleId { get; set; }

        [ForeignKey("ArticleId")]
        public virtual required Article Article { get; set; }

        public required string Content { get; set; }

        public DateTime RevisionDate { get; set; }
    }
}
