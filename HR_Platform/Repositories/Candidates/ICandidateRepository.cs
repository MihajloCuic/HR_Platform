using HR_Platform.Models;

namespace HR_Platform.Repositories.Candidates
{
    public interface ICandidateRepository
    {
        Task<List<Candidate>> GetAllAsync();
        Task<Candidate?> GetByIdAsync(int id);
        Task<Candidate?> GetByEmailAsync(string email);
        Task<bool> ExistsWithEmailAsync(string email, int? excludeId = null);
        Task<Candidate?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, int? excludeId = null);
        Task<List<Candidate>> SearchAsync(string? name, int[]? skillIds);
        Task AddAsync(Candidate candidate);
        Task DeleteAsync(Candidate candidate);
        Task DeleteByIdAsync(int id);
        Task SaveChangesAsync();
    }
}
