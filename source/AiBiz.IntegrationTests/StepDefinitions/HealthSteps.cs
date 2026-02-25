using System.Net;
using FluentAssertions;
using Reqnroll;

namespace AiBiz.IntegrationTests.StepDefinitions;

[Binding]
public sealed class HealthSteps(HttpClient client)
{
    private HttpResponseMessage? _response;

    [Given("the application is running")]
    public void GivenTheApplicationIsRunning()
    {
        // WebApplicationFactory is already started via Hooks.BeforeScenario.
        // The injected HttpClient is wired to the in-process test server.
        client.Should().NotBeNull();
    }

    [When(@"I send a GET request to ""(.*)""")]
    public async Task WhenISendAGetRequestTo(string path)
    {
        _response = await client.GetAsync(path);
    }

    [Then(@"the response status code should be (\d+)")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        _response.Should().NotBeNull("a request must have been made first");
        ((int)_response!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"the response should contain ""(.*)""")]
    public async Task ThenTheResponseShouldContain(string expectedContent)
    {
        _response.Should().NotBeNull("a request must have been made first");
        var body = await _response!.Content.ReadAsStringAsync();
        body.Should().Contain(expectedContent);
    }
}
