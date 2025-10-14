using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.CustomActionFilters;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;
using System.Text.Json;


namespace NZWalksAPI.Controllers
{
    // https://localhost:1234/api/regions
    [Route("api/v{version:apiVersion}/[controller]")]  // whenever a user enters this route along with the application URL
    //[Route("api/[controller]")]  // whenever a user enters this route along with the application URL
    [ApiController]              // it will be pointed to the regions controller -- you can replace "api/[controller]" with
                                 // "api/regions" and it is the same as [controller] and the location is pointing to 
                                 // https://localhost:1234/api/regions. And whenever anyone enters this URL, it will point 
                                 // to this controller.
                                 // We create Action methods to be able to do CRUD operation at this endpoint (resource)

    //[Authorize]                  // this means any method inside this class has to be accessed through an authenticated user
    // And this is telling that not any public user can access these resources
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext _dbContext;
        private readonly IRegionRepository _regionRepository;
        private readonly IMapper _autoMapper;
        private readonly ILogger<RegionsController> _logger;

        public RegionsController(
            NZWalksDbContext dbContext,
            IRegionRepository regionRepository,
            IMapper autoMapper,
            ILogger<RegionsController> logger)
        {
            _dbContext = dbContext;
            _regionRepository = regionRepository;
            _autoMapper = autoMapper;
            _logger = logger;
        }
        //public RegionsController(IRegionRepository regionRepository)
        //{
        //    this.regionRepository = regionRepository;
        //}

        // GET ALL REGIONS
        // GET: https:localhost:portnumber/api/regions
        [HttpGet]   //The return type of every async method in ASP.NET is a task
                    //Therefore every return type is wrapped inside a task
                    //[Authorize(Roles = "Reader")] // this is a role based authorization
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> GetAllV1()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                Console.WriteLine($"User is authenticated: {HttpContext.User.Identity.Name}");
                foreach (var claim in HttpContext.User.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("User is NOT authenticated.");
            }
            try
            {
                // GET Data from Database - Domain Models           //to async a method, we have to await on all calls that we have to make
                var regionsDomain = await _regionRepository.GetAllAsync();

                //Map Domain Models to DTO V1 - using AutoMapper
                var regionsDto = _autoMapper.Map<List<RegionDtoV1>>(regionsDomain);

                // Return DTOs to the client
                _logger.LogInformation($"GetAll Regions request with data was successful: {JsonSerializer.Serialize(regionsDomain)}"); // this is a log message
                return Ok(regionsDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                //return StatusCode(500, "Internal Server Error");

                throw;
            }
        }

        // GET ALL REGIONS
        // GET: https:localhost:portnumber/api/regions
        [HttpGet]   //The return type of every async method in ASP.NET is a task
                    //Therefore every return type is wrapped inside a task
                    //[Authorize(Roles = "Reader")] // this is a role based authorization
        [MapToApiVersion("2.0")]
        [Authorize]
        public async Task<IActionResult> GetAllV2()
        {
            try
            {
                // GET Data from Database - Domain Models           //to async a method, we have to await on all calls that we have to make;
                var regionsDomain = await _regionRepository.GetAllAsync();

                //Map Domain Models to DTO V2 - using AutoMapper
                var regionsDto = _autoMapper.Map<List<RegionDtoV2>>(regionsDomain);

                // Return DTOs to the client
                _logger.LogInformation($"GetAll Regions request with data was successful: {JsonSerializer.Serialize(regionsDomain)}"); // this is a log message
                return Ok(regionsDto);
                //return Ok(regionsDomain);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                //return StatusCode(500, "Internal Server Error");

                throw;
            }
        }

        // GET Single Region by ID
        // GET: https://localhost:portnumber/api/regions/{id}
        [HttpGet]
        [Route("{id:Guid}")] //ensure this id property matches the id property in the method
                             //the id property will get mapped from the route to an input parameter over in the method
                             //id: Guid will make it type safe.
                             //[Authorize(Roles = "Reader")]
        [Authorize(Roles = "Reader, Writer, Admin")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            //the find method only takes the primary key so you won't be able to use Code, name or image
            //var region = dbContext.Regions.Find(id);
            // or
            // GET Region Domain Model from Database
            //var regionDomain = await dbContext.Regions.FirstOrDefault(x => x.Id == id);  //if you are passing Code in the method and route you could use x.Code here 
            //var regionDomain = await dbContext.Regions.FirstOrDefaultAsync(x => x.Id == id);
            var regionDomain = await _regionRepository.GetRegionByIdAsync(id);
            if (regionDomain == null)
            {
                return NotFound(); //which is a 404
            }
            // Map/Convert regions domain model to region DTO
            //var regionDto = new RegionDto()
            //{
            //    Id = regionDomain.Id,
            //    Code = regionDomain.Code,
            //    Name = regionDomain.Name,
            //    RegionImageUrl = regionDomain.RegionImageUrl
            //};

            var regionDto = _autoMapper.Map<RegionDtoV1>(regionDomain);

            // Return DTO back to client
            return Ok(regionDto);
        }


