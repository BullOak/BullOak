Feature: RepositoryBasedES
	As a software engineer
	I want to be able to use repository-based event sourcing for storage of my entities
	In order to better isolate domain logic and decouple storage from business logic

# The background context of these tests is a software used by cinema companies to manage movie showing and reservations
#   The cinema can have exactly one movie showing every day
#   In a showing the cinema can sell at most tickets equal to the seats it has
#   The seats get reserved, can get cancelled (which means get unreserved) and no action can be performed after the viewing has happened.

Scenario: Creation events get stored when saved
	Given I creare a cinema named "MyCinema" with 2 seats
	When I save the cinema
	Then a CinemaCreatedEvent should exist
	And the cinema creation event should have seats set to 2
	And the cinema creation event should have a cinema name of "MyCinema"

Scenario: Reconstituting an aggregate populates it with data correctly
	Given the "MyCinema" cinema with 2 seats
	When I load the "MyCinema" cinema from the repository
	Then the cinema I get should not be null
	And the cinema aggregate should have seats set to 2
	And the cinema aggregate should have a cinema name of "MyCinema"

