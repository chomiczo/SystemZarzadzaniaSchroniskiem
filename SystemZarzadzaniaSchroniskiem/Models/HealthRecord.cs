namespace SystemZarzadzaniaSchroniskiem.Models
{
    public class HealthRecord
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public Pet Pet { get; set; } = null!;
        public DateTime CreationDate { get; set; }
        public string Content { get; set; } = null!;
    }
}
