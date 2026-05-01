## TOUSE
C# /
SQLite /
ORM /
Swagger documentation (Is just documentation formatted in JSON / YAML)

## TODO
Essentially need a link to GET/POST when want stuff:
1. Filter by Free-Parking (free_parking != 'NO')
2. Filter by Night-Parking (night_parking == 'YES')
3. Filter by gantry_height > vehicle height (input)

ERD
1. Change CARPARK -> FREE_SESSION to 0 : m (from having free parking to none)
2. Change all the connector IDs to use the car_park_no instead

Batch:
1. Have a local csv with stuff in it
2. Daily, update database based on the csv (#actually, this may not be required (like the python job I wrote for the crontab))
3. IFERR, dont update the db

## Optionals
1. Large size so prob reading whole file is unideal, need to find way to identify changes
2. Minimal human job recovery, the thing should work by smtg ez like restarting
3. Security, lets not send stuff unencrypted
4. API auth, like SQLCon with API key / user & pwd


# Carpark-Info
A take-home coding assignment for backend developer interview. 

## Your Task
1. Given the CSV dataset (hdb-carpark-information-<timestamp>.csv) that contains details of a list of carparks, design the database to store the given information in the dataset and to support the below given user stories. ER diagram should be provided.
2. Write a batch job that will process and store the information into the database of your choice. This is a daily delta file that will be interfaced over from source. In the event there is an error processing the records in the file, the entire file should rollback.
3. Write the APIs that will fulfill the below given user stories. Swagger documentation should be provided. No front-end screens are required to be developed - just the APIs. However, you should be prepared to articulate how the APIs are envisoned to be utilised by the front-end developer. :)

### User Stories
* As a user, I want to be able to filter the list of carpark by the following criteria:
  - Carpark that offer free parking
  - Carpark that offer night parking
  - Carpark that can meet my vehicle height requirement.
* As a user, I want to be able to add a specific carpark as my favourite.

## Getting Started
Please review the information in this section before you get started with your development. 

* Create a personal fork of the project on Github.
* Clone the fork on your local machine.
* Implement your solution and the rest of git basics applies.
* When you are ready, submit the forked repo for review by providing the link to the repo to our recruitment team.

### Tech Stack
You may choose to develop the application using either of the following stack:
* Spring Boot / Spring Batch with H2 database and ORM of your choice
* .NET Core 6.x with SQLite database and ORM of your choice
* Node.js with an in-memory database of your choice

Note: You are encouraged to try out .NET Core as Microsoft technologies are primarily used within the firm.

### Tools
You are free to choose the IDE (Integrated Development Environment) tool you are most comfortable with.

## Basic Expectation
* Ability to design data schema, apply normalisation technique and enhance query performances, if applicable.
* Write readable, maintainable, performant and well-documented codes.
* Code design / architecture should support implementation of unit testing.
* Code design / architecture should be flexible to changes / open to extensions, e.g. changing of data access technology, changing of interface file format from csv to JSON etc.
* Write clear and concise commit message.

## Challenge Yourself
Additional consideration to fine-tune your solution. It's not a must to implement in this assignment but please be prepared to discuss:
* The dataset has the potential to be large in size.
* Minimal human intervention for job recovery.
* Secure coding practices
* API authentication and authorisation

## Time Estimates
This assignment should take about 2 to 4 hours of your time depending on your level of experiences. 

## Need Help
Create a github issue. We'll get back to you.
