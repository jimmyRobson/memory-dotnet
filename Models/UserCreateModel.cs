using System;
using System.ComponentModel.DataAnnotations;

namespace Memory.API.Models
{
    public class UserCreateModel
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}