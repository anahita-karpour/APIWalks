using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;
using NZWalks.API.Repositories;

namespace NZWalks.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;
        private readonly IConfiguration _configuration; // Inject configuration
        private readonly IMapper _mapper;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(
            IImageRepository imageRepository,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<ImagesController> logger)
        {
            _imageRepository = imageRepository;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        // POST: /api/Images/Upload
        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> Upload([FromForm] ImageUploadRequestDto request)
        {
            ValidateFileUpload(request);

            if (ModelState.IsValid)
            {
                // Generate a unique file name on the server
                // File Name Security: Be cautious about directly using the client-provided file name for saving files.
                // It's often better to generate unique file names on the server to prevent potential security vulnerabilities (e.g., path traversal).
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";
                var filePath = Path.Combine(_configuration["FileUpload:FileUploadPath"], uniqueFileName); // Get path from config
                // The colon (:) is used to navigate the hierarchy of your JSON configuration

                // convert DTO to Domain model (Use AutoMapper to map from DTO to Domain model)
                //var imageDomainModel = new Image
                //{
                //    File = request.File, // You might not need to store the IFormFile in the domain model
                //    FileExtension = Path.GetExtension(request.File.FileName),
                //    FileSizeInBytes = request.File.Length,
                //    FileName = uniqueFileName, // Use the unique name
                //    FileDescription = request.FileDescription,
                //    FilePath = filePath // Store the actual file path
                //};

                // Use AutoMapper to map from DTO to Domain model those properties with matching names (like FileName, FileDescription)
                var imageDomainModel = _mapper.Map<Image>(request);

                // Manually set properties from the IFormFile from the DTO - these won't be mapped automatically
                // access IFormFile properties and set Domain Model Properties:
                // access the request.File (which is the IFormFile from the DTO) to get the properties you need

                imageDomainModel.FileExtension = Path.GetExtension(request.File.FileName);
                imageDomainModel.FileSizeInBytes = request.File.Length;

                // Note: We are still assigning the IFormFile here, but it won't be saved to the database
                imageDomainModel.File = request.File;
                // generate a unique file name and the construct the filePath from the configuration
                imageDomainModel.FileName = uniqueFileName;
                imageDomainModel.FilePath = filePath;


                try
                {
                    await _imageRepository.Upload(imageDomainModel); // Repository should handle saving the physical file

                    //var imageResponseDto = new ImageUploadResponseDto // Create a response DTO
                    //{
                    //    Id = imageDomainModel.Id,
                    //    FileName = imageDomainModel.FileName,
                    //    FileDescription = imageDomainModel.FileDescription,
                    //    FilePath = imageDomainModel.FilePath
                    //};

                    // Create a response DTO
                    var imageResponseDto = _mapper.Map<ImageUploadResponseDto>(imageDomainModel); // Map domain model to response DTO
                                                                                                  //_mapper.Map<TDestination>(source)
                                                                                                  //TDestination: This is the type you want to map to (the target type).
                                                                                                  //source: This is the instance you are mapping from(the source object).

                    return Ok(imageResponseDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload image.");
                    return StatusCode(500, "Failed to upload image.");
                }
            }
            return BadRequest(ModelState);
        }

        private void ValidateFileUpload(ImageUploadRequestDto request)
        {
            if (request.File != null)
            {
                //var allowedExtensions = _configuration.GetSection("FileUpload")["AllowedExtensions"].Split(','); // Get from config
                //  Get<string[]>: binds the array of values to a string[]
                var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>();
                if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName).ToLowerInvariant()))
                {
                    ModelState.AddModelError("file", $"Unsupported file extension. Allowed extensions: {string.Join(", ", allowedExtensions)}");
                }

                // directly retrieves the value of MaxFileSizeInBytes as a long
                var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSizeInBytes", 10485760); // Get from config
                if (request.File.Length > maxFileSize)
                {
                    ModelState.AddModelError("File", $"File size exceeds the limit of {maxFileSize / (1024 * 1024)} MB.");
                }
            }
            else
            {
                ModelState.AddModelError("File", "Please select a file.");
            }
        }
    }
}