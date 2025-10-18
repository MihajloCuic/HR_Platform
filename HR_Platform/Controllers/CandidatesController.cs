using HR_Platform.DTOs.Candidates;
using HR_Platform.DTOs.Skills;
using HR_Platform.Services.Candidates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR_Platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService candidateService;

        /// <summary>
        /// Constructor injects the ICandidateService depandency which handles all business logic for candidate operations.
        /// </summary>
        public CandidatesController(ICandidateService candidateService)
        {
            this.candidateService = candidateService;
        }

        /// <summary>
        /// GET: api/candidates
        /// Retrieves a list of all candidates.
        /// Success Response: 200 OK with all candidates data
        /// Error Response: 500 Internal Server Error if exception occurs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCandidates() 
        {
            try 
            {
                var candidates = await candidateService.GetAllCandidatesAsync();
                return Ok(new 
                { 
                    success = true,
                    data = candidates
                });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/candidates/{id}
        /// Retrieves a specific candidate by their ID along with their skills.
        /// Success Response: 200 OK with the candidate data
        /// Error Responses: 
        ///   - 404 Not Found if candidate doesn't exist
        ///   - 500 Internal Server Error for other exceptions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCandidateById(int id) 
        {
            try
            {
                var candidate = await candidateService.GetCandidateByIdAsync(id);
                return Ok(new
                {
                    success = true, 
                    data = candidate
                });
            }
            catch (KeyNotFoundException ex) {
                return StatusCode(404, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/candidates/search?name={name}&skillIds={skillIds}
        /// Searches for candidates based on name and/or skills.
        /// Query Parameters:
        ///   - name (optional): Search candidates by name (case-insensitive, partial match)
        ///   - skillIds (optional): Array of skill IDs to filter candidates who have all specified skills
        /// Success Response: 200 OK with matching candidates
        /// Error Response: 500 Internal Server Error
        /// Example: /api/candidates/search?name=John&skillIds=1&skillIds=2
        /// </summary>
        /// <param name="name"></param>
        /// <param name="skillIds"></param>
        /// <returns></returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchCandidates([FromQuery] string? name, [FromQuery] int[]? skillIds) 
        {
            try 
            { 
                var candidates = await candidateService.SearchCandidatesAsync(name, skillIds);
                return Ok(new { status = true, data = candidates });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/candidates
        /// Creates a new candidate without skills.
        /// Request Body: CreateCandidateDTO with name, email, phone number, and birthday
        /// Success Response: 201 Created with the newly created candidate data and location header
        /// Error Responses:
        ///   - 400 Bad Request if model validation fails
        ///   - 409 Conflict if email or phone number already exists
        ///   - 500 Internal Server Error for other exceptions
        /// </summary>
        /// <param name="createCandidateDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDTO createCandidateDTO) 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { status = false, errors = ModelState });
                }

                var candidate = await candidateService.CreateCandidateAsync(createCandidateDTO);

                return CreatedAtAction(
                    nameof(GetCandidateById), 
                    new { id = candidate.Id }, 
                    new { success = true, data = candidate, message = "Candidate created successfully!" }
                );
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/candidates/with-skills
        /// Creates a new candidate with associated skills in a single operation.
        /// Request Body: CreateCandidateWithSkillsDTO with candidate info and list of skill IDs
        /// Success Response: 201 Created with the newly created candidate including skills
        /// Error Responses:
        ///   - 400 Bad Request if model validation fails
        ///   - 404 Not Found if one or more skill IDs don't exist
        ///   - 409 Conflict if email/phone exists or duplicate skills in request
        ///   - 500 Internal Server Error for other exceptions
        /// If skill assignment fails, the created candidate is automatically deleted (rollback)
        /// </summary>
        /// <param name="createCandidateWithSkillsDTO"></param>
        /// <returns></returns>
        [HttpPost("with-skills")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCandidateWithSkills([FromBody] CreateCandidateWithSkillsDTO createCandidateWithSkillsDTO) 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, errors = ModelState });
                }

                var candidate = await candidateService.CreateCandidateWithSkillsAsync(createCandidateWithSkillsDTO);

                return CreatedAtAction(
                    nameof(GetCandidateById),
                    new { id = candidate.Id },
                    new { success = true, data = candidate, message = "Candidate created successfully!" }
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex) 
            {
                return StatusCode(404, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/candidates/{id}
        /// Updates an existing candidate's information (name, email, phone, birthday).
        /// Path Parameter: id - The candidate ID to update
        /// Request Body: UpdateCandidateDTO with fields to update (all fields optional)
        /// Success Response: 200 OK with updated candidate data
        /// Error Responses:
        ///   - 400 Bad Request if model validation fails
        ///   - 404 Not Found if candidate doesn't exist
        ///   - 409 Conflict if new email or phone already exists for another candidate
        ///   - 500 Internal Server Error for other exceptions
        /// Only non-null/non-empty fields in the DTO will be updated
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateCandidateDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCandidate(int id, [FromBody] UpdateCandidateDTO updateCandidateDTO) 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, errors = ModelState });
                }

                var candidate = await candidateService.UpdateCandidateAsync(id, updateCandidateDTO);
                return Ok(new { success = true, data = candidate, message = "Candidate updated successfully!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/candidates/{id}
        /// Deletes a candidate and all their associated skills from the database.
        /// Path Parameter: id - The candidate ID to delete
        /// Success Response: 200 OK with success message
        /// Error Responses:
        ///   - 404 Not Found if candidate doesn't exist
        ///   - 500 Internal Server Error for other exceptions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCandidate(int id) 
        {
            try
            {
                await candidateService.DeleteCandidateAsync(id);
                return Ok(new { success = true, message = "Candidate deleted successfully!" });
            }
            catch (KeyNotFoundException ex) 
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/candidates/{candidateId}/skills/batch
        /// Adds multiple skills to a candidate in a batch operation.
        /// Path Parameter: candidateId - The candidate to add skills to
        /// Request Body: AddSkillsDTO containing array of skill IDs to add
        /// Success Response: 200 OK with updated candidate including all skills
        /// Error Responses:
        ///   - 400 Bad Request if skillIds array is empty or null
        ///   - 404 Not Found if candidate or one/more skills don't exist
        ///   - 409 Conflict if candidate already has one or more of the specified skills
        ///   - 500 Internal Server Error for other exceptions
        /// </summary>
        /// <param name="candidateId"></param>
        /// <param name="skillIdsDTO"></param>
        /// <returns></returns>
        [HttpPost("{candidateId}/skills/batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddSkillsToCandidate(int candidateId, [FromBody] AddSkillsDTO skillIdsDTO) 
        {
            try
            {
                List<int> skillIds = skillIdsDTO.skillIds;
                if (skillIds == null || skillIds.Count == 0)
                {
                    return BadRequest(new { success = false, message = "SkillIds array cannot be empty" });
                }

                await candidateService.AddSkillsToCandidateAsync(candidateId, skillIds);
                var candidate = await candidateService.GetCandidateByIdAsync(candidateId);

                return Ok(new { success = true, data = candidate, message = "Skills added successfully!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/candidates/{candidateId}/skills/{skillId}
        /// Removes a single skill from a candidate.
        /// Path Parameters: 
        ///   - candidateId - The candidate ID
        ///   - skillId - The skill ID to remove
        /// Success Response: 200 OK with updated candidate (without the removed skill)
        /// Error Responses:
        ///   - 404 Not Found if candidate or candidate-skill association doesn't exist
        ///   - 500 Internal Server Error for other exceptions
        /// </summary>
        /// <param name="candidateId"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        [HttpDelete("{candidateId}/skills/{skillId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveSkillFromCandidate(int candidateId, int skillId) 
        {
            try
            {
                await candidateService.RemoveSkillFromCandidateAsync(candidateId, skillId);
                var candidate = await candidateService.GetCandidateByIdAsync(candidateId);

                return Ok(new { success = true, data = candidate, message = "Skill removed successfully!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
