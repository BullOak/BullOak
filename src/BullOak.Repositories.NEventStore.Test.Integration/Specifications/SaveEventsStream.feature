Feature: SaveEventsStream
	In order to persist using an event stream
	As a developer usint this new library
	I want to be able to save events in a stream

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

Scenario: Save one event in an existing stream
	Given an existing stream with 1 events
	And 1 new event starting with Order of 2
	When I try to save the new events in the stream
	Then the save process should succeed
	And there should be 2 events in the stream
	And there should be 1 event in the stream with Order higher or equal to 2
