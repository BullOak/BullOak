Feature: EventsCanBeIntercepted
	In order to implement cross cutting concerns
	As a developer
	I want to be able to intercept events going to the store or published

Scenario: Interceptor is called
	Given a new stream
	And 1 new event
	When I try to save the new events in the stream
	Then the interceptor should be called
