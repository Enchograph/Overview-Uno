namespace Overview.Client.Application.Home;

public interface ITimeSelectionService
{
    Task<TimeSelectionSnapshot> BuildMonthSnapshotAsync(
        Guid userId,
        DateOnly visibleMonth,
        Domain.Enums.TimeSelectionMode selectionMode,
        DateOnly? selectedDate = null,
        CancellationToken cancellationToken = default);

    Task<Domain.ValueObjects.CalendarPeriod> ResolveSelectionAsync(
        Guid userId,
        DateOnly selectedDate,
        Domain.Enums.TimeSelectionMode selectionMode,
        CancellationToken cancellationToken = default);

    Task<Domain.ValueObjects.CalendarPeriod> GetPreviousPeriodAsync(
        Guid userId,
        Domain.ValueObjects.CalendarPeriod period,
        CancellationToken cancellationToken = default);

    Task<Domain.ValueObjects.CalendarPeriod> GetNextPeriodAsync(
        Guid userId,
        Domain.ValueObjects.CalendarPeriod period,
        CancellationToken cancellationToken = default);
}
