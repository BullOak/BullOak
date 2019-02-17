Feature: ReconstituteState
	In order to apply business logic on stored entities
	As a developer using this library
	I want to be able to get correctly reconstituted states from my event stream

Scenario Outline: Load stored entity with from existing events
	Given a new stream
	And <eventsCount> new events
	When I try to save the new events in the stream
	And I load my entity
	Then HighOrder property should be <expectedState>
Examples:
	| eventsCount | expectedState |
	| 2           | 1             |
	| 5           | 4             |
	| 10000       | 9999          |

Scenario: Reconstitute state from one event stored using interface
	Given a new stream
	And 3 new events
	And I try to save the new events in the stream through their interface
	When I load my entity
	Then the load process should succeed
	And HighOrder property should be 2

Scenario: Reconstitute state from empty stream should succeed and return default state
	Given a new stream
	When I load my entity
	Then the load process should succeed
	And HighOrder property should be 0

Scenario: Reconstitute state after a soft delete should succeed and return default state
	Given a new stream
	And 3 new events
	And  I soft-delete the stream
	When I load my entity
	Then the load process should succeed
	And HighOrder property should be 0

Scenario: Reconstitute state after a hard delete should succeed and return default state
	Given a new stream
	And 3 new events
	And  I hard-delete the stream
	When I load my entity
	Then the load process should succeed
	And HighOrder property should be 0
