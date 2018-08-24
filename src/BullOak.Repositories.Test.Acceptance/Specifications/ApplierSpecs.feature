Feature: ApplierSpecs
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Same applier processing multiple events of different type
	Given an existing stream with 2 events
	And a buyer name set event which can be upconverted as below in the stream
	| Title | Name  | Surname |
	| Mr.   | Funny | Surname |
	And a buyer name set event which can be upconverted as below in the stream
	| Title | Name  | Surname |
	| Mr.   | Funny2 | Surname2 |
	When I load my entity
	Then HighOrder property should be 1
	And FullName property of the entity should be "Mr. Funny2 Surname2"
