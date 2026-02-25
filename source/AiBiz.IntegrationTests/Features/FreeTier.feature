Feature: Free Tier Agent Limit
  As a platform operator
  I want to enforce a 1-agent limit for free-tier sponsors
  So that monetisation tiers have meaning and infrastructure costs are controlled

  Background:
    Given the database is available

  @freetier
  Scenario: Free sponsor cannot exceed the 1-agent limit
    Given a free sponsor with 1 agent already created
    When they try to create another agent
    Then the request is rejected with a free-tier limit error

  @freetier
  Scenario: Free sponsor can create their first agent
    Given a new free sponsor with no agents
    When they create an agent named "First Bot"
    Then the agent is created successfully
