using LibraryApi.Contracts.Loans;
using LibraryApi.Domain.Entities;
using LibraryApi.Domain.Enums;
using LibraryApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Circulation;

public class LoanService : ILoanService
{
    private readonly AppDbContext _db;

    public LoanService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LoanResponse> BorrowAsync(int staffId, LoanBorrowRequest request)
    {
        // Member var mı?
        var memberExists = await _db.Members.AnyAsync(m => m.Id == request.MemberId);
        if (!memberExists) throw new InvalidOperationException("Member not found.");

        // Staff var mı?
        var staffExists = await _db.Staff.AnyAsync(s => s.Id == staffId);
        if (!staffExists) throw new InvalidOperationException("Staff not found.");

        // Copy var mı + status uygun mu?
        var copy = await _db.BookCopies.FirstOrDefaultAsync(c => c.Id == request.BookCopyId);
        if (copy is null) throw new InvalidOperationException("Book copy not found.");
        if (copy.Status != BookCopyStatus.Available)
            throw new InvalidOperationException("Book copy is not available.");

        // LoanDate/DueDate check (db constraint var ama daha iyi mesaj için)
        if (request.DueDate < request.LoanDate)
            throw new InvalidOperationException("DueDate must be >= LoanDate.");

        // Loan oluştur
        var loan = new Loan
        {
            MemberId = request.MemberId,
            BookCopyId = request.BookCopyId,
            StaffId = staffId,
            LoanDate = request.LoanDate,
            DueDate = request.DueDate,

            // senin SQLite kuralın: aktif loan = ActiveMarker = 1
            ActiveMarker = 1
        };

        _db.Loans.Add(loan);

        // Copy status değiştir
        copy.Status = BookCopyStatus.Loaned;

        await _db.SaveChangesAsync();

        return ToResponse(loan);
    }
    
    public async Task<LoanResponse> ReturnAsync(int staffId, int loanId, LoanReturnRequest request)
    {
        var staffExists = await _db.Staff.AnyAsync(s => s.Id == staffId);
        if (!staffExists) throw new InvalidOperationException("Staff not found.");

        var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan is null) throw new InvalidOperationException("Loan not found.");

        if (loan.ActiveMarker != 1)
            throw new InvalidOperationException("Loan is not active.");

        var copy = await _db.BookCopies.FirstOrDefaultAsync(c => c.Id == loan.BookCopyId);
        if (copy is null) throw new InvalidOperationException("Book copy not found.");

        // Return
        loan.ReturnDate = request.ReturnDate;
        loan.ActiveMarker = null;

        // Copy available
        copy.Status = BookCopyStatus.Available;

        await _db.SaveChangesAsync();

        return ToResponse(loan);
    }

    private static LoanResponse ToResponse(Loan l) => new()
    {
        Id = l.Id,
        MemberId = l.MemberId,
        BookCopyId = l.BookCopyId,
        StaffId = l.StaffId,
        LoanDate = l.LoanDate,
        DueDate = l.DueDate,
        ReturnDate = l.ReturnDate,
        IsActive = l.ActiveMarker == 1
    };


    public async Task<List<LoanListItemResponse>> GetActiveAsync()
{
    return await GetAllAsync(memberId: null, bookCopyId: null, isActive: true);
}

public async Task<List<LoanListItemResponse>> GetAllAsync(int? memberId, int? bookCopyId, bool? isActive)
{
    var query = _db.Loans
        .AsNoTracking()
        .Include(l => l.Member)
        .Include(l => l.BookCopy)
            .ThenInclude(bc => bc.Book)
        .AsQueryable();

    if (memberId.HasValue)
        query = query.Where(l => l.MemberId == memberId.Value);

    if (bookCopyId.HasValue)
        query = query.Where(l => l.BookCopyId == bookCopyId.Value);

    if (isActive.HasValue)
        query = isActive.Value
            ? query.Where(l => l.ActiveMarker == 1)
            : query.Where(l => l.ActiveMarker != 1);

    var list = await query
        .OrderByDescending(l => l.LoanDate)
        .Select(l => new LoanListItemResponse
        {
            Id = l.Id,
            MemberId = l.MemberId,
            MemberName = l.Member.FirstName + " " + l.Member.LastName,

            BookCopyId = l.BookCopyId,
            Barcode = l.BookCopy.Barcode,
            BookTitle = l.BookCopy.Book.Title,

            LoanDate = l.LoanDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            IsActive = l.ActiveMarker == 1
        })
        .ToListAsync();

    return list;
}

public async Task<LoanDetailResponse?> GetByIdAsync(int id)
{
    var loan = await _db.Loans
        .AsNoTracking()
        .Include(l => l.Member)
        .Include(l => l.Staff)
        .Include(l => l.BookCopy)
            .ThenInclude(bc => bc.Book)
        .Include(l => l.Fine)
        .FirstOrDefaultAsync(l => l.Id == id);

    if (loan is null) return null;

    return new LoanDetailResponse
    {
        Id = loan.Id,

        MemberId = loan.MemberId,
        MemberName = loan.Member.FirstName + " " + loan.Member.LastName,

        StaffId = loan.StaffId,
        StaffName = loan.Staff.FirstName + " " + loan.Staff.LastName,

        BookCopyId = loan.BookCopyId,
        Barcode = loan.BookCopy.Barcode,

        BookId = loan.BookCopy.BookId,
        BookTitle = loan.BookCopy.Book.Title,

        LoanDate = loan.LoanDate,
        DueDate = loan.DueDate,
        ReturnDate = loan.ReturnDate,
        IsActive = loan.ActiveMarker == 1,

        HasFine = loan.Fine != null
    };
}
}