using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Przyjęte do schroniska")]
        Admitted,
        [Display(Name = "Oczekuje na adopcję")]
        AvailableForAdoption,
        [Display(Name = "Dom tymczasowy")]
        InTemporaryHome,
        [Display(Name = "Adoptowane")]
        AdoptionCompleted,
        [Display(Name = "Zmarło")]
        Deceased,
    }

    public class Pet
    {
        public int Id { get; set; }

        [Display(Name = "Imię")]
        public string Name { get; set; } = null!;


        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Display(Name = "Data Przyjęcia")]
        public DateTime AdmissionDate { get; set; }

        [Display(Name = "Status Adopcji")]
        public AdoptionStatus AdoptionStatus { get; set; }


        [Display(Name = "Data Urodzenia")]
        public DateOnly? BirthDate { get; set; }

        [Display(Name = "Waga")]
        public double Weight { get; set; }

        [Display(Name = "Gatunek")]
        public Species Species { get; set; }

        [Display(Name = "Płeć")]
        public Gender Gender { get; set; }

        public int BreedId {  get; set; }

        [Display(Name = "Rasa")]
        public Breed Breed { get; set; } = null!;
    }
}
