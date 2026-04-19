using AwesomeAssertions;
using Evently.Common.Domain;
using Evently.IntegrationTests.Abstractions;
using Evently.Modules.Attendance.Application.Attendees.GetAttendee;
using Evently.Modules.Users.Application.Users.RegisterUser;
using Xunit;

namespace Evently.IntegrationTests.RegisterUser;

public class RegisterUserTests : BaseIntegrationTest
{
	public RegisterUserTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task RegisterUser_Should_PropagateToAttendanceModule()
	{
		// Register user
		var command = new RegisterUserCommand(
			Faker.Internet.Email(),
			Faker.Internet.Password(6),
			Faker.Name.FirstName(),
			Faker.Name.LastName());

		Result<Guid> userResult = await Sender.Send(command, CancellationToken.None);

		userResult.IsSuccess.Should().BeTrue();

		// Get customer
		Result<AttendeeResponse> attendeeResult = await Poller.WaitAsync(
			TimeSpan.FromSeconds(15),
			async () =>
			{
				var query = new GetAttendeeQuery(userResult.Value);

				Result<AttendeeResponse> customerResult = await Sender.Send(query);

				return customerResult;
			});

		// Assert
		attendeeResult.IsSuccess.Should().BeTrue();
		attendeeResult.Value.Should().NotBeNull();
	}
}
