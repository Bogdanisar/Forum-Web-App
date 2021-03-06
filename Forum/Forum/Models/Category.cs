﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required!")]
        public string CategoryName { get; set; }

        public DateTime Date { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; }
    }
}