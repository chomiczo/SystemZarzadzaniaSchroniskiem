using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace SystemZarzadzaniaSchroniskiem.Models
{
    public enum Species
    {
        [Display(Name = "pies")]
        Dog,
        [Display(Name = "kot")]
        Cat
    }

    public enum Gender
    {
        [Display(Name = "samiec")]
        Male,
        [Display(Name = "samica")]
        Female
    }

    public enum AdoptionStatus
    {
        [Display(Name = "Przyjęto do schroniska")]
        Admitted,
        [Display(Name = "Oczekuje na adopcję")]
        AvailableForAdoption,
        [Display(Name = "Dom tymczasowy")]
        InTemporaryHome,
        [Display(Name = "Adoptowano")]
        AdoptionCompleted,
        [Display(Name = "Zmarło")]
        Deceased,
    }

    public class Pet
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Pole Imię jest wymagane")]
        public string Name { get; set; } = null!;


        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Display(Name = "Data przyjęcia do schroniska", ShortName = "Data przyjęcia")]
        [Required(ErrorMessage = "Data przyjęcia do schroniska jest wymagana")]
        public DateTime AdmissionDate { get; set; }

        [Display(Name = "Status adopcji")]
        [Required(AllowEmptyStrings = false)]
        public AdoptionStatus AdoptionStatus { get; set; }

        public int? OwnerProfileId { get; set; }
        [Display(Name = "Opiekun")]
        [DeleteBehavior(DeleteBehavior.SetNull)]
        public UserProfile? OwnerProfile { get; set; }


        [Display(Name = "Data urodzenia")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Waga")]
        public double Weight { get; set; }

        [Display(Name = "Gatunek")]
        public Species Species { get; set; }

        [Display(Name = "Płeć")]
        public Gender Gender { get; set; }

        public int BreedId { get; set; }

        [Display(Name = "Rasa")]
        [ValidateNever]
        public Breed Breed { get; set; } = null!;

        [Display(Name = "Zdjęcie")]
        public string? ImagePath { get; set; }

        public List<HealthRecord> HealthRecords { get; set; } = [];

        [NotMapped]
        public IFormFile? FileUpload { get; set; }

        [NotMapped]
        public int Age
        {
            get
            {
                return DateTime.Now.Subtract(BirthDate).Days / 365;
            }
        }
    }
}
