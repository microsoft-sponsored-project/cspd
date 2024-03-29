﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Company_Software_Project_Documentation.Models
{
    public class Project
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea proiectului este obligatorie")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Data proiectului este obligatorie")]
        public DateTime DateTime { get; set; }
        public virtual IEnumerable<Article>? Articles { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; } 
    }
}
