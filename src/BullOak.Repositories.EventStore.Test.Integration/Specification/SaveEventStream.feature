Feature: SaveEventsStream
	In order to persist using an event stream
	As a developer usint this new library
	I want to be able to save events in a stream

Scenario: Save one event in a new stream
	Given a new stream
	And 1 new events
	When I try to save the new events in the stream
	Then the save process should succeed
	 And there should be 1 events in the stream


Scenario: Save multiple events in a new stream
	Given a new stream
	And 30 new events
	When I try to save the new events in the stream
	Then the save process should succeed
	And there should be 30 events in the stream

Scenario: Save additional events in an existing stream
	Given an existing stream with 10 events
	And 10 new events
	When I try to save the new events in the stream
	Then the save process should succeed
	And there should be 20 events in the stream
