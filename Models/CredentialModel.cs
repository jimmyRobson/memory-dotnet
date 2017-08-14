using System;
using System.ComponentModel.DataAnnotations;

namespace Memory.API.Models
{
    public class CredentialModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}