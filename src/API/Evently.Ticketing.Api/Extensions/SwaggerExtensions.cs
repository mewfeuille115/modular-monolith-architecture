using Microsoft.OpenApi;

namespace Evently.Ticketing.Api.Extensions;

internal static class SwaggerExtensions
{
	internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
	{
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Evently Ticketing API",
				Version = "v1",
				Description = "Evently Ticketing API built using the modular monolith architecture."
			});

			options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
		});

		return services;
	}
}
