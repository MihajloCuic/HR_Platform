using HR_Platform.Data;
using Microsoft.EntityFrameworkCore;

namespace HR_Platform.Repositories.Skills
{
    public class SkillRepository : ISkillRepository
    {
        private readonly ApplicationDbContext dbContext;

        public SkillRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Models.Skill>> GetSkillsByIdAsync(List<int> skillIds)
        {
            return await dbContext.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToListAsync();
        }
    }
}
