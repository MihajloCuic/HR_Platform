using HR_Platform.Data;
using HR_Platform.DTOs.Candidates;
using HR_Platform.Models;
using HR_Platform.Repositories.Candidates;
using HR_Platform.Repositories.CandidateSkills;
using HR_Platform.Repositories.Skills;
using HR_Platform.Services.Candidates;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Platform.Tests.ServiceTests
{
    public class CandidateServiceTests
    {
        private readonly Mock<ICandidateRepository> mockCandidateRepository;
        private readonly Mock<ISkillRepository> mockSkillRepository;
        private readonly Mock<ICandidateSkillRepository> mockCandidateSkillRepository;
        private readonly CandidateService candidateService;
        private Candidate dummyCandidate1;
        private Candidate dummyCandidate2;
        private Skill dummySkill1;
        private Skill dummySkill2;
        private Skill dummySkill3;

        public CandidateServiceTests()
        {
            mockCandidateRepository = new Mock<ICandidateRepository>();
            mockSkillRepository = new Mock<ISkillRepository>();
            mockCandidateSkillRepository = new Mock<ICandidateSkillRepository>();

            candidateService = new CandidateService(
                mockCandidateRepository.Object,
                mockSkillRepository.Object,
                mockCandidateSkillRepository.Object
            );

            dummySkill1 = new Skill
            {
                Id = 1,
                Name = "C#"
            };

            dummySkill2 = new Skill
            {
                Id = 2,
                Name = "Java"
            };

            dummySkill3 = new Skill
            {
                Id = 3,
                Name = "Python"
            };

            dummyCandidate1 = new Candidate
            {
                Id = 1,
                Name = "Petar Petrovic",
                Birthday = new DateOnly(2000, 1, 1),
                PhoneNumber = "0612345678",
                Email = "petar@petrovic.com",
                CandidateSkills = new List<CandidateSkill>
                { 
                    new CandidateSkill
                    { 
                        CandidateId = 1,
                        SkillId = 3,
                        Skill = dummySkill3
                    }
                }
            };

            dummyCandidate2 = new Candidate
            {
                Id = 2,
                Name = "Marko Petrovic",
                Birthday = new DateOnly(2001, 2, 1),
                PhoneNumber = "0673322322",
                Email = "marko@petrovic.com",
                CandidateSkills = new List<CandidateSkill>()
            };
        }

        [Fact]
        public async Task GetAllCandidatesAsync_Success()
        {
            List<Candidate> candidates = new List<Candidate> { dummyCandidate1, dummyCandidate2 };

            mockCandidateRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(candidates);

            var result = await candidateService.GetAllCandidatesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Petar Petrovic", result[0].Name);
            Assert.Equal("Marko Petrovic", result[1].Name);
        }

        [Fact]
        public async Task GetCandidateByIdAsync_Success()
        {
            int candidateId = 1;
            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);

            var result = await candidateService.GetCandidateByIdAsync(candidateId);

            Assert.NotNull(result);
            Assert.Equal("Petar Petrovic", result.Name);
        }

        [Fact]
        public async Task GetCandidateByIdAsync_CandidateNotFound() 
        {
            int candidateId = -1;
            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync((Candidate?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => candidateService.GetCandidateByIdAsync(candidateId)
            );

            Assert.Contains($"Candidate with id {candidateId} not found!", exception.Message);
        }

        [Fact]
        public async Task CreateCandidateAsync_Success()
        {
            CreateCandidateDTO createCandidateDTO = new CreateCandidateDTO
            {
                Name = "Jovan Jovanovic",
                Birthday = new DateOnly(1999, 5, 5),
                PhoneNumber = "0698765432",
                Email = "jovan@jovanovic.com"
            };

            mockCandidateRepository.Setup(repo => repo.GetByEmailAsync(createCandidateDTO.Email))
                .ReturnsAsync((Candidate?)null);
            mockCandidateRepository.Setup(repo => repo.GetByPhoneNumberAsync(createCandidateDTO.PhoneNumber))
                .ReturnsAsync((Candidate?)null);
            mockCandidateRepository.Setup(repo => repo.AddAsync(It.IsAny<Candidate>()))
                .Returns(Task.CompletedTask);
            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Candidate
                { 
                    Id = 2,
                    Name = createCandidateDTO.Name,
                    Birthday = createCandidateDTO.Birthday,
                    PhoneNumber = createCandidateDTO.PhoneNumber,
                    Email = createCandidateDTO.Email,
                    CandidateSkills = new List<CandidateSkill>()
                });

            var result = await candidateService.CreateCandidateAsync(createCandidateDTO);

            Assert.NotNull(result);
            Assert.Equal(createCandidateDTO.Name, result.Name);
            Assert.Equal(createCandidateDTO.Birthday, result.Birthday);
            Assert.Equal(createCandidateDTO.PhoneNumber, result.PhoneNumber);
            Assert.Equal(createCandidateDTO.Email, result.Email);
            mockCandidateRepository.Verify(repo => repo.AddAsync(It.IsAny<Candidate>()), Times.Once);
        }

        [Fact]
        public async Task CreateCandidateAsync_Email_Exists() 
        {
            CreateCandidateDTO createCandidateDTO = new CreateCandidateDTO
            {
                Name = "Jovan Jovanovic",
                Birthday = new DateOnly(1999, 5, 5),
                PhoneNumber = "0698765432",
                Email = "petar@petrovic.com"
            };

            mockCandidateRepository.Setup(repo => repo.GetByEmailAsync(createCandidateDTO.Email))
                .ReturnsAsync(dummyCandidate1);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => candidateService.CreateCandidateAsync(createCandidateDTO)
            );

            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task CreateCandidateAsync_FutureBirthday()
        {
            CreateCandidateDTO createCandidateDTO = new CreateCandidateDTO
            {
                Name = "Jovan Jovanovic",
                Birthday = new DateOnly(2035, 5, 5),
                PhoneNumber = "0698765432",
                Email = "petar@petrovic.com"
            };

            mockCandidateRepository.Setup(repo => repo.GetByEmailAsync(createCandidateDTO.Email))
                .ReturnsAsync((Candidate?)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => candidateService.CreateCandidateAsync(createCandidateDTO)
            );

            Assert.Contains("birthday cannot be in the future", exception.Message);
        }

        [Fact]
        public async Task CreateCandidateAsync_PhoneNumberExists() 
        {
            CreateCandidateDTO createCandidateDTO = new CreateCandidateDTO
            {
                Name = "Jovan Jovanovic",
                Birthday = new DateOnly(1999, 5, 5),
                PhoneNumber = "0612345678",
                Email = "jovan@jovanovic.com"
            };

            mockCandidateRepository.Setup(repo => repo.GetByEmailAsync(createCandidateDTO.Email))
                .ReturnsAsync((Candidate?)null);
            mockCandidateRepository.Setup(repo => repo.GetByPhoneNumberAsync(createCandidateDTO.PhoneNumber))
                .ReturnsAsync(dummyCandidate1);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => candidateService.CreateCandidateAsync(createCandidateDTO)
            );

            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateCandidateAsync_Success()
        {
            int candidateId = 1;
            UpdateCandidateDTO updateCandidateDTO = new UpdateCandidateDTO
            {
                Name = "Zoran Petrovic",
                Birthday = new DateOnly(1998, 12, 12),
                PhoneNumber = "0699999999",
                Email = "zoran@petrovic.com"
            };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            mockCandidateRepository.Setup(repo => repo.ExistsWithEmailAsync(updateCandidateDTO.Email, candidateId))
                .ReturnsAsync(false);

            mockCandidateRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await candidateService.UpdateCandidateAsync(candidateId, updateCandidateDTO);


            Assert.NotNull(result); 
            Assert.Equal(updateCandidateDTO.Name, result.Name);
            Assert.Equal(updateCandidateDTO.Birthday, result.Birthday);
            Assert.Equal(updateCandidateDTO.PhoneNumber, result.PhoneNumber);
            Assert.Equal(updateCandidateDTO.Email, result.Email);
            mockCandidateRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCandidateAsync_CandidateNotFound() 
        {
            int candidateId = -1;
            UpdateCandidateDTO updateCandidateDTO = new UpdateCandidateDTO
            {
                Name = "Zoran Petrovic",
                Birthday = new DateOnly(1998, 12, 12),
                PhoneNumber = "0699999999",
                Email = "zoran@petrovic.com"
            };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync((Candidate?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => candidateService.UpdateCandidateAsync(candidateId, updateCandidateDTO)
            );

            Assert.Contains($"Candidate with ID {candidateId} not found!", exception.Message);
        }

        [Fact]
        public async Task UpdateCandidateAsync_FutureBirthday() 
        {
            int candidateId = 1;
            UpdateCandidateDTO updateCandidateDTO = new UpdateCandidateDTO
            {
                Birthday = new DateOnly(2035, 12, 12)
            };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => candidateService.UpdateCandidateAsync(candidateId, updateCandidateDTO)
            );

            Assert.Contains("Candidate birthday cannot be in the future", exception.Message);
        }

        [Fact]
        public async Task UpdateCandidateAsync_EmailExists()
        {
            int candidateId = 1;
            UpdateCandidateDTO updateCandidateDTO = new UpdateCandidateDTO
            {
                Email = "marko@petrovic.com"
            };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            mockCandidateRepository.Setup(repo => repo.ExistsWithEmailAsync(updateCandidateDTO.Email, candidateId))
                .ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => candidateService.UpdateCandidateAsync(candidateId, updateCandidateDTO)
            );

            Assert.Contains("already in use", exception.Message);
        }

        [Fact]
        public async Task UpdateCandidateAsync_PhoneNumberExists() 
        {
            int candidateId = 1;
            UpdateCandidateDTO updateCandidateDTO = new UpdateCandidateDTO
            {
                PhoneNumber = "0673322322"
            };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            mockCandidateRepository.Setup(repo => repo.ExistsWithPhoneNumberAsync(updateCandidateDTO.PhoneNumber, candidateId))
                .ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => candidateService.UpdateCandidateAsync(candidateId, updateCandidateDTO)
            );

            Assert.Contains("already in use", exception.Message);
        }

        [Fact]
        public async Task DeleteCandidateAsync_Success() 
        {
            int candidateId = 1;

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            mockCandidateRepository.Setup(repo => repo.DeleteAsync(dummyCandidate1))
                .Returns(Task.CompletedTask);

            await candidateService.DeleteCandidateAsync(candidateId);

            mockCandidateRepository.Verify(repo => repo.DeleteAsync(dummyCandidate1), Times.Once);
        }

        [Fact]
        public async Task DeleteCandidateAsync_CandidateNotFound() 
        {
            int candidateId = -1;

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync((Candidate?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => candidateService.DeleteCandidateAsync(candidateId)
            );

            Assert.Contains($"Candidate with id {candidateId} not found!", exception.Message);
        }

        [Fact]
        public async Task AddSkillsToCandidateAsync_Success() 
        {
            int candidateId = 1;
            var skillIds = new List<int> { 1, 2 };
            List<Skill> dummySkills = new List<Skill> { dummySkill1, dummySkill2 };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            mockSkillRepository.Setup(repo => repo.GetSkillsByIdAsync(skillIds))
                .ReturnsAsync(dummySkills);
            mockCandidateSkillRepository.Setup(repo => repo.AddRangeAsync(candidateId, dummySkills))
                .Returns(Task.CompletedTask);

            await candidateService.AddSkillsToCandidateAsync(candidateId, skillIds);

            mockCandidateSkillRepository.Verify(repo => repo.AddRangeAsync(candidateId, dummySkills), Times.Once);
        }

        [Fact]
        public async Task AddSkillsToCandidateAsync_CandidateNotFound() 
        {
            int candidateId = -1;
            List<int> skillIds = new List<int> { 1, 2 };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync((Candidate?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => candidateService.AddSkillsToCandidateAsync(candidateId, skillIds)
            );

            Assert.Contains($"Candidate with id {candidateId} not found!", exception.Message);
        }

        [Fact]
        public async Task AddSkillsToCandidateAsync_SkillsNotFound() 
        {
            int candidateId = 1;
            List<int> skillIds = new List<int> { 1, 2, 3 };
            List<Skill> dummySkills = new List<Skill> ();
            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            mockSkillRepository.Setup(repo => repo.GetSkillsByIdAsync(skillIds))
                .ReturnsAsync(dummySkills);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => candidateService.AddSkillsToCandidateAsync(candidateId, skillIds)
            );

            Assert.Contains("One or more skills not found!", exception.Message);
        }

        [Fact]
        public async Task AddSkillsToCandidateAsync_CandidateAlreadyHasSkill() 
        {
            int candidateId = 1;
            List<int> skillIds = new List<int> { 1, 2, 3 };
            List<Skill> dummySkills = new List<Skill> { dummySkill1, dummySkill2, dummySkill3 };

            mockCandidateRepository.Setup(repo => repo.GetByIdAsync(candidateId))
                .ReturnsAsync(dummyCandidate1);
            mockSkillRepository.Setup(repo => repo.GetSkillsByIdAsync(skillIds))
                .ReturnsAsync(dummySkills);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => candidateService.AddSkillsToCandidateAsync(candidateId, skillIds)
            );

            Assert.Contains("Candidate already has one or more of the specified skills!", exception.Message);
        }

        [Fact]
        public async Task RemoveSkillFromCandidateAsync_Success() 
        {
            int candidateId = 1;
            int skillId = 3;

            var candidateSkill = new CandidateSkill
            {
                CandidateId = candidateId,
                SkillId = skillId
            };

            mockCandidateSkillRepository.Setup(repo => repo.GetAsync(candidateId, skillId))
                .ReturnsAsync(candidateSkill);
            mockCandidateSkillRepository.Setup(repo => repo.RemoveAsync(candidateSkill))
                .Returns(Task.CompletedTask);

            await candidateService.RemoveSkillFromCandidateAsync(candidateId, skillId);

            mockCandidateSkillRepository.Verify(repo => repo.RemoveAsync(candidateSkill), Times.Once);
        }

        [Fact]
        public async Task RemoveSkillFromCandidateAsync_CandidateDoesNotHaveSkill() 
        {
            int candidateId = -1;
            int skillId = -1;

            mockCandidateSkillRepository.Setup(repo => repo.GetAsync(candidateId, skillId))
                .ReturnsAsync((CandidateSkill?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => candidateService.RemoveSkillFromCandidateAsync(candidateId, skillId)
            );

            Assert.Contains($"Candidate with id {candidateId} doesn't have a skill {skillId}", exception.Message);
        }
    }
}
