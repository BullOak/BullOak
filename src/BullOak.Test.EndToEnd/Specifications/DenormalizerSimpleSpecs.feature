Feature: DenormalizerSimpleSpecs
	As a software engineer
	I want a read store for to handle queries
	In order to segregate representation responsibility

# The background context of these tests is a software used by cinema companies to manage movie showing and reservations
#   The cinema can have exactly one movie showing every day
#   In a showing the cinema can sell at most tickets equal to the seats it has
#   The seats get reserved, can get cancelled (which means get unreserved) and no action can be performed after the viewing has happened.

Scenario: Creating an entity gets stored in the denormalizer
	Given a cinema with 2 seats
	When I create the viewing on 4th of June 2017
	Then a view is created on the 4th of June 2017
	And A seat view for this viewing for seat number 1
	And a seat view for this viewing for seat number 2

Scenario: Updating an entity updates the denormalizer
	Given a cinema with 2 seats
	When I create the viewing on 4th of June 2017
	And seat 2 of the viewing gets reserved
	Then seat view with seat number 2 gets updated to show its reserved


