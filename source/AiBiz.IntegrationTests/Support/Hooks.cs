using AiBiz.IntegrationTests.Support;
using Reqnroll;
using Reqnroll.BoDi;

namespace AiBiz.IntegrationTests.Support;

/// <summary>
/// Reqnroll lifecycle hooks — creates and tears down the WebApplicationFactory
/// once per scenario via Reqnroll's built-in DI container.
/// </summary>
[Binding]
public sealed class Hooks(IObjectContainer objectContainer)
{
    private WebAppFixture? _factory;

    [BeforeScenario]
    public void BeforeScenario()
    {
        _factory = new WebAppFixture();
        var client = _factory.CreateClient();

        // Register in Reqnroll's DI so step definitions can inject them
        objectContainer.RegisterInstanceAs(_factory);
        objectContainer.RegisterInstanceAs(client);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _factory?.Dispose();
    }
}
