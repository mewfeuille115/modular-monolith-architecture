using Evently.Common.Application.Authorization;
using Evently.Common.Domain;
using Evently.Modules.Users.IntegrationEvents;

namespace Evently.Modules.Users.Presentation.Users;

public sealed class GetUserPermissionsRequestConsumer(IPermissionService permissionService)
{
	public async Task<object> Handle(GetUserPermissionsRequest context)
	{
		Result<PermissionsResponse> result =
			await permissionService.GetUserPermissionsAsync(context.IdentityId);

		return result.IsSuccess
			? result.Value
			: result.Error;
	}
}
