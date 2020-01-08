using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Forum.Models
{
    public class Profile
    {
        [Key]
        public string ProfileId { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }
        // public byte[] Image { get; set; }
    }
}