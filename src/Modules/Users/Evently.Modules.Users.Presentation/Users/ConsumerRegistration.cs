using MassTransit;

namespace Evently.Modules.Users.Presentation.Users;

public static class ConsumerRegistration
{
    public static void AddConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
    {
        registrationConfigurator.AddConsumer<GetUserPermissionsRequestConsumer>()
            .Endpoint(c => c.InstanceId = instanceId);
    }
}
