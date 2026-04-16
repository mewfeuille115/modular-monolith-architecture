using Evently.Common.Domain;
using Evently.IntegrationTests.Abstractions;
using Evently.Modules.Ticketing.Application.Carts.AddItemToCart;
using Evently.Modules.Ticketing.Application.Customers.GetCustomer;
using Evently.Modules.Users.Application.Users.RegisterUser;
using FluentAssertions;
using Xunit;

namespace Evently.IntegrationTests.AddToCart;

public sealed class AddItemToCartTests : BaseIntegrationTest
{
	private const decimal Quantity = 10;

	public AddItemToCartTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task Customer_ShouldBeAbleTo_AddItemToCart()
	{
		// Register user
		var registerUserCommand = new RegisterUserCommand(
			Faker.Internet.Email(),
			Faker.Internet.Password(6),
			Faker.Name.FirstName(),
			Faker.Name.LastName());

		Result<Guid> userResult = await Sender.Send(registerUserCommand, CancellationToken.None);

		userResult.IsSuccess.Should().BeTrue();

		// Get customer
		Result<CustomerResponse> customerResult = await Poller.WaitAsync(
			TimeSpan.FromSeconds(15),
			async () =>
			{
				var query = new GetCustomerQuery(userResult.Value);

				Result<CustomerResponse> customerResult = await Sender.Send(query);

				return customerResult;
			});

		customerResult.IsSuccess.Should().BeTrue();

		// Add item to cart
		CustomerResponse customer = customerResult.Value;
		var ticketTypeId = Guid.NewGuid();

		await Sender.CreateEventAsync(Guid.NewGuid(), ticketTypeId, Quantity);

		var command = new AddItemToCartCommand(customer.Id, ticketTypeId, Quantity);

		Result result = await Sender.Send(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
	}
}
