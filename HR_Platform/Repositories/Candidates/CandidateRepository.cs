using HR_Platform.Data;
using HR_Platform.DTOs.Candidates;
using HR_Platform.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Platform.Repositories.Candidates
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly ApplicationDbContext dbContext;
        public CandidateRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Candidate>> GetAllAsync() 
        { 
            return await dbContext.Candidates
                                    .Include(c => c.CandidateSkills)
                                    .ThenInclude(cs => cs.Skill)
                                    .ToListAsync();
        }

        public async Task<Candidate?> GetByIdAsync(int id)
        {
            return await dbContext.Candidates
                                    .Include(c => c.CandidateSkills)
                                    .ThenInclude(cs => cs.Skill)
                                    .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Candidate?> GetByEmailAsync(string email)
        {
            return await dbContext.Candidates.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> ExistsWithEmailAsync(string email, int? excludeId = null)
        {
            var query = dbContext.Candidates.Where(c => c.Email == email);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<Candidate?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await dbContext.Candidates.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }

        public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, int? excludeId = null)
        {
            var query = dbContext.Candidates.Where(c => c.PhoneNumber == phoneNumber);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<List<Candidate>> SearchAsync(string? name, int[]? skillIds)
        {
            var query = dbContext.Candidates.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                string nameLower = name.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(nameLower));
            }

            if (skillIds != null && skillIds.Length > 0)
            {
                foreach (int skillId in skillIds)
                {
                    query = query.Where(c => c.CandidateSkills.Any(cs => cs.SkillId == skillId));
                }
            }

            return await query.Include(c => c.CandidateSkills)
                                        .ThenInclude(cs => cs.Skill)
                                        .ToListAsync();
        }

        public async Task AddAsync(Candidate candidate)
        {
            dbContext.Candidates.Add(candidate);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Candidate candidate)
        {
            dbContext.Candidates.Remove(candidate);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            await dbContext.Candidates.Where(c => c.Id == id).ExecuteDeleteAsync();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
