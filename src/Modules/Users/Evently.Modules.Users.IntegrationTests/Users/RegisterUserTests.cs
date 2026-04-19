using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Bogus;
using Evently.Modules.Users.IntegrationTests.Abstractions;
using Evently.Modules.Users.Presentation.Users;

namespace Evently.Modules.Users.IntegrationTests.Users;

public class RegisterUserTests : BaseIntegrationTest
{
	public RegisterUserTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	public static TheoryData<string, string, string, string> InvalidRequests()
	{
		var faker = new Faker { Random = new Randomizer(0) };
		return new TheoryData<string, string, string, string>
		{
			{ string.Empty, faker.Internet.Password(), faker.Name.FirstName(), faker.Name.LastName() },
			{ faker.Internet.Email(), string.Empty, faker.Name.FirstName(), faker.Name.LastName() },
			{ faker.Internet.Email(), "12345", faker.Name.FirstName(), faker.Name.LastName() },
			{ faker.Internet.Email(), faker.Internet.Password(), string.Empty, faker.Name.LastName() },
			{ faker.Internet.Email(), faker.Internet.Password(), faker.Name.FirstName(), string.Empty },
		};
	}

	[Theory]
	[MemberData(nameof(InvalidRequests))]
	public async Task Should_ReturnBadRequest_WhenRequestIsNotValid(
		string email,
		string password,
		string firstName,
		string lastName)
	{
		// Arrange
		var request = new RegisterUser.Request
		{
			Email = email,
			Password = password,
			FirstName = firstName,
			LastName = lastName,
		};

		// Act
		HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
			"users/register",
			request,
			CancellationToken.None);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Should_ReturnOk_WhenRequestIsValid()
	{
		// Arrange
		var request = new RegisterUser.Request
		{
			Email = "create@test.com",
			Password = Faker.Internet.Password(),
			FirstName = Faker.Name.FirstName(),
			LastName = Faker.Name.LastName(),
		};

		// Act
		HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
			"users/register",
			request,
			CancellationToken.None);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task Should_ReturnAccessToken_WhenUserIsRegistered()
	{
		// Arrange
		var request = new RegisterUser.Request
		{
			Email = "token@test.com",
			Password = Faker.Internet.Password(),
			FirstName = Faker.Name.FirstName(),
			LastName = Faker.Name.LastName(),
		};

		await HttpClient.PostAsJsonAsync(
			"users/register",
			request,
			CancellationToken.None);

		// Act
		string accessToken = await GetAccessTokenAsync(request.Email, request.Password);

		// Assert
		accessToken.Should().NotBeEmpty();
	}
}
