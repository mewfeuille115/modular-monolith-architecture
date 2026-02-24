using FluentAssertions;

namespace Evently.Modules.Events.ArchitectureTests.Abstractions;

internal static class TestResultExtensions
{
	internal static void ShouldBeSuccessful(this NetArchTest.Rules.TestResult testResult)
	{
		testResult.FailingTypes?.Should().BeEmpty();
	}
}
