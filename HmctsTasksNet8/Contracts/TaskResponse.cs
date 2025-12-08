using HmctsTasks.Models;

namespace HmctsTasks.Contracts
{
    public class TaskResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset DueAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public static TaskResponse FromEntity(TaskItem entity)
        {
            return new TaskResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Status = entity.Status.ToString(),
                DueAt = entity.DueAt,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
