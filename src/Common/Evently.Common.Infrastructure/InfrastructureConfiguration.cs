using Evently.Common.Application.Caching;
using Evently.Common.Application.Clock;
using Evently.Common.Application.Data;
using Evently.Common.Application.EventBus;
using Evently.Common.Infrastructure.Authentication;
using Evently.Common.Infrastructure.Authorization;
using Evently.Common.Infrastructure.Caching;
using Evently.Common.Infrastructure.Clock;
using Evently.Common.Infrastructure.Data;
using Evently.Common.Infrastructure.EventBus;
using Evently.Common.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Evently.Common.Infrastructure;

public static class InfrastructureConfiguration
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		string serviceName,
		RabbitMqSettings rabbitMqSettings,
		string databaseConnectionString,
		string redisConnectionString)
	{
		services.AddAuthenticationInternal();

		services.AddAuthorizationInternal();

		NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
		services.TryAddSingleton(npgsqlDataSource);

		services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

		services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

		services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

		services.AddQuartz(configurator =>
		{
			var scheduler = Guid.NewGuid();
			configurator.SchedulerId = $"default-id-{scheduler}";
			configurator.SchedulerName = $"default-name-{scheduler}";
		});
		services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

		try
		{
			IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
			services.TryAddSingleton(connectionMultiplexer);

			services.AddStackExchangeRedisCache(options =>
			{
				options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
			});
		}
		catch
		{
			services.AddDistributedMemoryCache();
		}

		services.TryAddSingleton<ICacheService, CacheService>();

		services.TryAddScoped<IEventBus, EventBus.EventBus>();

		services.AddSingleton<IConnection>(sp =>
		{
			var factory = new ConnectionFactory
			{
				Uri = new Uri(rabbitMqSettings.Host),
			};
			return factory.CreateConnectionAsync().GetAwaiter().GetResult();
		});

		services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(serviceName))
			.WithTracing(tracing =>
			{
				tracing
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddEntityFrameworkCoreInstrumentation()
					.AddRedisInstrumentation()
					.AddNpgsql()
					.AddSource("Wolverine");

				tracing.AddOtlpExporter();
			});

		return services;
	}
}
