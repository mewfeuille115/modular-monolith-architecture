namespace Evently.Modules.Attendance.Application.EventStatistics.GetEventStatistics;

public sealed record EventStatisticsResponse
{
	public Guid EventId { get; init; }
	public string Title { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public string Location { get; init; } = string.Empty;
	public DateTime StartsAtUtc { get; init; }
	public DateTime? EndsAtUtc { get; init; }
	public int TicketsSold { get; init; }
	public int AttendeesCheckedIn { get; init; }
	public string[] DuplicateCheckInTickets { get; init; } = [];
	public string[] InvalidCheckInTickets { get; init; } = [];
}
