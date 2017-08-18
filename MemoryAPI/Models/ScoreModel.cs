using System;

namespace Memory.API.Models
{
    public class ScoreModel
    {     
        public string Id { get; set; }
        public int Score { get; set; }
        public DateTime ScoreDate { get; set; }
        public string GameUserId { get; set; }
    }
}