using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Memory.API.Entities
{
  public class GameUser : IdentityUser
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ICollection<GameScore> GameScores { get; set; }
            = new List<GameScore>();

  }
}