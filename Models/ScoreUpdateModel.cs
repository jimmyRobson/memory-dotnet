using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Memory.API.Models
{
    public class ScoreUpdateModel
    {
        [Required]
        public int Score { get; set; }

    }
}