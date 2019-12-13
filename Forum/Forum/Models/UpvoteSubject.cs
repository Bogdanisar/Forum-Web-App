using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class UpvoteSubject
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public int SubjectId { get; set; }
    }
}