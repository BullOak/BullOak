Feature: InNewSessionSpecs
	In order to guard against illegal transitions for pre-existing entities (for example to raise an error when trying to do any action on a non-initialized entity)
	As a developer using this library
	I want the current session to have a flag informing me if this is a new entity or pre-existing

Scenario Outline: When I load a stream I want to know if it represents a new or existing entity
	Given a stream with <eventCount> events
	When I load the session
	Then the IsNewState should be <isNewState>
Examples:
	| eventCount | isNewState |
	| 0          | true       |
	| 2          | false      |
