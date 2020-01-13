Feature: InvariantValidation
	In order to avoid breaking invariants
	As a developer
	I want to be able to set an invariant validator on my repos which gets excercised on all actions

Scenario: Validator gets called
	Given a new stream
    And a validator which enforces that a state should never be above <MaxState>
	And <Count> new event
	When I try to save the new events in the stream
	Then the save process should <Outcome>
    Examples:
    | MaxState | Count | Outcome |
    | 5        | 1     | succeed |
    | 2        | 5     | fail    |
