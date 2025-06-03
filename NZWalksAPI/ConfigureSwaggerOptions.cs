using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NZWalks.API
{
    public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            this._apiVersionDescriptionProvider = apiVersionDescriptionProvider;
        }

        public void Configure(string? name, SwaggerGenOptions options)
        {
            Configure(options); // Call the other Configure method
        }

        // Configure the Swagger document for each API version
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var item in _apiVersionDescriptionProvider.ApiVersionDescriptions) // Loop through each API version
            {
                options.SwaggerDoc(item.GroupName, CreateVersionInfo(item)); // Create the Swagger document for each API version
            }
        }

        // 
        private OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "NZ Walks API", // Title of the API
                Version = description.ApiVersion.ToString(), // Version of the API
                Description = "A simple API to get information about walks in New Zealand"
            };

            return info;
        }
    }
}
