using AwesomeAssertions;
using Bogus;
using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Events.CreateEvent;
using Evently.Modules.Attendance.IntegrationTests.Abstractions;

namespace Evently.Modules.Attendance.IntegrationTests.Events;

public class CreateEventTests : BaseIntegrationTest
{
	public CreateEventTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	public static TheoryData<Guid, string, string, string> InvalidData()
	{
		var faker = new Faker { Random = new Randomizer(0) };
		return new TheoryData<Guid, string, string, string>
		{
			{ Guid.Empty, faker.Music.Genre(), faker.Music.Genre(), faker.Address.StreetAddress() },
			{ faker.Random.Guid(), string.Empty, faker.Music.Genre(), faker.Address.StreetAddress() },
			{ faker.Random.Guid(), faker.Music.Genre(), string.Empty, faker.Address.StreetAddress() },
			{ faker.Random.Guid(), faker.Music.Genre(), faker.Music.Genre(), string.Empty },
			{ faker.Random.Guid(), faker.Music.Genre(), faker.Music.Genre(), faker.Address.StreetAddress() }
		};
	}

	[Theory]
	[MemberData(nameof(InvalidData))]
	public async Task Should_ReturnFailure_WhenCommandIsInvalid(
		Guid eventId,
		string title,
		string description,
		string location)
	{
		// Arrange
		var command = new CreateEventCommand(eventId, title, description, location, default, default);

		// Act
		Result result = await Sender.Send(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
	}

	[Fact]
	public async Task Should_ReturnSuccess_WhenCommandIsValid()
	{
		// Arrange
		var eventId = Guid.NewGuid();

		var command = new CreateEventCommand(
			eventId,
			Faker.Music.Genre(),
			Faker.Music.Genre(),
			Faker.Address.StreetAddress(),
			DateTime.UtcNow.AddMinutes(10),
			null);

		// Act
		Result result = await Sender.Send(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
	}
}
