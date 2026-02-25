Feature: Health Check
  As a DevOps engineer
  I want to verify the application is healthy
  So that I can monitor its status

  Scenario: Application health check returns healthy
    Given the application is running
    When I send a GET request to "/health"
    Then the response status code should be 200
    And the response should contain "Healthy"
