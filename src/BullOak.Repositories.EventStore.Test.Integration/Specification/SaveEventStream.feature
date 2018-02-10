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

Scenario: Concurrent write should fail for outdated session
	Given an existing stream with 10 events
	And session 'Session1' is open
	And session 'Session2' is open
	And 10 new events are added by 'Session1'
	And 10 new events are added by 'Session2'	
	When I try to save 'Session1'
	And I try to save 'Session2'
	Then the save process should succeed for 'Session1'
	And the save process should fail for 'Session2' with ConcurrencyException
	And there should be 20 events in the stream

Scenario: Saving already saved session should throw meaningful usage advice exception
	Given an existing stream with 10 events
	And session 'Session1' is open
	And 10 new events are added by 'Session1'
	When I try to save 'Session1'
	And I try to save 'Session1'
	Then the save process should fail for 'Session1'
	And there should be 20 events in the stream
