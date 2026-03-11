using BookStoreApi.Models;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BooksService _booksService;

        public BooksController(BooksService booksService) =>
            _booksService = booksService;

        [HttpGet]
        public async Task<List<Books>> Get() =>
            await _booksService.GetAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Books>> Get(string id)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
                return NotFound();

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Books newBook)
        {
            await _booksService.CreateAsync(newBook);
            return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Books updatedBook)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
                return NotFound();

            updatedBook.Id = book.Id;
            await _booksService.UpdateAsync(id, updatedBook);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
                return NotFound();

            await _booksService.RemoveAsync(id);
            return NoContent();
        }
    }
}