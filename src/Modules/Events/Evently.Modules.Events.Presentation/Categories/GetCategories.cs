using Evently.Common.Application.Caching;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Categories.GetCategories;
using Evently.Modules.Events.Application.Categories.GetCategory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Categories;

internal sealed class GetCategories : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("categories", async (
			IQueryHandler<GetCategoriesQuery, IReadOnlyCollection<CategoryResponse>> handler,
			ICacheService cacheService,
			CancellationToken cancellationToken) =>
		{
			IReadOnlyCollection<CategoryResponse>? categoriesResponses =
				await cacheService.GetAsync<IReadOnlyCollection<CategoryResponse>>(
					"categories",
					cancellationToken);

			if (categoriesResponses is not null)
			{
				return Results.Ok(categoriesResponses);
			}

			Result<IReadOnlyCollection<CategoryResponse>> result = await handler.Handle(
				new GetCategoriesQuery(),
				cancellationToken);

			if (result.IsSuccess)
			{
				await cacheService.SetAsync(
					"categories",
					result.Value,
					cancellationToken: cancellationToken);
			}

			return result.Match(Results.Ok, ApiResults.Problem);
		})
		.RequireAuthorization(Permissions.GetCategories)
		.WithTags(Tags.Categories);
	}
}
