using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Categories.ArchiveCategory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Categories;

internal sealed class ArchiveCategory : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPut("categories/{id}/archive", async (
			Guid id,
			ICommandHandler<ArchiveCategoryCommand> handler,
			CancellationToken cancellationToken) =>
		{
			Result result = await handler.Handle(
				new ArchiveCategoryCommand(id),
				cancellationToken);

			return result.Match(() => Results.Ok(), ApiResults.Problem);
		})
		.RequireAuthorization(Permissions.ModifyCategories)
		.WithTags(Tags.Categories);
	}
}
