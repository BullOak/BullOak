Feature: ReconstituteState
	In order to apply business logic on stored entities
	As a developer using this library
	I want to be able to get correctly reconstituted states from my event stream

@WithNoUpconverters
Scenario: Load stored entity with single event
	Given an existing stream with 2 events
	When I load my entity
	Then HighOrder property should be 1

# There is an upconverter from one event to another event which composes properties to a FullName
@WithBuyerNameUpconverter
Scenario: An event gets upconverted and loaded without upconverter of original event
	Given a buyer name set event which can be upconverted as below in the stream
	| Title | Name  | Surname |
	| Mr.   | Funny | Surname |
	When I load my entity
	Then FullName property of the entity should be "Mr. Funny Surname"

# There is an upconverter from set balance event to split to two events
@WithBalanceUpdateUpconverter
Scenario: An event gets split upconverted and loaded without upconverter of original event
	Given a balance set event with balance 50.00 and date 2017-12-30
	When I load my entity
	Then the loaded entity should have a balance of 50.00 and last update date 2017-12-30
