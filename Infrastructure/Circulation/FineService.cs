using LibraryApi.Contracts.Fines;
using LibraryApi.Domain.Entities;
using LibraryApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Circulation;

public class FineService : IFineService
{
    private readonly AppDbContext _db;

    public FineService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<FineResponse?> GetByLoanIdAsync(int loanId)
    {
        var fine = await _db.Fines
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.LoanId == loanId);

        if (fine is null) return null;

        return new FineResponse
        {
            Id = fine.Id,
            LoanId = fine.LoanId,
            Amount = fine.Amount,
            DaysLate = 0, // sadece calculate endpointinde hesaplıyoruz
            IsPaid = fine.IsPaid,
            CreatedDate = fine.CreatedDate
        };
    }

    public async Task<FineResponse> CalculateAsync(int loanId, FineCalculateRequest request)
    {
        if (request.DailyRate < 0)
            throw new InvalidOperationException("DailyRate must be >= 0.");

        var loan = await _db.Loans
            .Include(l => l.Fine)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan is null)
            throw new InvalidOperationException("Loan not found.");

        var effectiveDate = loan.ReturnDate ?? request.AsOf ?? DateTime.UtcNow;

        var daysLate = (effectiveDate.Date - loan.DueDate.Date).Days;
        if (daysLate < 0) daysLate = 0;

        var amount = request.DailyRate * daysLate;

        if (!request.Persist)
        {
            return new FineResponse
            {
                Id = loan.Fine?.Id ?? 0,
                LoanId = loan.Id,
                Amount = amount,
                DaysLate = daysLate,
                IsPaid = loan.Fine?.IsPaid ?? false,
                CreatedDate = loan.Fine?.CreatedDate ?? DateTime.UtcNow
            };
        }

        if (daysLate == 0)
            throw new InvalidOperationException("No late days. Fine is not created.");

        if (loan.Fine is null)
        {
            var fine = new Fine
            {
                LoanId = loan.Id,
                Amount = amount,
                IsPaid = false,
                CreatedDate = DateTime.UtcNow
            };

            _db.Fines.Add(fine);
            await _db.SaveChangesAsync();

            return new FineResponse
            {
                Id = fine.Id,
                LoanId = fine.LoanId,
                Amount = fine.Amount,
                DaysLate = daysLate,
                IsPaid = fine.IsPaid,
                CreatedDate = fine.CreatedDate
            };
        }
        else
        {
            if (loan.Fine.IsPaid)
                throw new InvalidOperationException("Fine already paid. Cannot recalculate.");

            loan.Fine.Amount = amount;
            await _db.SaveChangesAsync();

            return new FineResponse
            {
                Id = loan.Fine.Id,
                LoanId = loan.Fine.LoanId,
                Amount = loan.Fine.Amount,
                DaysLate = daysLate,
                IsPaid = loan.Fine.IsPaid,
                CreatedDate = loan.Fine.CreatedDate
            };
        }
    }

    public async Task<FineResponse> MarkPaidAsync(int loanId, FinePayRequest request)
    {
        var fine = await _db.Fines
            .FirstOrDefaultAsync(f => f.LoanId == loanId);

        if (fine is null)
            throw new InvalidOperationException("Fine not found.");

        if (fine.IsPaid)
            throw new InvalidOperationException("Fine already paid.");

        fine.IsPaid = true;

        await _db.SaveChangesAsync();

        return new FineResponse
        {
            Id = fine.Id,
            LoanId = fine.LoanId,
            Amount = fine.Amount,
            DaysLate = 0,
            IsPaid = fine.IsPaid,
            CreatedDate = fine.CreatedDate
        };
    }
}