using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Breed
    {
        public int Id { get; set; }
        [Display(Name = "Nazwa")]
        public string Name { get; set; } = null!;
        [Display(Name = "Gatunek")]
        public Species Species { get; set; }
    }
}
