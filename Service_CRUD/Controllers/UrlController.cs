using CrudCoursework.CQRS.Commands;
using CrudCoursework.CQRS.Handlers;
using CrudCoursework.CQRS.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudCoursework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UrlController : ControllerBase
    {
        private readonly GetAllUrlsHandler _getAll;
        private readonly GetUrlByIdHandler _getById;
        private readonly CreateUrlHandler _create;
        private readonly UpdateUrlHandler _update;
        private readonly DeleteUrlHandler _delete;
        private readonly DeleteAllUrlsHandler _deleteAll = null!;



        public UrlController(
            GetAllUrlsHandler getAll,
            GetUrlByIdHandler getById,
            CreateUrlHandler create,
            UpdateUrlHandler update,
            DeleteUrlHandler delete,
            DeleteAllUrlsHandler deleteAll)
        {
            _getAll = getAll;
            _getById = getById;
            _create = create;
            _update = update;
            _delete = delete;
            _deleteAll = deleteAll;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _getAll.Handle(new GetAllUrlsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getById.Handle(new GetUrlByIdQuery { Id = id });
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUrlCommand command)
        {
            var result = await _create.Handle(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUrlCommand command)
        {
            if (id != command.Id) return BadRequest("ID mismatch");
            var result = await _update.Handle(command);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{shortCode}")]
        public async Task<IActionResult> Delete(string shortCode)
        {
            var result = await _delete.Handle(new DeleteUrlCommand { ShortCode = shortCode });
            return result ? NoContent() : NotFound();
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var result = await _deleteAll.Handle(new DeleteAllUrlsCommand());
            return result ? NoContent() : NotFound("No URLs to delete.");
        }

    }
}
