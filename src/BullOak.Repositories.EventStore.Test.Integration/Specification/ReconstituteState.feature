Feature: ReconstituteState
	In order to apply business logic on stored entities
	As a developer using this library
	I want to be able to get correctly reconstituted states from my event stream

Scenario: Load stored entity with from existing events
	Given a new stream
	And 20 new events
	When I try to save the new events in the stream
	And I load my entity
	Then HighOrder property should be 19
