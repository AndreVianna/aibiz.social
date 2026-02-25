using AiBiz.Domain.Entities;
using AiBiz.Infrastructure.Persistence;
using AiBiz.Infrastructure.Services;
using AiBiz.IntegrationTests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace AiBiz.IntegrationTests.StepDefinitions;

[Binding]
public sealed class FreeTierSteps(
    AiBizDbContext db,
    IAgentService agentService,
    FreeTierTestContext testCtx)
{
    private Sponsor? _currentSponsor;
    private AgentProfile? _createdAgent;
    private Exception? _caughtException;

    // ── Background ──────────────────────────────────────────────────────────

    [Given("the database is available")]
    public async Task GivenTheDatabaseIsAvailable()
    {
        // Verifies we can connect — throws if DB is down
        await db.Database.OpenConnectionAsync();
    }

    // ── Given ────────────────────────────────────────────────────────────────

    [Given("a free sponsor with 1 agent already created")]
    public async Task GivenAFreeSponsorWith1AgentAlreadyCreated()
    {
        // Create sponsor
        _currentSponsor = new Sponsor
        {
            Email = $"freetier-{Guid.NewGuid():N}@test.local",
            DisplayName = "Free Sponsor (1 agent)"
        };
        db.Sponsors.Add(_currentSponsor);
        await db.SaveChangesAsync();
        testCtx.TestSponsorId = _currentSponsor.Id;  // register for cleanup

        // Insert the one allowed agent directly via DB (bypasses service — this is setup data)
        var existingAgent = new AgentProfile
        {
            SponsorId = _currentSponsor.Id,
            Name = "Existing Agent"
        };
        db.AgentProfiles.Add(existingAgent);
        await db.SaveChangesAsync();
    }

    [Given("a new free sponsor with no agents")]
    public async Task GivenANewFreeSponsorWithNoAgents()
    {
        _currentSponsor = new Sponsor
        {
            Email = $"new-sponsor-{Guid.NewGuid():N}@test.local",
            DisplayName = "New Sponsor (no agents)"
        };
        db.Sponsors.Add(_currentSponsor);
        await db.SaveChangesAsync();
        testCtx.TestSponsorId = _currentSponsor.Id;  // register for cleanup
    }

    // ── When ─────────────────────────────────────────────────────────────────

    [When("they try to create another agent")]
    public async Task WhenTheyTryToCreateAnotherAgent()
    {
        _caughtException = null;
        _createdAgent = null;
        try
        {
            _createdAgent = await agentService.CreateAgentAsync(
                _currentSponsor!.Id,
                "Second Agent — should be rejected");
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"they create an agent named ""(.*)""")]
    public async Task WhenTheyCreateAnAgentNamed(string name)
    {
        _caughtException = null;
        _createdAgent = null;
        try
        {
            _createdAgent = await agentService.CreateAgentAsync(
                _currentSponsor!.Id,
                name);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    // ── Then ─────────────────────────────────────────────────────────────────

    [Then("the request is rejected with a free-tier limit error")]
    public void ThenTheRequestIsRejectedWithAFreeTierLimitError()
    {
        _caughtException.Should().NotBeNull(
            "a FreeTierLimitExceededException should have been thrown");
        _caughtException.Should().BeOfType<FreeTierLimitExceededException>();
        ((FreeTierLimitExceededException)_caughtException!).SponsorId
            .Should().Be(_currentSponsor!.Id);
        _createdAgent.Should().BeNull("no agent should have been created");
    }

    [Then("the agent is created successfully")]
    public void ThenTheAgentIsCreatedSuccessfully()
    {
        _caughtException.Should().BeNull(because: _caughtException?.Message);
        _createdAgent.Should().NotBeNull();
        _createdAgent!.SponsorId.Should().Be(_currentSponsor!.Id);
        _createdAgent.Id.Should().NotBe(Guid.Empty);
    }
}
