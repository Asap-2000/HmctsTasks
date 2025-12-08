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
    public class TasksController(AppDbContext dbContext, ILogger<TasksController> logger) : ControllerBase
    {
        private readonly AppDbContext _dbContext = dbContext;
        private readonly ILogger<TasksController> _logger = logger;

        /// <summary>
        /// Creates a new task.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskResponse>> CreateTask(
            [FromBody] CreateTaskRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Received request to create task with title {Title}", request.Title);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid for task creation. Errors: {Errors}",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                return ValidationProblem(ModelState);
            }

            var statusText = request.Status?.Trim();

            if (!Enum.TryParse(typeof(TaskStatus), statusText, true, out var statusObj))
            {
                _logger.LogWarning("Invalid status value '{Status}' supplied for task creation.", statusText);
                ModelState.AddModelError(nameof(request.Status),
                    "Status must be one of: New, InProgress, Completed.");
                return ValidationProblem(ModelState);
            }

            var status = (TaskStatus)statusObj;

            var now = DateTimeOffset.UtcNow;

            if (request.DueAt <= now)
            {
                _logger.LogWarning("DueAt {DueAt} is not in the future for title {Title}.", request.DueAt, request.Title);
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
                CreatedAt = now
            };

            _dbContext.Tasks.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task created with id {Id} and status {Status}.", entity.Id, entity.Status);

            var response = TaskResponse.FromEntity(entity);

            return CreatedAtAction(nameof(GetTaskById), new { id = entity.Id }, response);
        }

        /// <summary>
        /// Returns a task by id.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> GetTaskById(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching task with id {Id}.", id);

            var entity = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Task with id {Id} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Task with id {Id} found.", id);

            return TaskResponse.FromEntity(entity);
        }
    }
}