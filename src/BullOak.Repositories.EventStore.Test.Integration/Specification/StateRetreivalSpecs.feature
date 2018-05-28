Feature: StateRetrievalSpecs
	In order to implement complex logic without further state storage
	As a developer using this library
	I want the current state to be updated immediately when I add new events even if I don't save the session

# The behaviour defined here is actually desirable. If we do not save events, obviously we do not want them
#  to exist when we again reconstitute the state, but we do want the added events to instantly update the state
#  so that we can branch on conditions of current state without polluting the stateless, functional domain
#  class that implements the domain logic

Scenario Outline: When I add new events in the stream I want the state to be updated immediately
	Given an existing stream with <eventCount> events
	When I add <addedEvents> events in the session without saving it
	Then HighOrder property should be <highOrder>
Examples: 
	| eventCount | addedEvents | highOrder |
	| 0          | 3           | 2         |
	| 2          | 3           | 2         |
	| 7          | 3           | 6         |
	| 0          | 10000       | 9999      |
