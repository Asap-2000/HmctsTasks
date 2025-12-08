using System;
using System.Linq;
using System.Threading.Tasks;
using HmctsTasks.Contracts;
using HmctsTasks.Controllers;
using HmctsTasks.Data;
using HmctsTasks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskStatus = HmctsTasks.Models.TaskStatus;
using Microsoft.Extensions.Logging.Abstractions;


namespace HmctsTasks.Tests
{
    [TestClass]
    public class TasksControllerTests
    {
        private static AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static ObjectResult? ExtractObjectResult<T>(ActionResult<T> action)
        {
            // Direct ObjectResult (BadRequestObjectResult, ValidationProblemResult, etc.)
            if (action.Result is ObjectResult or)
            {
                // Some controller helper implementations produce an ObjectResult whose StatusCode
                // property is null but whose Value is a ProblemDetails/ValidationProblemDetails
                // with the actual status. Handle that case by normalizing to an ObjectResult
                // that has a StatusCode set.
                if (or.StatusCode == null && or.Value is ProblemDetails pdFromOr)
                    return new ObjectResult(pdFromOr) { StatusCode = pdFromOr.Status ?? StatusCodes.Status400BadRequest };

                return or;
            }

            // Some controller helper paths return an unwrapped ProblemDetails (e.g. ValidationProblem)
            if (action.Value is ProblemDetails pdFromValue)
                return new ObjectResult(pdFromValue) { StatusCode = pdFromValue.Status ?? StatusCodes.Status400BadRequest };

            // Fallback: if the result is a plain StatusCodeResult of 400, wrap to ObjectResult
            if (action.Result is StatusCodeResult sc && sc.StatusCode == StatusCodes.Status400BadRequest)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status400BadRequest };

            return null;
        }

        [TestMethod]
        public async Task CreateTask_ReturnsCreated_WhenRequestIsValid()
        {
            using var db = CreateInMemoryDb();
            var controller = new TasksController(db, NullLogger<TasksController>.Instance);

            var request = new CreateTaskRequest
            {
                Title = "Prepare case file",
                Description = "Case 12345",
                Status = "New",
                DueAt = DateTimeOffset.UtcNow.AddHours(1)
            };

            var result = await controller.CreateTask(request);

            var created = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(created);

            var response = created.Value as TaskResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual("Prepare case file", response.Title);
            Assert.AreEqual("New", response.Status);
        }

        [TestMethod]
        public async Task CreateTask_TrimsTitleAndDescription()
        {
            using var db = CreateInMemoryDb();
            var controller = new TasksController(db, NullLogger<TasksController>.Instance);

            var request = new CreateTaskRequest
            {
                Title = "  Trimmed title  ",
                Description = "  Trimmed description  ",
                Status = "New",
                DueAt = DateTimeOffset.UtcNow.AddHours(1)
            };

            var result = await controller.CreateTask(request);
            var created = result.Result as CreatedAtActionResult;

            Assert.IsNotNull(created);

            var response = created.Value as TaskResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual("Trimmed title", response.Title);
            Assert.AreEqual("Trimmed description", response.Description);
        }

        [TestMethod]
        public async Task CreateTask_AllowsNullOrWhitespaceDescription()
        {
            using var db = CreateInMemoryDb();
            var controller = new TasksController(db, NullLogger<TasksController>.Instance);

            var request = new CreateTaskRequest
            {
                Title = "No description",
                Description = "   ",
                Status = "New",
                DueAt = DateTimeOffset.UtcNow.AddHours(1)
            };

            var result = await controller.CreateTask(request);
            var created = result.Result as CreatedAtActionResult;

            Assert.IsNotNull(created);

            var response = created.Value as TaskResponse;
            Assert.IsNotNull(response);

            Assert.IsNull(response.Description);
        }

        [TestMethod]
        public async Task CreateTask_RejectsMissingTitle()
        {
            using var db = CreateInMemoryDb();
            var controller = new TasksController(db, NullLogger<TasksController>.Instance);

            controller.ModelState.AddModelError(nameof(CreateTaskRequest.Title), "Required");

            var result = await controller.CreateTask(new CreateTaskRequest
            {
                Title = "",
                Status = "New",
                DueAt = DateTimeOffset.UtcNow.AddHours(1)
            });

            var badRequest = ExtractObjectResult(result);

            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task CreateTask_RejectsInvalidStatus()
        {
            using var db = CreateInMemoryDb();
            var controller = new TasksController(db, NullLogger<TasksController>.Instance);

            var request = new CreateTaskRequest
            {
                Title = "Invalid status task",
                Status = "NotARealStatus",
                DueAt = DateTimeOffset.UtcNow.AddHours(1)
            };

            var result = await controller.CreateTask(request);
            var badRequest = ExtractObjectResult(result);

            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task CreateTask_AcceptsStatusCaseInsensitive()
        {
            using var db = CreateInMemoryDb();
            var controller = new TasksController(db, NullLogger<TasksController>.Instance);

            var request = new CreateTaskRequest
            {
                Title = "Case insensitive status",
                Status = "inprogress",
                DueAt = DateTimeOffset.UtcNow.AddHours(1)
            };

            var result = await controller.CreateTask(request);

            var created = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(created);

            var response = created.Value as TaskResponse;
            Assert.IsNotNull(response);

            Assert.AreEqual("InProgress", response.Status);
        }

        [TestMethod]
        public async Task CreateTask_RejectsPastDueDate()
        {
            using var db = CreateInMemoryDb();
            var controller = new TasksController(db, NullLogger<TasksController>.Instance);

            var request = new CreateTaskRequest
            {
                Title = "Past due",
                Status = "New",
                DueAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            };

            var result = await controller.CreateTask(request);
            var badRequest = ExtractObjectResult(result);

            Assert.IsNotNull(badRequest);
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        }
    }
}
