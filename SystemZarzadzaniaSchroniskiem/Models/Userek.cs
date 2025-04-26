using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SystemZarzadzaniaSchroniskiem.Models
{
    public class Userek
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pole Imię jest wymagane.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Pole Nazwisko jest wymagane.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Pole Email jest wymagane.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format adresu email.")]
        public string Email { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public virtual IdentityUser? User { get; set; }
    }
}
