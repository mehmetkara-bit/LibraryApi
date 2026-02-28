using LibraryApi.Contracts.BookCopies;
using LibraryApi.Infrastructure.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers.Catalog;

[ApiController]
public class BookCopiesController : ControllerBase
{
    private readonly IBookCopyService _service;

    public BookCopiesController(IBookCopyService service)
    {
        _service = service;
    }

    // POST /api/books/{bookId}/copies
    [HttpPost("api/books/{bookId:int}/copies")]
    public async Task<ActionResult<List<BookCopyResponse>>> CreateCopies(int bookId, BookCopyCreateRequest request)
    {
        try
        {
            var created = await _service.CreateCopiesAsync(bookId, request);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/books/{bookId}/copies
    [HttpGet("api/books/{bookId:int}/copies")]
    public async Task<ActionResult<List<BookCopyResponse>>> GetCopiesByBook(int bookId)
    {
        try
        {
            var list = await _service.GetCopiesByBookIdAsync(bookId);
            return Ok(list);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}