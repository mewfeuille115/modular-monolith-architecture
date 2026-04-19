using System.Reflection;
using Evently.ArchitectureTests.Abstractions;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Infrastructure;
using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.Infrastructure;
using Evently.Modules.Users.Domain.Users;
using Evently.Modules.Users.Infrastructure;
using NetArchTest.Rules;

namespace Evently.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
	[Fact]
	public void AttendanceModule_ShouldNotHaveDependencyOn_AnyOtherModule()
	{
		string[] otherModules =
		[
			EventsNamespace,
			TicketingNamespace,
			UsersNamespace,
		];

		string[] integrationEventsModules =
		[
			EventsIntegrationEventsNamespace,
			TicketingIntegrationEventsNamespace,
			UsersIntegrationEventsNamespace,
		];

		List<Assembly> attendanceAssemblies =
		[
			typeof(Attendee).Assembly,
			Modules.Attendance.Application.AssemblyReference.Assembly,
			Modules.Attendance.Presentation.AssemblyReference.Assembly,
			typeof(AttendanceModule).Assembly,
		];

		Types.InAssemblies(attendanceAssemblies)
			.That()
			.DoNotHaveDependencyOnAny(integrationEventsModules)
			.Should()
			.NotHaveDependencyOnAny(otherModules)
			.GetResult()
			.ShouldBeSuccessful();
	}

	[Fact]
	public void EventsModule_ShouldNotHaveDependencyOn_AnyOtherModule()
	{
		string[] otherModules =
		[
			AttendanceNamespace,
			TicketingNamespace,
			UsersNamespace,
		];

		string[] integrationEventsModules =
		[
			AttendanceIntegrationEventsNamespace,
			TicketingIntegrationEventsNamespace,
			UsersIntegrationEventsNamespace,
		];

		List<Assembly> eventsAssemblies =
		[
			typeof(Event).Assembly,
			Modules.Events.Application.AssemblyReference.Assembly,
			Modules.Events.Presentation.AssemblyReference.Assembly,
			typeof(EventsModule).Assembly,
		];

		Types.InAssemblies(eventsAssemblies)
			.That()
			.DoNotHaveDependencyOnAny(integrationEventsModules)
			.Should()
			.NotHaveDependencyOnAny(otherModules)
			.GetResult()
			.ShouldBeSuccessful();
	}

	[Fact]
	public void UsersModule_ShouldNotHaveDependencyOn_AnyOtherModule()
	{
		string[] otherModules =
		[
			AttendanceNamespace,
			EventsNamespace,
			TicketingNamespace,
		];

		string[] integrationEventsModules =
		[
			AttendanceIntegrationEventsNamespace,
			EventsIntegrationEventsNamespace,
			TicketingIntegrationEventsNamespace,
		];

		List<Assembly> usersAssemblies =
		[
			typeof(User).Assembly,
			Modules.Users.Application.AssemblyReference.Assembly,
			Modules.Users.Presentation.AssemblyReference.Assembly,
			typeof(UsersModule).Assembly,
		];

		Types.InAssemblies(usersAssemblies)
			.That()
			.DoNotHaveDependencyOnAny(integrationEventsModules)
			.Should()
			.NotHaveDependencyOnAny(otherModules)
			.GetResult()
			.ShouldBeSuccessful();
	}
}
