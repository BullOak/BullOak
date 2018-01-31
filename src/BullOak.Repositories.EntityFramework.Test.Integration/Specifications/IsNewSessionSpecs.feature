Feature: InNewSessionSpecs
	In order to guard against illegal transitions for pre-existing entities (for example to raise an error when trying to do any action on a non-initialized entity)
	As a developer using this library
	I want the current session to have a flag informing me if this is a new entity or pre-existing

Scenario: When I start a session for a new entity, the session should hold the fact that the entity is new (not stored)
	Given no existing entity
	When I load the session
	Then the IsNewState should be true

Scenario: When I start a session for an existing entity, the session should hold the fact that the entity already existed in db
	Given an existing entity with HigherOrder 2
	When I load the session
	Then the IsNewState should be false
