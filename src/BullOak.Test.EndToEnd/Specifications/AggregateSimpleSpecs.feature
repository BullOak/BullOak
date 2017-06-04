Feature: AggregateSimpleSpecs
	As a software engineer
	I want to be able to use event sourcing for storage of my entities
	In order to better isolate domain logic and decouple storage from business logic

# The background context of these tests is a software used by cinema companies to manage movie showing and reservations
#   The cinema can have exactly one movie showing every day
#   In a showing the cinema can sell at most tickets equal to the seats it has
#   The seats get reserved, can get cancelled (which means get unreserved) and no action can be performed after the viewing has happened.

Scenario: Storing creation event of aggregate root
	Given a cinema with 2 seats
	When I create the viewing on 4th of June 2017
	Then a ViewingCreatedEvent should be in the stream

Scenario: Storing creation event of sub-entities
	Given a cinema with 2 seats
	When I create the viewing on 4th of June 2017
	Then 2 SeatInViewingCreated events should be fired

Scenario: Aggregate can get reconstituted from events
	Given a cinema with 2 seats
	When I reserve seat one on viewing on the 4th of June 2017
	Then AggregateNotFoundError event is not raised
