using AwesomeAssertions;
using Bogus;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Categories.UpdateCategory;
using Evently.Modules.Events.Domain.Categories;
using Evently.Modules.Events.IntegrationTests.Abstractions;

namespace Evently.Modules.Events.IntegrationTests.Categories;

public class UpdateCategoryTests : BaseIntegrationTest
{
	public UpdateCategoryTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	public static TheoryData<Guid, string> InvalidCommands()
	{
		var faker = new Faker { Random = new Randomizer(0) };
		return new TheoryData<Guid, string>
		{
			{ Guid.Empty, faker.Music.Genre() },
			{ faker.Random.Guid(), string.Empty }
		};
	}

	[Theory]
	[MemberData(nameof(InvalidCommands))]
	public async Task Should_ReturnFailure_WhenCommandIsNotValid(Guid categoryId, string name)
	{
		// Arrange
		var command = new UpdateCategoryCommand(categoryId, name);

		// Act
		Result result = await Sender.Send(command, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Type.Should().Be(ErrorType.Validation);
	}

	[Fact]
	public async Task Should_ReturnFailure_WhenCategoryDoesNotExist()
	{
		// Arrange
		var command = new UpdateCategoryCommand(Guid.NewGuid(), Faker.Music.Genre());

		// Act
		Result result = await Sender.Send(command, CancellationToken.None);

		// Assert
		result.Error.Should().Be(CategoryErrors.NotFound(command.CategoryId));
	}

	[Fact]
	public async Task Should_UpdateCategory_WhenCategoryExists()
	{
		// Arrange
		Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());

		var command = new UpdateCategoryCommand(categoryId, Faker.Music.Genre());

		// Act
		Result result = await Sender.Send(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
	}
}
