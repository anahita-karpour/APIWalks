using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.CustomActionFilters;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;

namespace NZWalks.API.Controllers
{
    // /api/walks
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class WalksController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IWalkRepository _walkRepository;

        public WalksController(IMapper mapper, IWalkRepository walkRepository)
        {
            _mapper = mapper;
            _walkRepository = walkRepository;
        }

        // CREATE Walk
        // POST: /api/walks
        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Admin, Writer")]
        public async Task<IActionResult> Create([FromBody] AddWalkRequestDto addWalkRequestDto)
        {
            //if (ModelState.IsValid)
            //{
            // Map (addWalkRequestDto)DTO to Walk Domain Model
            var walkDomainModel = _mapper.Map<Walk>(addWalkRequestDto);
            await _walkRepository.CreatAsync(walkDomainModel);

            // Map Domain model to DTO
            return Ok(_mapper.Map<WalkDto>(walkDomainModel));
            //}
            //return BadRequest(ModelState);  // return the ModelState with the BadRequest() method because it will have a collection 
            // of errors
        }

        // GET ALL WALKS
        // GET: /api/walks?filterOn=Name&filterQuery=Track&sortBy=Name&isAscending=true&pageNumber=1&pageSize=10
        //  --> GET https:localhost:portnumber/api/regions
        [HttpGet]                              //The return type of every async method in ASP.NET is a task
                                               //Therefore every return type is wrapped inside a task
        [Authorize(Roles = "Writer, Reader")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? filterOn, [FromQuery] string? filterQuery,    // Filtering
            [FromQuery] string? sortBy, [FromQuery] bool? isAscending,        // Sorting
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 1000)// Pagination
        {
            //try
            //{
            //throw new Exception("This was the error message");
            // GET data from Database - Domain Models           
            var walkDomainModel = await _walkRepository.GetAllAsync(filterOn, filterQuery, sortBy,
                isAscending ?? true, pageNumber, pageSize);
            //var walkDto = _mapper.Map<List<WalkDto>>(walkDomainModel);

            // Create an exception to test the ExceptionHandlerMiddleware
            //throw new Exception("This was the exception");

            //return Ok();
            return Ok(_mapper.Map<List<WalkDto>>(walkDomainModel));
            //}
            //catch (Exception e)
            //{
            //    // log this exception
            //    return Problem(detail: e.Message, statusCode: 500); //500 Internal Server Error
            //    //return Problem(detail: e.Message, null, (int)HttpStatusCode.InternalServerError); //500 Internal Server Error

            //}
        }

        // GET: /api/Walks/{id} 
        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer, Reader")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            // GET data from Database / Domain Model
            var walkDomainModel = await _walkRepository.GetByIdAsync(id);
            if (walkDomainModel == null)
            {
                return NotFound(); //404 response
            }
            // Map DomainModel to Dto
            return Ok(_mapper.Map<WalkDto>(walkDomainModel));
        }

        // Update Walk by Id
        // PUT: /api/Walks/{id}
        [HttpPut]
        [Route("{id:Guid}")]
        [ValidateModel]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateWalkRequestDto updateWalkRequestDto)
        {
            //if (ModelState.IsValid)
            //{
            // Map Dto to Domain Model
            var walkDomainModel = _mapper.Map<Walk>(updateWalkRequestDto);

            await _walkRepository.UpdateAsync(id, walkDomainModel);

            if (walkDomainModel == null)
            {
                return NotFound();
            }

            // Map Domain Model to Dto
            var updatedWalk = _mapper.Map<WalkDto>(walkDomainModel);

            return Ok(updatedWalk);
            //}
            //return BadRequest(ModelState);
        }

        // DELETE: /api/Walks/{id}
        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            // Map Dto to Domain Model
            var walkDomainModel = await _walkRepository.DeleteAsync(id);

            if (walkDomainModel == null)
            {
                return NotFound();
            }

            // Map Domain Model to Dto
            var walkDto = _mapper.Map<WalkDto>(walkDomainModel);
            return Ok(walkDto);
        }


    }
}
