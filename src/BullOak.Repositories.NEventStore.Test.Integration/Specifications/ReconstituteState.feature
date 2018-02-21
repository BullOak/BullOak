Feature: ReconstituteState
	In order to apply business logic on stored entities
	As a developer using this library
	I want to be able to get correctly reconstituted states from my event stream

Scenario: Load stored entity with single event
	Given an existing stream with 2 events
	When I load my entity
	Then HighOrder property should be 1

Scenario: Reconstitute state from one event stored using interface
	Given a new stream
	And 3 new events
	And I try to save the new events in the stream through their interface
	When I load my entity
	Then the load process should succeed
	And HighOrder property should be 2
