# EventParking

## Introduction
This repository contains the code and documentation for the IS7024 Final Project. The project will combine event API from [SeatGeek](https://seatgeek.com/) and parking data from [ParkWhiz](https://www.parkwhiz.com/) to show convenient parking locations near events.


## Icon / Logo
<img src="Assets/logo/logo.png" alt="A logo of a blue car on a bright red chair." width="200"/>

## Storyboard / Wireframe  

<img src="Assets/documentation/wireframes/wireframes.png" alt="An image of wireframes for Home, Event Search, Event Page, Purchase Modal, and Purchase Modal Result" width="800"/>  
 
(click for full image size)  

## GitHub Project

- [GitHub Repository](https://github.com/poeppenz/IS7024FinalProject)
- [GitHub Project Board](https://github.com/users/poeppenz/projects/1)


## Requirements

- As an event goer, I want to find my event so that I can then search for parking.
  - Given that the site has information for events in my event location, when I search [Partial Event Name], then I should get at least one result of [Event Name].
  - Given that the site has information for events in my event location, when I search [Event Location and Date], then I should get at least one result of [Event Name].
  - Given that the site has information for events in my event location, when I search [Invalid Event Name], then I should get no results.
- As an event goer, I want to see parking options near my event so that I can get from parking to the event quickly and easily.
  - Given that the site has information for events and parking in my event location, when I search [Valid Event Name], then I will get at least one result for [Parking Option] with a listed distance of [Distance].
  - Given that the site has information for events and parking in my event location, when I search [Invalid Event Name], then I will get no results.
- As an event goer, I want to see parking options with prices so that I can compare prices and find the best deal.
  - Given that the site has information for events and parking in my event location, when I search [Valid Event Name], then I will get at least one result for [Parking Option] with a listed price of [Price].
  -  Given that the site has information for events and parking in my event location, when I search [Invalid Event Name], then I will get no results.
- As an event goer, I want to see the times that parking options are available so that I can ensure that I will be able to park there for the entire duration of the event.
  - Given that the site has information for events and parking in my event location, when I search [Valid Event Name], then I will get at least one result for [Parking Option] with a listed availability of [Availability Time Range].
  - Given that the site has information for events and parking in my event location, when I search [Invalid Event Name], then I will get no results.
- As an event goer, I want the abililty to click on a link to purchase parking so that I can make the purchase without having to find the the parking provider's website.
  - Given I have chosen a parking option that the site has information on, when I select [Parking Option With Online Purchasing], then I should be redirected to the parking provider's website to complete my purchase.
  - Given I have chosen a parking option that the site has information on, when I select [Parking Option Without Online Purchasing], then I be told that online purchasing is not available for this selection. 
    

## Tasks

- [x] Research and select APIs for event and parking data.
- [X] Implement API integration for event data. SeatGeek
- [X] Implement API integration for parking data. ParkWhiz
- [ ] Finalize a project name. EventParking
- [ ] Develop frontend components to display events and parking options.
- [ ] Test the application for usability and performance.
- [ ] Deploy the application to Azure.
## Examples: 1.
Event and parking search data are available and accessible.
### Assumptions
Events names and locations are stated clearly 
Closest parking, spot spaces well stated.

**Provide**: Location

**Seat**: Available

**When** I search for “SeatGreek”
Then I should receive at least one result with these attributes:

Location: Paycor Stadium Cincinnati

Availability: Section 114, Row 1

Parking Spot: CRG west garage  spot A34 | Lot B - C14 | Lot A - B27
 


## Data Sources

- [SeatGeek API Documentation](https://seatgeek.com/build) - Approved
- [ParkWhiz API Documentation](https://developer.parkwhiz.com/) - Approved
- [Spot Hero API Documentation](https://spothero.com/developers) - Rejected

## Roles
Assignments still TBD  
-Project Manager  
-Tester/QA  
-Azure tech  
-Usability  
-UX/UI design  

## Members

- Nathan Poeppelman - API Manager - poeppenz@ucmail.uc.edu
- Michael Seitz - QA - seitzme@mail.uc.edu
- Zachary Durst - Azure/Operations - durstzd@mail.uc.edu
- Cassandra Horton - UX/UI Designer - hortonco@mail.uc.edu
- Monica Mwangi - mwangima@mail.uc.edu

## Weekly Meetings
- Meetings will be held every Tuesday at 9 PM via Teams.