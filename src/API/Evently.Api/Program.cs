using Evently.Api.Extensions;
using Evently.Api.Middleware;
using Evently.Api.OpenTelemetry;
using Evently.Common.Application;
using Evently.Common.Infrastructure;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Infrastructure.EventBus;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Attendance.Infrastructure;
using Evently.Modules.Events.Infrastructure;
using Evently.Modules.Users.Infrastructure;
using HealthChecks.UI.Client;
using JasperFx.Resources;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddApplication([
	Evently.Modules.Attendance.Application.AssemblyReference.Assembly,
	Evently.Modules.Events.Application.AssemblyReference.Assembly,
	Evently.Modules.Users.Application.AssemblyReference.Assembly,
]);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database");
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache");
var rabbitMqSettings = new RabbitMqSettings(builder.Configuration.GetConnectionStringOrThrow("Queue"));

builder.Services.AddInfrastructure(
	DiagnosticsConfig.ServiceName,
	rabbitMqSettings,
	databaseConnectionString,
	redisConnectionString);

builder.Host.UseWolverine(options =>
{
	options.PersistMessagesWithPostgresql(databaseConnectionString);

	options.UseRabbitMq(rabbitMqSettings.Host)
		.AutoProvision()
		.UseConventionalRouting();

	options.Services.AddResourceSetupOnStartup();

	options.Policies.DisableConventionalLocalRouting();

	options.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
	options.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
	options.Durability.Mode = DurabilityMode.Solo;

	AttendanceModule.ConfigureWolverine(options);
	EventsModule.ConfigureWolverine(options);
	UsersModule.ConfigureWolverine(options);
});

Uri keyCloakHealthUrl = builder.Configuration.GetKeyCloakHealthUrl();

builder.Services.AddHealthChecks()
	.AddNpgSql(databaseConnectionString)
	.AddRedis(redisConnectionString)
	.AddRabbitMQ()
	.AddUrlGroup(keyCloakHealthUrl);

builder.Configuration.AddModuleConfiguration(["attendance", "events", "users"]);

builder.Services.AddAttendanceModule(builder.Configuration);
builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();

	app.ApplyMigrations();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
	ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.UseLogContextTraceLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync();
