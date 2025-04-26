using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Breed
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public Species Species { get; set; }
    }
}
