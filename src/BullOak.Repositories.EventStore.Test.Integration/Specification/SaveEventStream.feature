Feature: SaveEventsStream
	In order to persist using an event stream
	As a developer usint this new library
	I want to be able to save events in a stream

Scenario Outline: Save events in a new stream
	Given a new stream
	And <eventsCount> new events
	When I try to save the new events in the stream
	Then the save process should succeed
	 And there should be <eventsCount> events in the stream
Examples: 
	| eventsCount |
	| 1           |
	| 30          |
	| 10000       |

Scenario: Save additional events in an existing stream
	Given an existing stream with 10 events
	And 10 new events
	When I try to save the new events in the stream
	Then the save process should succeed
	And there should be 20 events in the stream
