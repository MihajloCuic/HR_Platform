using HR_Platform.DTOs.Candidates;

namespace HR_Platform.Services.Candidates
{
    public interface ICandidateService
    {
        Task<CandidateResponseDTO> GetCandidateByIdAsync(int id);
        Task<List<CandidateResponseDTO>> GetAllCandidatesAsync();
        Task<List<CandidateResponseDTO>> SearchCandidatesAsync(string? name, int[]? skillIds);
        Task<CandidateResponseDTO> CreateCandidateAsync(CreateCandidateDTO createCandidateDTO);
        Task<CandidateResponseDTO> CreateCandidateWithSkillsAsync(CreateCandidateWithSkillsDTO createCandidateWithSkillsDTO);
        Task<CandidateResponseDTO> UpdateCandidateAsync(int id, UpdateCandidateDTO updateCandidateDTO);
        Task DeleteCandidateAsync(int id);
        Task AddSkillsToCandidateAsync(int candidateId, List<int> skillIds);
        Task RemoveSkillFromCandidateAsync(int candidateId, int skillId);
    }
}
