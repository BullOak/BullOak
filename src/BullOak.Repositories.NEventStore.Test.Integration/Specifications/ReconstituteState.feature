Feature: ReconstituteState
	In order to apply business logic on stored entities
	As a developer using this library
	I want to be able to get correctly reconstituted states from my event stream

Scenario: Load stored entity with single event
	Given an existing stream with 2 events
	When I load my entity
	Then HighOrder property should be 1
