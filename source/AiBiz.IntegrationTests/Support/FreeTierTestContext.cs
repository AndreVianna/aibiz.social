namespace AiBiz.IntegrationTests.Support;

/// <summary>
/// Shared mutable state for a single free-tier test scenario.
/// Registered in BoDi container so both hooks and step definitions can access it.
/// </summary>
public sealed class FreeTierTestContext
{
    /// <summary>The sponsor created for this test. Used by AfterScenario for cleanup.</summary>
    public Guid TestSponsorId { get; set; }
}
