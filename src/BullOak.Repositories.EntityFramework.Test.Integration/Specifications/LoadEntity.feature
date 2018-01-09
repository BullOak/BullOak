Feature: LoadEntity
	In order to apply business logic on stored entities
	As a developer using this library
	I want to be able to get correctly load state from my store

Scenario: Load stored entity
	Given an existing entity with HigherOrder 2
	When I load my entity
	Then HighOrder property should be 2
