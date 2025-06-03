using NZWalks.API.Data;
using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    /// <summary>
    /// SQLImageRepository is a class that implements the IImageRepository interface.
    /// saves files locally, the metadata about the image
    /// (file name, path, size, description) is being stored in your SQL database via the NZWalksDbContext.
    /// </summary>
    public class SQLImageRepository : IImageRepository
    {
        // The IWebHostEnvironment interface provides information about the web hosting environment an application is running in.

        private readonly IWebHostEnvironment _webHostEnvironement;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NZWalksDbContext _dbContext;
        private readonly ILogger<SQLImageRepository> _logger;

        public SQLImageRepository(
            IWebHostEnvironment webHostEnvironement,
            IHttpContextAccessor httpContextAccessor,
            NZWalksDbContext dbContext,
            ILogger<SQLImageRepository> logger)
        {
            _webHostEnvironement = webHostEnvironement;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<Image> Upload(Image image)
        {
            try
            {
                // 1- Generate a unique file name
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.File.FileName)}";

                // 2- creates a local file path for saving the upload image file  
                var localFilePath =
                    Path.Combine(_webHostEnvironement.ContentRootPath, //webHostEnvironment.ContentRootPath is a fundamental property
                                                                       //in ASP.NET Core that provides the absolute physical path to the content
                                                                       //root directory of your web application. 
                    "Images",                   // subdirectory/ folder within content root
                    uniqueFileName);// { image.FileExtension}"); //combined fileName and FileExtension

                // 3- Save the image to the local file system with error handling

                // The using statement ensures that the FileStream object is properly disposed of when it's no longer needed.
                // This is crucial because FileStream uses unmanaged resources (file handles), and failing to dispose of them
                // can lead to resource leaks.
                try
                {
                    using var stream = new FileStream(
                        localFilePath,      // This is the absolute file path to the file that you want to create or write to.
                        FileMode.Create     // Creates a new file. If the file already exists, it will be overwritten
                        );
                    await image.File.CopyToAsync(stream);   // Copy the uploaded image to the file stream
                }
                catch (IOException ex)
                {
                    _logger.LogError($"Error saving image to {localFilePath}: {ex.Message}");
                    return null; // Or throw a specific exception
                }

                // 3- URL path (that we are going to upload and return to the user) construction
                // https://localhost:1234/images/image.jpg
                // Scheme://Host:PathBase/FileName + FileExtension
                //This is the url path that we are going to upload to the table and also return it to the user
                var urlFilePath =
                        $"{_httpContextAccessor.HttpContext.Request.Scheme}://" + //Scheme in HttpContext refers to Http or Https
                        $"{_httpContextAccessor.HttpContext.Request.Host}" +      //Host refers to the website URL: localhost
                        $"{_httpContextAccessor.HttpContext.Request.PathBase}" +  //PathBase referes to the name after ':' here: 123
                        $"/Images/{uniqueFileName}";   //image.FileExtension      // Images folder so: /Images
                                                       //Then the name of the file: image.FileName
                                                       //Along with the file extension: image.FileExtension
                image.FileName = uniqueFileName; // Store the unique file name in the database
                image.FilePath = urlFilePath;

                // 4- Database Operations with error handling
                try
                {
                    // Add Image to the Images table in the database
                    await _dbContext.Images.AddAsync(image);              // nZWalksDbContext add the new image we
                                                                          // are passing to the Image table
                    await _dbContext.SaveChangesAsync();                  // Save the changes
                    return image;                                          // Return the saved image object
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving image to database: {ex.Message}");
                    // Optionally, delete the locally saved file if database saving fails
                    if (File.Exists(localFilePath))
                    {
                        try
                        {
                            File.Delete(localFilePath);
                            _logger.LogInformation($"Successfully deleted orphaned file: {localFilePath}");
                        }
                        catch (IOException deleteEx)
                        {
                            _logger.LogError($"Error deleting orphaned file {localFilePath}: {deleteEx.Message}");
                        }
                    }
                    return null;
                }
            }
            catch (Exception generalEx)
            {
                _logger.LogError($"General error during image upload: {generalEx.Message}");
                return null; // Or throw a specific exception
            }

        }
    }
}
