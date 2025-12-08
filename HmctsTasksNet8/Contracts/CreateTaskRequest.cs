using System.ComponentModel.DataAnnotations;

namespace HmctsTasks.Contracts
{
    public class CreateTaskRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } 

        [Required]
        public DateTimeOffset DueAt { get; set; }
    }
}
