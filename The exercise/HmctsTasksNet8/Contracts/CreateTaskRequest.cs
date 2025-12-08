using System.ComponentModel.DataAnnotations;

namespace HmctsTasks.Contracts
{
    public class CreateTaskRequest
    {
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public required string Status { get; set; } 

        [Required]
        public required DateTimeOffset DueAt { get; set; }
    }
}
