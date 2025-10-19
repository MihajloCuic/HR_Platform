using HR_Platform.Models;

namespace HR_Platform.Repositories.Skills
{
    public interface ISkillRepository
    {
        Task<List<Skill>> GetSkillsByIdAsync(List<int> skillIds);
    }
}
