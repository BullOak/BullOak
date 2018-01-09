Feature: UpdateAndSaveEntity
	In order to persist using an event stream
	As a developer usint this new library
	I want to be able to save events in a stream

Scenario: Save one event in a new entity
	Given no existing entity
	And 1 new events
	When I try to save a new entity with the new events in the stream
	Then the save process should succeed

Scenario: Save multiple events in a new entity
	Given no existing entity
	And 3 new events
	When I try to save a new entity with the new events in the stream
	Then the save process should succeed

Scenario: Save one event in an existing entity
	Given an existing entity with HigherOrder 2
	And 1 new event
	When I try to update an existing entity with the new events in the stream
	Then the save process should succeed
