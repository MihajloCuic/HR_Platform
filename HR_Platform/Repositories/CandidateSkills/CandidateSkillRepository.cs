using HR_Platform.Data;
using HR_Platform.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Platform.Repositories.CandidateSkills
{
    public class CandidateSkillRepository : ICandidateSkillRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CandidateSkillRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<CandidateSkill?> GetAsync(int candidateId, int skillId)
        {
            return await dbContext.CandidateSkills.FirstOrDefaultAsync(cs => cs.CandidateId == candidateId && cs.SkillId == skillId);
        }

        public async Task AddRangeAsync(int candidateId, List<Skill> skills)
        {
            foreach (var skill in skills)
            {
                dbContext.CandidateSkills.Add(new CandidateSkill
                {
                    CandidateId = candidateId,
                    SkillId = skill.Id
                });
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveAsync(CandidateSkill candidateSkill)
        {
            dbContext.CandidateSkills.Remove(candidateSkill);
            await dbContext.SaveChangesAsync();
        }
    }
}
