using AwesomeAssertions;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.UnitTests.Abstractions;

namespace Evently.Modules.Ticketing.UnitTests.Customers;

public class CustomerTests : BaseTest
{
	[Fact]
	public void Create_ShouldReturnValue_WhenCustomerIsCreated()
	{
		//Act
		Result<Customer> result = Customer.Create(
			Guid.NewGuid(),
			Faker.Internet.Email(),
			Faker.Name.FirstName(),
			Faker.Name.LastName());
		//Assert
		result.Value.Should().NotBeNull();
	}
}
