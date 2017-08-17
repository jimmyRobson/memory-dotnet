using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Memory.API.Models
{
    public class UserCreateModel
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public ICollection<ScoreCreateModel> GameScores {get; set;}
            = new List<ScoreCreateModel>();

    }
}