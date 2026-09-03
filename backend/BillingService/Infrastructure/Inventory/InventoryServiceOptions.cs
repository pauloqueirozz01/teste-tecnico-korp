namespace KorpTeste.BillingService.Infrastructure.Inventory;

public sealed class InventoryServiceOptions
{
    public const string SectionName = "InventoryService";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
    public InventoryResilienceOptions Resilience { get; set; } = new();
}

public sealed class InventoryResilienceOptions
{
    public int RetryCount { get; set; } = 2;
    public int RetryBaseDelayMilliseconds { get; set; } = 200;
    public double CircuitBreakerFailureRatio { get; set; } = 0.5;
    public int CircuitBreakerMinimumThroughput { get; set; } = 4;
    public int CircuitBreakerSamplingSeconds { get; set; } = 10;
    public int CircuitBreakerBreakSeconds { get; set; } = 15;
}
