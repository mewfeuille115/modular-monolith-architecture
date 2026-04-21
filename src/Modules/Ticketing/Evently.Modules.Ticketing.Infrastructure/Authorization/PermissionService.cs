using Evently.Common.Application.Authorization;
using Evently.Common.Application.Caching;
using Evently.Common.Domain;
using Evently.Modules.Users.IntegrationEvents;
using Wolverine;

namespace Evently.Modules.Ticketing.Infrastructure.Authorization;

internal sealed class PermissionService(
	IMessageBus messageBus,
	ICacheService cacheService
) : IPermissionService
{
	private static readonly Error NotFound = Error.NotFound(nameof(PermissionService), "The user was not found");
	private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

	public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
	{
		PermissionsResponse? permissionsResponse =
			await cacheService.GetAsync<PermissionsResponse>(CreateCacheKey(identityId));

		if (permissionsResponse is not null)
		{
			return permissionsResponse;
		}

		var request = new GetUserPermissionsRequest(identityId);

		PermissionsResponse? response = await messageBus.InvokeAsync<PermissionsResponse>(request);

		if (response is null)
		{
			return Result.Failure<PermissionsResponse>(NotFound);
		}

		await cacheService.SetAsync(CreateCacheKey(identityId), response, CacheExpiration);

		return response;
	}

	private static string CreateCacheKey(string identityId) => $"user-permissions:{identityId}";
}