        // POST To Create New Region
        // POST: https://localhost:portnumber/api/regions/
        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Writer, Admin")]
        public async Task<IActionResult> Create([FromBody] AddRegionRequestDto addRegionRequestDTO)
        {
            //if (ModelState.IsValid)
            //{
            //Map or Convert DTO to Domain Model
            //var regionDomainModel = new Region
            //{
            //    Code = addRegionRequestDTO.Code,
            //    Name = addRegionRequestDTO.Name,
            //    RegionImageUrl = addRegionRequestDTO.RegionImageUrl
            //};
            var regionDomainModel = _autoMapper.Map<Region>(addRegionRequestDTO);

            //Use Domain Model to create Region
            //dbContext.Regions.Add(regionDomainModel);
            //await dbContext.Regions.AddAsync(regionDomainModel);

            //dbContext.SaveChanges();
            //Use Domain model to create Region
            regionDomainModel = await _regionRepository.CreateRegionAsync(regionDomainModel);

            //Map Domain model back to DTO
            //var regionDto = new RegionDto
            //{
            //    Id = regionDomainModel.Id,
            //    Code = regionDomainModel.Code,
            //    Name = regionDomainModel.Name,
            //    RegionImageUrl = regionDomainModel.RegionImageUrl
            //};
            var regionDto = _autoMapper.Map<RegionDtoV1>(regionDomainModel);

            return CreatedAtAction(nameof(GetById), new { id = regionDto.Id }, regionDto); //The create method returns a 201 response.
                                                                                           //}
                                                                                           //return BadRequest(ModelState);
        }

        // UPDATE a region
        // PUT: https://localhost:portnumber/api/regions/{id}
        [HttpPut]
        [Route("{id:guid}")] //making it type safe by adding guid as well
        [ValidateModel]
        [Authorize(Roles = "Writer, Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateRegionRequestDto updateRegionRequestDto)
        {
            //if (ModelState.IsValid)
            //{
            // Map DTO to Domain Model
            //var regionDomainModel = new Region
            //{
            //    Code = updateRegionRequestDto.Code,
            //    Name = updateRegionRequestDto.Name,
            //    RegionImageUrl = updateRegionRequestDto.RegionImageUrl
            //};
            var regionDomainModel = _autoMapper.Map<Region>(updateRegionRequestDto);


            regionDomainModel = await _regionRepository.UpdateRegionAsync(id, regionDomainModel);

            // Check if region exists
            //we use the id to check if the region exits, if it does then we update the region with the updateRegionRequestDto
            //var regionDomainModel = dbContext.Regions.FirstOrDefault(x => x.Id == id);//if the id is incorrect we will have null back 
            //var regionDomainModel = await dbContext.Regions.FirstOrDefaultAsync(x => x.Id == id);
            ////otherwise we will have a region back
            if (regionDomainModel == null)
            {
                return NotFound();
            }

            ////Map DTO to Domain model and update the value
            //regionDomainModel.Code = updateRegionRequestDto.Code;
            //regionDomainModel.Name = updateRegionRequestDto.Name;
            //regionDomainModel.RegionImageUrl = updateRegionRequestDto.RegionImageUrl;

            ////dbContext.SaveChanges();
            //await dbContext.SaveChangesAsync();


            //convert Domain Model to DTO
            //var regionDto = new RegionDto
            //{
            //    Id = regionDomainModel.Id,
            //    Code = regionDomainModel.Code,
            //    Name = regionDomainModel.Name,
            //    RegionImageUrl = regionDomainModel.RegionImageUrl
            //};

            var regionDto = _autoMapper.Map<RegionDtoV1>(regionDomainModel);

            return Ok(regionDto);
            //}
            //return BadRequest(ModelState);
        }


        // Delete Region
        // DELETE: https://localhost:portnumber/api/regions/{id}

        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize(Roles = "Writer, Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            //var regionDomainModel = dbContext.Regions.FirstOrDefault(x => x.Id == id);
            //var regionDomainModel = await dbContext.Regions.FirstOrDefaultAsync(x => x.Id == id);
            var regionDomainModel = await _regionRepository.DeleteRegionAsync(id);

            if (regionDomainModel == null)
            {
                return NotFound(); //404 page
            }
            //////dbcontext.regions.remove(regiondomainmodel);
            //////dbcontext.savechanges();
            //dbContext.Regions.Remove(regionDomainModel);        //There is no async remove 
            //await dbContext.SaveChangesAsync();



            // if return deleted Region back
            //map domain model to Dto
            //var regionDto = new RegionDto
            //{
            //    Code = regionDomainModel.Code,
            //    Name = regionDomainModel.Name,
            //    RegionImageUrl = regionDomainModel.RegionImageUrl
            //};

            // return deleted region back
            // map Domain Model to DTO
            //var regionDto = new RegionDto
            //{
            //    Id = regionDomainModel.Id,
            //    Code = regionDomainModel.Code,
            //    Name = regionDomainModel.Name,
            //    RegionImageUrl = regionDomainModel.RegionImageUrl
            //};

            var regionDto = _autoMapper.Map<RegionDtoV1>(regionDomainModel);

            return Ok(regionDto);
        }
    }
}
