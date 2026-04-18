using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Evently.Modules.Ticketing.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
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
