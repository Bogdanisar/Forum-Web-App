using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "The subject name is required")]
        public string Title { get; set; }
        
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        //[Required(ErrorMessage = "The subject must be created by a user")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}