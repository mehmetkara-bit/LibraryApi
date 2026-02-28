using LibraryApi.Contracts.Fines;
using LibraryApi.Infrastructure.Circulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers.Circulation;

[ApiController]
[Route("api/fines")]
[Authorize(Roles = "Staff")]
public class FinesController : ControllerBase
{
    private readonly IFineService _service;

    public FinesController(IFineService service)
    {
        _service = service;
    }

    // GET /api/fines/by-loan/{loanId}
    [HttpGet("by-loan/{loanId:int}")]
    public async Task<ActionResult<FineResponse>> GetByLoanId(int loanId)
    {
        try
        {
            var fine = await _service.GetByLoanIdAsync(loanId);
            return fine is null ? NotFound() : Ok(fine);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/fines/calculate/{loanId}
    [HttpPost("calculate/{loanId:int}")]
    public async Task<ActionResult<FineResponse>> Calculate(int loanId, FineCalculateRequest request)
    {
        try
        {
            var fine = await _service.CalculateAsync(loanId, request);
            return Ok(fine);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/fines/{loanId}/pay
    [HttpPost("{loanId:int}/pay")]
    public async Task<ActionResult<FineResponse>> Pay(int loanId, FinePayRequest request)
    {
        try
        {
            var fine = await _service.MarkPaidAsync(loanId, request);
            return Ok(fine);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}