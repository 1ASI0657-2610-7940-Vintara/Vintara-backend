namespace WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Value object representing a report period
/// </summary>
/// <param name="StartDate">Start date of the period</param>
/// <param name="EndDate">End date of the period</param>
public record ReportPeriod(DateTime StartDate, DateTime EndDate)
{
    public ReportPeriod() : this(DateTime.MinValue, DateTime.MinValue)
    {
    }
    /// <summary>
    /// Gets the duration of the period in days.
    /// </summary>
    public int DaysDuration => (EndDate - StartDate).Days;

    /// <summary>
    /// Validates the report period constraints
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the start date is after the end date, or the end date is in the future.
    /// </exception>
    public void Validate()
    {
        if (StartDate > EndDate)
            throw new ArgumentException("Start date cannot be after end date");
        
        if (EndDate > DateTime.UtcNow)
            throw new ArgumentException("End date cannot be in the future");
    }
}