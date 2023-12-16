using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Company_Software_Project_Documentation.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Continutul articolului este obligatoriu")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Data este obligatorie")]
        public DateTime DateTime { get; set; }

        
        public string? UserId { get; set; }

        // Un articol este scris de un singur utilizator
        public virtual ApplicationUser? User { get; set; } // → un comentariu apartine unui singur utilizator


        // Un articol se afla intr-o categorie
        [Required(ErrorMessage = "Proiectul este obligatoriu")]
        public int? ProjectId { get; set; }
        // The category we want the article to be in and the list of comments
        public virtual Project? Project { get; set; }

    }
}
