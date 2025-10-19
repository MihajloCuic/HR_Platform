using HR_Platform.Controllers;
using HR_Platform.DTOs.Candidates;
using HR_Platform.DTOs.Skills;
using HR_Platform.Models;
using HR_Platform.Services.Candidates;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HR_Platform.Tests.ControllerTests
{
    public class CandidateControllerTests
    {
        private readonly Mock<ICandidateService> mockCandidateService;
        private readonly CandidatesController controller;
        private CandidateResponseDTO candidateResponse;

        public CandidateControllerTests()
        {
            mockCandidateService = new Mock<ICandidateService>();
            controller = new CandidatesController(mockCandidateService.Object);

            candidateResponse = new CandidateResponseDTO
            {
                Id = 1,
                Name = "Ana Anic",
                Email = "ana@anic.com",
                PhoneNumber = "3234453452",
                Birthday = new DateOnly(1971, 8, 2),
                Skills = new List<SkillResponseDTO>
                {
                    new SkillResponseDTO { Id = 1, Name = "C#" },
                    new SkillResponseDTO { Id = 2, Name = "SQL" }
                }
            };
        }

        [Fact]
        public async Task GetAllCandidates_Success()
        {
            List<CandidateResponseDTO> candidates = new List<CandidateResponseDTO>
            {
                candidateResponse
            };

            mockCandidateService.Setup(s => s.GetAllCandidatesAsync()).ReturnsAsync(candidates);

            var result = await controller.GetAllCandidates();

            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetCandidateById_Success()
        {
            int candidateId = 1;

            mockCandidateService.Setup(s => s.GetCandidateByIdAsync(candidateId))
                .ReturnsAsync(candidateResponse);

            var result = await controller.GetCandidateById(candidateId);

            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetCandidateById_WrongCandidateId()
        {
            int candidateId = -1;
            mockCandidateService.Setup(s => s.GetCandidateByIdAsync(candidateId))
                .ThrowsAsync(new KeyNotFoundException("Not found"));

            var result = await controller.GetCandidateById(candidateId);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, statusResult.StatusCode);
        }

        [Fact]
        public async Task SearchCandidates_Success()
        {
            string name = "ana";
            int[] skillIds = { 1, 3 };
            List<CandidateResponseDTO> candidates = new List<CandidateResponseDTO>
            {
                candidateResponse
            };

            mockCandidateService.Setup(s => s.SearchCandidatesAsync(name, skillIds))
                .ReturnsAsync(candidates);

            var result = await controller.SearchCandidates(name, skillIds);

            var statusResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, statusResult.StatusCode);
        }

        [Fact]
        public async Task CreateCandidate_Success()
        {
            CreateCandidateDTO createCandidateDTO = new CreateCandidateDTO
            {
                Name = "Petar Petrovic",
                Birthday = new DateOnly(1985, 7, 20),
                PhoneNumber = "5934587345",
                Email = "petar@petrovic.com"
            };

            CandidateResponseDTO createdCandidate = new CandidateResponseDTO
            {
                Id = 1,
                Name = createCandidateDTO.Name,
                Birthday = createCandidateDTO.Birthday,
                PhoneNumber = createCandidateDTO.PhoneNumber,
                Email = createCandidateDTO.Email,
                Skills = new List<SkillResponseDTO>()
            };

            mockCandidateService.Setup(s => s.CreateCandidateAsync(createCandidateDTO))
                .ReturnsAsync(createdCandidate);

            var result = await controller.CreateCandidate(createCandidateDTO);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateCandidate_EmailAlreadyExists()
        {
            var createDto = new CreateCandidateDTO
            {
                Name = "Marko Markovic",
                Email = "ana@anic.com",
                PhoneNumber = "123",
                Birthday = new DateOnly(1990, 1, 1)
            };

            mockCandidateService.Setup(s => s.CreateCandidateAsync(createDto))
                .ThrowsAsync(new InvalidOperationException("Email already exists"));

            var result = await controller.CreateCandidate(createDto);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task CreateCandidateWithSkills_Success()
        {
            var createDto = new CreateCandidateWithSkillsDTO
            {
                Name = "mirko mirkovic",
                Email = "new@test.com",
                PhoneNumber = "123456",
                Birthday = new DateOnly(1995, 5, 10),
                SkillsIds = new List<int> { 1, 2 }
            };

            var createdCandidate = new CandidateResponseDTO
            {
                Id = 1,
                Name = createDto.Name,
                Email = createDto.Email,
                PhoneNumber = createDto.PhoneNumber,
                Birthday = createDto.Birthday,
                Skills = new List<SkillResponseDTO>
                {
                    new SkillResponseDTO { Id = 1, Name = "C#" },
                    new SkillResponseDTO { Id = 2, Name = "SQL" }
                }
            };

            mockCandidateService.Setup(s => s.CreateCandidateWithSkillsAsync(createDto))
                .ReturnsAsync(createdCandidate);

            var result = await controller.CreateCandidateWithSkills(createDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task CreateCandidateWithSkills_SkillNotFound()
        {
            var createDto = new CreateCandidateWithSkillsDTO
            {
                Name = "mirko mirkovic",
                Email = "new@test.com",
                PhoneNumber = "123456",
                Birthday = new DateOnly(1995, 5, 10),
                SkillsIds = new List<int> { 1, 2 }
            };

            mockCandidateService.Setup(s => s.CreateCandidateWithSkillsAsync(createDto))
                .ThrowsAsync(new KeyNotFoundException("Skill not found"));

            var result = await controller.CreateCandidateWithSkills(createDto);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, statusResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCandidate_Success()
        {
            int candidateId = 1;
            var updateDto = new UpdateCandidateDTO
            {
                Name = "jadranka Jandric",
                Birthday = new DateOnly(1988, 12, 12),
                PhoneNumber = "987654321",
                Email = "jadranka@jandric.com"
            };

            var updatedCandidate = new CandidateResponseDTO
            {
                Id = candidateId,
                Name = updateDto.Name,
                Email = updateDto.Email,
                PhoneNumber = updateDto.PhoneNumber,
                Birthday = updateDto.Birthday.Value,
                Skills = new List<SkillResponseDTO>()
            };

            mockCandidateService.Setup(s => s.UpdateCandidateAsync(candidateId, updateDto))
                .ReturnsAsync(updatedCandidate);

            var result = await controller.UpdateCandidate(candidateId, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCandidate_WrongCandidateId()
        {
            int candidateId = -1;
            var updateDto = new UpdateCandidateDTO
            {
                Name = "jadranka Jandric",
                Birthday = new DateOnly(1988, 12, 12),
                PhoneNumber = "987654321",
                Email = "jadranka@jandric.com"
            };

            mockCandidateService.Setup(s => s.UpdateCandidateAsync(candidateId, updateDto))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            var result = await controller.UpdateCandidate(candidateId, updateDto);

            var statusResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, statusResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCandidate_Success()
        {
            int candidateId = 1;

            mockCandidateService.Setup(s => s.DeleteCandidateAsync(candidateId))
                .Returns(Task.CompletedTask);

            var result = await controller.DeleteCandidate(candidateId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCandidate_WrongCandidateId()
        {
            int candidateId = -1;

            mockCandidateService.Setup(s => s.DeleteCandidateAsync(candidateId))
                .ThrowsAsync(new KeyNotFoundException("Candidate not found"));

            var result = await controller.DeleteCandidate(candidateId);

            var statusResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, statusResult.StatusCode);
        }

        [Fact]
        public async Task AddSkillsToCandidate_Success()
        { 
            int candidateId = 1;
            AddSkillsDTO skills = new AddSkillsDTO
            { 
                skillIds = new List<int> { 3, 4 }
            };

            var updatedCandidate = candidateResponse;
            updatedCandidate.Skills.AddRange(new List<SkillResponseDTO>
            {
                new SkillResponseDTO { Id = 3, Name = "Java" },
                new SkillResponseDTO { Id = 4, Name = "Python" }
            });

            mockCandidateService.Setup(s => s.AddSkillsToCandidateAsync(candidateId, skills.skillIds))
                .Returns(Task.CompletedTask);
            mockCandidateService.Setup(s => s.GetCandidateByIdAsync(candidateId))
                .ReturnsAsync(updatedCandidate);

            var result = await controller.AddSkillsToCandidate(candidateId, skills);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task AddSkillsToCandidate_SkillAlreadyExists()
        {
            int candidateId = 1;
            AddSkillsDTO skills = new AddSkillsDTO
            {
                skillIds = new List<int> { 1 }
            };

            mockCandidateService.Setup(s => s.AddSkillsToCandidateAsync(candidateId, skills.skillIds))
                .ThrowsAsync(new InvalidOperationException("Skill already exists for candidate"));

            var result = await controller.AddSkillsToCandidate(candidateId, skills);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task RemoveSkillFromCandidate_Success()
        {
            int candidateId = 1;
            int skillId = 1;

            var updatedCandidate = candidateResponse;
            updatedCandidate.Skills.RemoveAll(s => s.Id == skillId);

            mockCandidateService.Setup(s => s.RemoveSkillFromCandidateAsync(candidateId, skillId))
                .Returns(Task.CompletedTask);
            mockCandidateService.Setup(s => s.GetCandidateByIdAsync(candidateId))
                .ReturnsAsync(updatedCandidate);

            var result = await controller.RemoveSkillFromCandidate(candidateId, skillId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task RemoveSkillFromCandidate_SkillNotFound()
        {
            int candidateId = 1;
            int skillId = 99;

            mockCandidateService.Setup(s => s.RemoveSkillFromCandidateAsync(candidateId, skillId))
                .ThrowsAsync(new KeyNotFoundException("Skill not found for candidate"));

            var result = await controller.RemoveSkillFromCandidate(candidateId, skillId);

            var statusResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, statusResult.StatusCode);
        }
    }
}
