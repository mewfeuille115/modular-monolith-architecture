using Bogus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Evently.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
	private readonly IServiceScope _scope;
	protected readonly ISender Sender;
	protected readonly Faker Faker = new();
	private bool _disposed;

	protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
	{
		_scope = factory.Services.CreateScope();
		Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
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
