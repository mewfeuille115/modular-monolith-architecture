using Evently.Common.Application.Clock;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Evently.Modules.Events.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	public readonly IDateTimeProvider DateTimeProviderMock = Substitute.For<IDateTimeProvider>();

	private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18.1-alpine3.23")
		.WithDatabase("evently")
		.WithUsername("postgres")
		.WithPassword("postgres")
		.Build();

	private readonly RedisContainer _redisContainer = new RedisBuilder("redis:8.4.0").Build();

	private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:4.2.5-management-alpine")
		.WithUsername("guest")
		.WithPassword("guest")
		.Build();

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		Environment.SetEnvironmentVariable("ConnectionStrings:Database", _dbContainer.GetConnectionString());
		Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
		Environment.SetEnvironmentVariable("ConnectionStrings:Queue", _rabbitMqContainer.GetConnectionString());

		builder.ConfigureTestServices(services =>
		{
			services.RemoveAll<IDateTimeProvider>();

			DateTimeProviderMock.UtcNow.Returns(_ => DateTime.UtcNow);
			services.AddSingleton(DateTimeProviderMock);
		});
	}

	public async ValueTask InitializeAsync()
	{
		await _dbContainer.StartAsync();
		await _redisContainer.StartAsync();
		await _rabbitMqContainer.StartAsync();
	}

	public new async ValueTask DisposeAsync()
	{
		await _dbContainer.StopAsync();
		await _redisContainer.StopAsync();
		await _rabbitMqContainer.StopAsync();
	}
}
