using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Memory.API.Entities
{
    public class GameScore
    {
        [Key]       
        public Guid Id { get; set; }

        [Required]
        public int Score { get; set; }
        
        [Required]
        public DateTime ScoreDate { get; set; }

        [Required]
        [ForeignKey("GameUserId")]
        public GameUser GameUser { get; set; }
    }
}
