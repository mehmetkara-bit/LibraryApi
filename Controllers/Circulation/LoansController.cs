using LibraryApi.Contracts.Loans;
using LibraryApi.Infrastructure.Circulation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LibraryApi.Controllers.Circulation;

[Authorize(Roles = "Staff")]
[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;

    public LoansController(ILoanService service)
    {
        _service = service;
    }

    [HttpPost("borrow")]
    public async Task<ActionResult<LoanResponse>> Borrow(LoanBorrowRequest request)
    {
        if (!TryGetUserId(out var staffId))
            return Unauthorized();

        try
        {
            var loan = await _service.BorrowAsync(staffId, request);
            return Ok(loan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{loanId:int}/return")]
    public async Task<ActionResult<LoanResponse>> Return(int loanId, LoanReturnRequest request)
    {

        if (!TryGetUserId(out var staffId))
           return Unauthorized();


        try
        {
           var loan = await _service.ReturnAsync(staffId, loanId, request);
            return Ok(loan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ✅ GET /api/loans/active
    [HttpGet("active")]
    public async Task<ActionResult<List<LoanListItemResponse>>> GetActive()
    {
        var list = await _service.GetActiveAsync();
        return Ok(list);
    }

    // ✅ GET /api/loans?memberId=..&bookCopyId=..&isActive=true/false
    [HttpGet]
    public async Task<ActionResult<List<LoanListItemResponse>>> GetAll(
        [FromQuery] int? memberId,
        [FromQuery] int? bookCopyId,
        [FromQuery] bool? isActive)
    {
        var list = await _service.GetAllAsync(memberId, bookCopyId, isActive);
        return Ok(list);
    }

    // ✅ GET /api/loans/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDetailResponse>> GetById(int id)
    {
        var loan = await _service.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;

        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");

        return int.TryParse(idStr, out userId);
    }
}