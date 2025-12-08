using HmctsTasks.Contracts;
using HmctsTasks.Data;
using HmctsTasks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskStatus = HmctsTasks.Models.TaskStatus;

namespace HmctsTasks.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public TasksController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var statusText = request.Status?.Trim();

            if (!Enum.TryParse(typeof(TaskStatus), statusText, true, out var statusObj))
            {
                ModelState.AddModelError(nameof(request.Status),
                    "Status must be one of: New, InProgress, Completed.");
                return ValidationProblem(ModelState);
            }

            var status = (TaskStatus)statusObj;


            var now = DateTimeOffset.UtcNow;

            if (request.DueAt <= now)
            {
                ModelState.AddModelError(nameof(request.DueAt),
                    "Due date and time must be in the future.");
                return ValidationProblem(ModelState);
            }

            var entity = new TaskItem
            {
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),
                Status = status,
                DueAt = request.DueAt,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.Tasks.Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = TaskResponse.FromEntity(entity);

            return CreatedAtAction(nameof(GetTaskById), new { id = entity.Id }, response);
        }

        /// <summary>
        /// Returns a task by id.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> GetTaskById(int id)
        {
            var entity = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            return TaskResponse.FromEntity(entity);
        }
    }
}
