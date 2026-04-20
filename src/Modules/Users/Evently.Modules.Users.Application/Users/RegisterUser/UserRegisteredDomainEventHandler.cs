using Evently.Common.Application.EventBus;
using Evently.Common.Application.Exceptions;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Users.Application.Users.GetUser;
using Evently.Modules.Users.Domain.Users;
using Evently.Modules.Users.IntegrationEvents;

namespace Evently.Modules.Users.Application.Users.RegisterUser;

internal sealed class UserRegisteredDomainEventHandler(
		IQueryHandler<GetUserQuery, UserResponse> handler,
		IEventBus eventBus
	) : DomainEventHandler<UserRegisteredDomainEvent>
{
	public override async Task Handle(
		UserRegisteredDomainEvent notification,
		CancellationToken cancellationToken = default)
	{
		Result<UserResponse> result = await handler.Handle(
			new GetUserQuery(notification.UserId),
			cancellationToken);

		if (result.IsFailure)
		{
			throw new EventlyException(nameof(GetUserQuery), result.Error);
		}

		await eventBus.PublishAsync(
			new UserRegisteredIntegrationEvent(
				notification.Id,
				notification.OccurredOnUtc,
				result.Value.Id,
				result.Value.Email,
				result.Value.FirstName,
				result.Value.LastName),
			cancellationToken);
	}
}
