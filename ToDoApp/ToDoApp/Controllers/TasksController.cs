using Microsoft.AspNetCore.Mvc;
using ToDoApp.Models;
using ToDoApp.Services;

namespace ToDoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _service;

        public TasksController(TaskService service)
        {
            _service = service;
        }

        // GET /api/tasks
        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> GetAll() =>
            await _service.GetAllAsync();

        // GET /api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> Get(string id)
        {
            var task = await _service.GetByIdAsync(id);
            return task == null ? NotFound() : task;
        }

        // POST /api/tasks
        [HttpPost]
        public async Task<IActionResult> Create(TaskCreateDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _service.CreateAsync(task);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
        }

        // PUT /api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, TaskItem task)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            task.Id = existing.Id;
            await _service.UpdateAsync(id, task);
            return NoContent();
        }

        // DELETE /api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }

        // GET /api/tasks/latest
        [HttpGet("latest")]
        public async Task<ActionResult<List<TaskItem>>> GetLatest() =>
            await _service.GetLatestFromRedisAsync();
    }
}
