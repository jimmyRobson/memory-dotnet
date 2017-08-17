using System;
using System.ComponentModel.DataAnnotations;

namespace Memory.API.Models
{
    public class ScoreCreateModel
    {     
        [Required]
        public int? Score { get; set; }
    }
}