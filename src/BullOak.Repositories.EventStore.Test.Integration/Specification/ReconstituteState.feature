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
	| 1           | 0             |
	| 5           | 4             |
	| 10000       | 9999          |