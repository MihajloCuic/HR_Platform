using HR_Platform.Data;
using HR_Platform.DTOs.Candidates;
using HR_Platform.Models;
using HR_Platform.DTOs.Skills;
using Microsoft.EntityFrameworkCore;
using HR_Platform.Repositories.Candidates;
using HR_Platform.Repositories.Skills;
using HR_Platform.Repositories.CandidateSkills;

namespace HR_Platform.Services.Candidates
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository candidateRepository;
        private readonly ISkillRepository skillRepository;
        private readonly ICandidateSkillRepository candidateSkillRepository;

        public CandidateService(ICandidateRepository candidateRepository, ISkillRepository skillRepository, ICandidateSkillRepository candidateSkillRepository)
        {
            this.candidateRepository = candidateRepository;
            this.skillRepository = skillRepository;
            this.candidateSkillRepository = candidateSkillRepository;
        }

        /// <summary>
        /// Retrieves all candidates from the database with their associated skills.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<List<CandidateResponseDTO>> GetAllCandidatesAsync()
        {
            var candidates = await candidateRepository.GetAllAsync();

            if (candidates == null || !candidates.Any())
            {
                throw new KeyNotFoundException("No candidates found!");
            }

            return candidates.Select(c => MapCandidateToDTO(c)).ToList();
        }

        /// <summary>
        /// Retrieves a specific candidate by ID with their associated skills.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<CandidateResponseDTO> GetCandidateByIdAsync(int id)
        {
            var candidate = await candidateRepository.GetByIdAsync(id);

            if (candidate == null)
            {
                throw new KeyNotFoundException($"Candidate with id {id} not found!");
            }

            return MapCandidateToDTO(candidate);
        }

        /// <summary>
        /// Searches for candidates based on name and/or skill IDs.
        /// Applies multiple filter conditions: name (partial, case-insensitive) and skill IDs (all must match).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="skillIds"></param>
        /// <returns></returns>
        public async Task<List<CandidateResponseDTO>> SearchCandidatesAsync(string? name, int[]? skillIds)
        {
            var candidates = await candidateRepository.SearchAsync(name, skillIds);
            return candidates.Select(c => MapCandidateToDTO(c)).ToList();
        }

        /// <summary>
        /// Creates a new candidate with validation.
        /// Validates: email uniqueness, phone number uniqueness, and birthday is not in future.
        /// </summary>
        /// <param name="createCandidateDTO"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<CandidateResponseDTO> CreateCandidateAsync(CreateCandidateDTO createCandidateDTO)
        {
            var existngCandidate = await candidateRepository.GetByEmailAsync(createCandidateDTO.Email);
            if (existngCandidate != null)
            {
                throw new InvalidOperationException($"Candidate with email {existngCandidate.Email} already exists!");
            }

            if (createCandidateDTO.Birthday > DateOnly.FromDateTime(DateTime.Now))
            {
                throw new InvalidOperationException("Candidate birthday cannot be in the future!");
            }

            existngCandidate = await candidateRepository.GetByPhoneNumberAsync(createCandidateDTO.PhoneNumber);
            if (existngCandidate != null)
            {
                throw new InvalidOperationException($"Candidate with email {existngCandidate.PhoneNumber} already exists!");
            }

            var candidate = MapDTOToCandidate(createCandidateDTO);
            await candidateRepository.AddAsync(candidate);

            var createdCandidate = await candidateRepository.GetByIdAsync(candidate.Id);
            if (createdCandidate == null)
            {
                throw new Exception("An error occurred while creating the candidate!");
            }

            return MapCandidateToDTO(createdCandidate);
        }

        /// <summary>
        /// Creates a new candidate with associated skills in a single operation.
        /// First creates the candidate, then adds specified skills.
        /// If skill assignment fails, the created candidate is automatically deleted (rollback).
        /// </summary>
        /// <param name="createCandidateWithSkillsDTO"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<CandidateResponseDTO> CreateCandidateWithSkillsAsync(CreateCandidateWithSkillsDTO createCandidateWithSkillsDTO)
        {
            CreateCandidateDTO createCandidateDTO = new CreateCandidateDTO
            {
                Name = createCandidateWithSkillsDTO.Name,
                Birthday = createCandidateWithSkillsDTO.Birthday,
                PhoneNumber = createCandidateWithSkillsDTO.PhoneNumber,
                Email = createCandidateWithSkillsDTO.Email
            };

            var createdCandidate = await CreateCandidateAsync(createCandidateDTO);

            if (createCandidateWithSkillsDTO.SkillsIds != null && createCandidateWithSkillsDTO.SkillsIds.Count > 0)
            {
                try
                {
                    await AddSkillsToCandidateAsync(createdCandidate.Id, createCandidateWithSkillsDTO.SkillsIds);
                    var updatedCandidate = await candidateRepository.GetByIdAsync(createdCandidate.Id);
                    if (updatedCandidate == null)
                    {
                        throw new Exception("An error occurred while loading the candidate!");
                    }
                    return MapCandidateToDTO(updatedCandidate);
                }
                catch (Exception ex)
                {
                    await candidateRepository.DeleteByIdAsync(createdCandidate.Id);
                    await candidateRepository.SaveChangesAsync();

                    throw new InvalidOperationException($"Failed to create candidate with skills: {ex.Message}");
                }
            }

            return createdCandidate;
        }

        /// <summary>
        /// Updates an existing candidate's information with partial update support.
        /// Only updates fields that are provided (non-null, non-empty in DTO).
        /// Validates email and phone uniqueness before updating.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateCandidateDTO"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<CandidateResponseDTO> UpdateCandidateAsync(int id, UpdateCandidateDTO updateCandidateDTO)
        {
            var candidate = await candidateRepository.GetByIdAsync(id);

            if (candidate == null)
            {
                throw new KeyNotFoundException($"Candidate with ID {id} not found!");
            }

            if (!string.IsNullOrWhiteSpace(updateCandidateDTO.Name))
            {
                candidate.Name = updateCandidateDTO.Name;
            }

            if (updateCandidateDTO.Birthday.HasValue)
            {
                candidate.Birthday = updateCandidateDTO.Birthday.Value;
            }

            if (!string.IsNullOrWhiteSpace(updateCandidateDTO.PhoneNumber) && updateCandidateDTO.PhoneNumber != candidate.PhoneNumber)
            {
                var existingPhoneNumber = await candidateRepository.ExistsWithPhoneNumberAsync(updateCandidateDTO.PhoneNumber, id);

                if (existingPhoneNumber)
                {
                    throw new InvalidOperationException($"Phone number '{updateCandidateDTO.PhoneNumber}' is already in use!");
                }
                candidate.PhoneNumber = updateCandidateDTO.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(updateCandidateDTO.Email) && updateCandidateDTO.Email != candidate.Email)
            {
                var existingEmail = await candidateRepository.ExistsWithEmailAsync(updateCandidateDTO.Email, id);

                if (existingEmail)
                {
                    throw new InvalidOperationException($"Email '{updateCandidateDTO.Email}' is already in use!");
                }

                candidate.Email = updateCandidateDTO.Email;
            }

            await candidateRepository.SaveChangesAsync();
            return MapCandidateToDTO(candidate);
        }

        /// <summary>
        /// Deletes a candidate from the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task DeleteCandidateAsync(int id)
        {
            var candidate = await candidateRepository.GetByIdAsync(id);

            if (candidate == null)
            {
                throw new KeyNotFoundException($"Candidate with ID {id} not found!");
            }

            await candidateRepository.DeleteAsync(candidate);
        }

        /// <summary>
        /// Adds multiple skills to a candidate in a batch operation.
        /// Validates that all skill IDs exist and candidate doesn't already have the skills.
        /// </summary>
        /// <param name="candidateId"></param>
        /// <param name="skillIds"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task AddSkillsToCandidateAsync(int candidateId, List<int> skillIds)
        {
            var candidate = await candidateRepository.GetByIdAsync(candidateId);
            if (candidate == null)
            {
                throw new KeyNotFoundException($"Candidate with id {candidateId} not found!");
            }

            List<Skill> skills = await skillRepository.GetSkillsByIdAsync(skillIds);
            if (skills.Count() != skillIds.Count)
            {
                throw new KeyNotFoundException("One or more skills not found!");
            }

            if (candidate.CandidateSkills.Any(cs => skillIds.Contains(cs.SkillId)))
            {
                throw new InvalidOperationException("Candidate already has one or more of the specified skills!");
            }

            await candidateSkillRepository.AddRangeAsync(candidateId, skills);
        }

        /// <summary>
        /// Removes a single skill from a candidate.
        /// </summary>
        /// <param name="candidateId"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task RemoveSkillFromCandidateAsync(int candidateId, int skillId)
        {
            var candidateSkill = await candidateSkillRepository.GetAsync(candidateId, skillId);

            if (candidateSkill == null)
            {
                throw new KeyNotFoundException($"Candidate with id {candidateId} doesn't have a skill {skillId}");
            }

            await candidateSkillRepository.RemoveAsync(candidateSkill);
        }

        /// <summary>
        /// Maps a Candidate entity to a CandidateResponseDTO.
        /// Converts database entity to data transfer object, including nested skill mappings.
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        private CandidateResponseDTO MapCandidateToDTO(Candidate candidate)
        {
            return new CandidateResponseDTO
            {
                Id = candidate.Id,
                Name = candidate.Name,
                Birthday = candidate.Birthday,
                PhoneNumber = candidate.PhoneNumber,
                Email = candidate.Email,
                Skills = candidate.CandidateSkills.Select(cs => new SkillResponseDTO
                {
                    Id = cs.Skill.Id,
                    Name = cs.Skill.Name
                }).ToList()
            };
        }

        /// <summary>
        /// aps a CreateCandidateDTO to a Candidate entity.
        /// Converts data transfer object to database entity for insertion.
        /// </summary>
        /// <param name="createCandidateDTO"></param>
        /// <returns></returns>
        private Candidate MapDTOToCandidate(CreateCandidateDTO createCandidateDTO)
        {
            return new Candidate
            {
                Name = createCandidateDTO.Name,
                Birthday = createCandidateDTO.Birthday,
                PhoneNumber = createCandidateDTO.PhoneNumber,
                Email = createCandidateDTO.Email
            };
        }
    }
}
