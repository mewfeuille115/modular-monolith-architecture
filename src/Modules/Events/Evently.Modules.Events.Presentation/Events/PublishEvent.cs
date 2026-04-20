using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.PublishEvent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class PublishEvent : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPut("events/{id}/publish", async (
			Guid id,
			ICommandHandler<PublishEventCommand> handler,
			CancellationToken cancellationToken) =>
		{
			Result result = await handler.Handle(new PublishEventCommand(id), cancellationToken);

			return result.Match(Results.NoContent, ApiResults.Problem);
		})
		.RequireAuthorization(Permissions.ModifyEvents)
		.WithTags(Tags.Events);
	}
}
