using System.Diagnostics;
using Evently.Common.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Evently.Common.Application.Behaviors;

internal sealed partial class RequestLoggingPipelineBehavior<TRequest, TResponse>(
		ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger
	) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : class
	where TResponse : Result
{
	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		string moduleName = GetModuleName(typeof(TRequest).FullName!);
		string requestName = typeof(TRequest).Name;

		Activity.Current?.AddTag("request.module", moduleName);
		Activity.Current?.AddTag("request.name", requestName);

		using (LogContext.PushProperty("Module", moduleName))
		{
			LogProcessingRequest(requestName);

			TResponse result = await next(cancellationToken);

			if (result.IsSuccess)
			{
				LogCompletedRequest(requestName);
			}
			else
			{
				using (LogContext.PushProperty("Error", result.Error, true))
				{
					LogCompletedRequestWithError(requestName);
				}
			}

			return result;
		}
	}

	private static string GetModuleName(string requestName) => requestName.Split('.')[2];

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Processing request {RequestName}")]
	private partial void LogProcessingRequest(string requestName);

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Completed request {RequestName}")]
	private partial void LogCompletedRequest(string requestName);

	[LoggerMessage(
		Level = LogLevel.Error,
		Message = "Completed request {RequestName} with error")]
	private partial void LogCompletedRequestWithError(string requestName);
}
