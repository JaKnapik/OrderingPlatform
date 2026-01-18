namespace Ordering.API.Infrastructure.Data;

public class OutboxMessage
{
	public Guid Id { get; set; }
	public string Type { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public DateTime OccurredOnUtc { get; set; }
	public DateTime? ProcessedOnUtc { get; set; }
	public string? Error { get; set; }

	// --- Sekcja Traceability ---
	public Guid? CorrelationId { get; set; }

	public Guid? CausationId { get; set; }

	public string? TraceId { get; set; }
}
