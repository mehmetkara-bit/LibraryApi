using LibraryApi.Contracts.Loans;

namespace LibraryApi.Infrastructure.Circulation;

public interface ILoanService
{
    Task<LoanResponse> BorrowAsync(int staffId, LoanBorrowRequest request);
    Task<LoanResponse> ReturnAsync(int staffId, int loanId, LoanReturnRequest request);
    Task<List<LoanListItemResponse>> GetActiveAsync();
    Task<List<LoanListItemResponse>> GetAllAsync(int? memberId, int? bookCopyId, bool? isActive);
    Task<LoanDetailResponse?> GetByIdAsync(int id);
}