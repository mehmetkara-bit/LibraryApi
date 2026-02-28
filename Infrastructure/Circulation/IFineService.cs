using LibraryApi.Contracts.Fines;

namespace LibraryApi.Infrastructure.Circulation;

public interface IFineService
{
    Task<FineResponse?> GetByLoanIdAsync(int loanId);
    Task<FineResponse> CalculateAsync(int loanId, FineCalculateRequest request);
    Task<FineResponse> MarkPaidAsync(int loanId, FinePayRequest request);
}