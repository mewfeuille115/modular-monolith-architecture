using AwesomeAssertions;
using Evently.Common.Domain;
using Evently.Modules.Users.Application.Users.GetUser;
using Evently.Modules.Users.Application.Users.RegisterUser;
using Evently.Modules.Users.Domain.Users;
using Evently.Modules.Users.IntegrationTests.Abstractions;

namespace Evently.Modules.Users.IntegrationTests.Users;

public class GetUserTests : BaseIntegrationTest
{
	public GetUserTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task Should_ReturnError_WhenUserDoesNotExist()
	{
		// Arrange
		var userId = Guid.NewGuid();

		var query = new GetUserQuery(userId);

		// Act
		Result<UserResponse> userResult =
			await SendQuery<GetUserQuery, UserResponse>(query);

		// Assert
		userResult.Error.Should().Be(UserErrors.NotFound(userId));
	}

	[Fact]
	public async Task Should_ReturnUser_WhenUserExists()
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

		var query = new GetUserQuery(userId);

		// Act
		Result<UserResponse> userResult =
			await SendQuery<GetUserQuery, UserResponse>(query);

		// Assert
		userResult.IsSuccess.Should().BeTrue();
		userResult.Value.Should().NotBeNull();
	}
}
