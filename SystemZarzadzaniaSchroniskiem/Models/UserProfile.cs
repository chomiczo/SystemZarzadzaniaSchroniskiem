using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SystemZarzadzaniaSchroniskiem.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        [Required(ErrorMessage = "Pole Imię jest wymagane.")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Nazwisko")]
        [Required(ErrorMessage = "Pole Nazwisko jest wymagane.")]
        public string LastName { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public IdentityUser User { get; set; } = null!;

        public List<Timetable>? Timetables { get; set; }

        [NotMapped]
        public bool IsLocked
        {
            get
            {
                return User.AccessFailedCount >= 3;
            }
        }

        [NotMapped]
        public class RegisterInput
        {
            [Display(Name = "E-mail")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Pole E-mail jest wymagane")]
            public string Email { get; set; } = null!;

            [Display(Name = "Hasło")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Hasło jest wymagane")]
            public string Password { get; set; } = null!;

            [Display(Name = "Potwierdź Hasło")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Potwierdzenie hasła jest wymagane")]
            [Compare(nameof(Password), ErrorMessage = "Hasło musi się zgadzać")]
            public string ConfirmPassword { get; set; } = null!;
            [Display(Name = "Imię")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Imię jest wymagane")]
            public string FirstName { get; set; } = null!;
            [Display(Name = "Nazwisko")]
            [Required(AllowEmptyStrings = false, ErrorMessage = "Nazwisko jest wymagane")]
            public string LastName { get; set; } = null!;
        }
    }
}
