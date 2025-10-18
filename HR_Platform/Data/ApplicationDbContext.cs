using HR_Platform.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_Platform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<CandidateSkill> CandidateSkills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Candidate>(entity =>
            { 
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber).IsUnique();

                entity.HasMany(e => e.CandidateSkills)
                      .WithOne(cs => cs.Candidate)
                      .HasForeignKey(cs => cs.CandidateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Skill>(entity => 
            { 
                entity.HasIndex(e => e.Name).IsUnique();

                entity.HasMany(e => e.CandidateSkills)
                      .WithOne(cs => cs.Skill)
                      .HasForeignKey(cs => cs.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CandidateSkill>(entity =>
            {
                entity.HasIndex(cs => new { cs.CandidateId, cs.SkillId }).IsUnique();
            });



            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Skill>().HasData(
                new Skill { Id = 1, Name = "C#" },
                new Skill { Id = 2, Name = "JavaScript" },
                new Skill { Id = 3, Name = "SQL" },
                new Skill { Id = 4, Name = "English" },
                new Skill { Id = 5, Name = "Database Design" },
                new Skill { Id = 6, Name = "Project Management" },
                new Skill { Id = 7, Name = "Russian" },
                new Skill { Id = 8, Name = "German" }
            );

            modelBuilder.Entity<Candidate>().HasData(
                new Candidate
                {
                    Id = 1,
                    Name = "Petar Petrovic",
                    Birthday = new DateOnly(1990, 5, 24),
                    PhoneNumber = "+381623457998",
                    Email = "petar.petrovic@gmail.com"
                },
                new Candidate
                {
                    Id = 2,
                    Name = "Ana Jovanovic",
                    Birthday = new DateOnly(2002, 12, 4),
                    PhoneNumber = "+381656783207",
                    Email = "anajovanovic@gmail.com"
                },
                new Candidate
                {
                    Id = 3,
                    Name = "Pera Peric",
                    Birthday = new DateOnly(1986, 3, 5),
                    PhoneNumber = "+381630096381",
                    Email = "pera.peric@gmail.com"
                },
                new Candidate
                {
                    Id = 4,
                    Name = "Jelena Djordjevic",
                    Birthday = new DateOnly(1999, 8, 30),
                    PhoneNumber = "+381623358998",
                    Email = "jelena.djordjevic@gmail.com"
                },
                new Candidate
                {
                    Id = 5,
                    Name = "Marko Markovic",
                    Birthday = new DateOnly(2000, 5, 12),
                    PhoneNumber = "+381612766438",
                    Email = "marko.markovic@gmail.com"
                }
            );

            modelBuilder.Entity<CandidateSkill>().HasData(
                new CandidateSkill { Id = 1, CandidateId = 1, SkillId = 1 },
                new CandidateSkill { Id = 2, CandidateId = 1, SkillId = 4 },
                new CandidateSkill { Id = 3, CandidateId = 1, SkillId = 5 },
                new CandidateSkill { Id = 4, CandidateId = 2, SkillId = 2 },
                new CandidateSkill { Id = 5, CandidateId = 2, SkillId = 4 },
                new CandidateSkill { Id = 6, CandidateId = 2, SkillId = 7 },
                new CandidateSkill { Id = 7, CandidateId = 3, SkillId = 1 },
                new CandidateSkill { Id = 8, CandidateId = 3, SkillId = 3 },
                new CandidateSkill { Id = 9, CandidateId = 3, SkillId = 6 },
                new CandidateSkill { Id = 10, CandidateId = 4, SkillId = 2 },
                new CandidateSkill { Id = 11, CandidateId = 4, SkillId = 4 },
                new CandidateSkill { Id = 12, CandidateId = 4, SkillId = 8 },
                new CandidateSkill { Id = 13, CandidateId = 5, SkillId = 1 },
                new CandidateSkill { Id = 14, CandidateId = 5, SkillId = 2 },
                new CandidateSkill { Id = 15, CandidateId = 5, SkillId = 3 }
            );
        }
    }
}
