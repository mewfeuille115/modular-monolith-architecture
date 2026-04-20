using AwesomeAssertions;
using Bogus;
using Evently.Common.Domain;
using Evently.Modules.Users.Application.Users.RegisterUser;
using Evently.Modules.Users.Application.Users.UpdateUser;
using Evently.Modules.Users.Domain.Users;
using Evently.Modules.Users.IntegrationTests.Abstractions;

namespace Evently.Modules.Users.IntegrationTests.Users;

public class UpdateUserTests : BaseIntegrationTest
{
	public UpdateUserTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	public static TheoryData<Guid, string, string> InvalidCommands()
	{
		var faker = new Faker { Random = new Randomizer(0) };
		return new TheoryData<Guid, string, string>
		{
			{ Guid.Empty, faker.Name.FirstName(), faker.Name.LastName() },
			{ faker.Random.Guid(), string.Empty, faker.Name.LastName() },
			{ faker.Random.Guid(), faker.Name.FirstName(), string.Empty },
		};
	}

	[Theory]
	[MemberData(nameof(InvalidCommands))]
	public async Task Should_ReturnError_WhenCommandIsNotValid(Guid userId, string firstName, string lastName)
	{
		// Act
		var command = new UpdateUserCommand(userId, firstName, lastName);
		Result result = await SendCommand(command);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Type.Should().Be(ErrorType.Validation);
	}

	[Fact]
	public async Task Should_ReturnError_WhenUserDoesNotExist()
	{
		// Arrange
		var userId = Guid.NewGuid();

		var command = new UpdateUserCommand(userId, Faker.Name.FirstName(), Faker.Name.LastName());

		// Act
		Result updateResult = await SendCommand(command);

		// Assert
		updateResult.Error.Should().Be(UserErrors.NotFound(userId));
	}

	[Fact]
	public async Task Should_ReturnSuccess_WhenUserExists()
	{
		// Arrange
		var registerUserCommand = new RegisterUserCommand(
			Faker.Internet.Email(),
			Faker.Internet.Password(),
			Faker.Name.FirstName(),
			Faker.Name.LastName()
		);

		Result<Guid> result = await SendCommand<RegisterUserCommand, Guid>(registerUserCommand);

		Guid userId = result.Value;

		var updateUSerCommand = new UpdateUserCommand(
			userId,
			Faker.Name.FirstName(),
			Faker.Name.LastName()
		);

		// Act
		Result updateResult = await SendCommand(updateUSerCommand);

		// Assert
		updateResult.IsSuccess.Should().BeTrue();
	}
}
