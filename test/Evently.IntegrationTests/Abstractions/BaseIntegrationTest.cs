using Bogus;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Evently.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
	private readonly IServiceScope _scope;
	protected readonly Faker Faker = new();
	private bool _disposed;

	protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
	{
		_scope = factory.Services.CreateScope();
	}

	protected async Task<Result<TResult>> SendCommand<TCommand, TResult>(TCommand command)
	   where TCommand : ICommand<TResult>
	{
		ICommandHandler<TCommand, TResult> handler = _scope.ServiceProvider
			.GetRequiredService<ICommandHandler<TCommand, TResult>>();

		return await handler.Handle(command, CancellationToken.None);
	}

	public async Task<Result> SendCommand<TCommand>(TCommand command)
		where TCommand : ICommand
	{
		ICommandHandler<TCommand> handler = _scope.ServiceProvider
			.GetRequiredService<ICommandHandler<TCommand>>();

		return await handler.Handle(command, CancellationToken.None);
	}

	protected async Task<Result<TResult>> SendQuery<TQuery, TResult>(TQuery query)
		where TQuery : IQuery<TResult>
	{
		IQueryHandler<TQuery, TResult> handler = _scope.ServiceProvider
			.GetRequiredService<IQueryHandler<TQuery, TResult>>();

		return await handler.Handle(query, CancellationToken.None);
	}


	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_scope.Dispose();
			}

			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
