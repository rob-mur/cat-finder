Feature: Picture retrieval from sources

Scenario: Pictures are retrieved
	When the Sources are asked for their latest 100 pictures
	Then the latest 100 pictures are returned