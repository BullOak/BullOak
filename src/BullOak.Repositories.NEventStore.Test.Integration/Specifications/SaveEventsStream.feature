Feature: SaveEventsStream
	In order to persist using an event stream
	As a developer usint this new library
	I want to be able to save events in a stream

Scenario: Save one event using interface
	Given a new stream
	And 1 new event
	When I try to save the new events in the stream through their interface
	Then the save process should succeed

Scenario: Save one event in a new stream
	Given a new stream
	And 1 new event
	When I try to save the new events in the stream
	Then the save process should succeed
	And there should be 1 event in the stream
	And there should be 1 event in the stream with Order higher or equal to 0

Scenario: Save multiple events in a new stream
	Given a new stream
	And 3 new events
	When I try to save the new events in the stream
	Then the save process should succeed
	And there should be 3 events in the stream

Scenario: Throw concurrency exception if two stream try to save at the same time
	Given an existing stream with 1 events
	And I start session 1 and I add 1 event
	And I start session 2 and I add 1 event
	When I try to save all sessions
	Then 1 save session should fail
	And 1 save session should succeed
	And all failed sessions should have failed with ConcurrencyException
	And there should be 2 events in the stream

Scenario: Save one event in an existing stream
	Given an existing stream with 1 events
	And 1 new event starting with Order of 2
	When I try to save the new events in the stream
	Then the save process should succeed
	And there should be 2 events in the stream
	And there should be 1 event in the stream with Order higher or equal to 2
