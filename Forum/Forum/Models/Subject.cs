using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Forum.Models
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "The subject title is required")]
        public string Title { get; set; }
        
        [AllowHtml]
        [Required(ErrorMessage = "The subject description is required")]
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        //[Required(ErrorMessage = "The subject must be created by a user")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }


        public virtual ICollection<Comment> Comments { get; set; }
    }
}