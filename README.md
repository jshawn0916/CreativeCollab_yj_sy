# CreativeCollab_yj_sy

We have designed a system that allows users to create and manage their own Journey lists. Through this system, users can register a Journey and add individual restaurants and destinations within it. Additionally, users with administrator privileges can add and manage restaurants and Destinations by editing and deleting them, as well as manage the Journeys of all users. This enables users to easily manage their own Journey lists and quickly find their desired restaurants and Destinations. Additionally, when registering restaurants and destinations, administrator can select a precise address through Google Maps for accurate registration.
## Features

### Journey Management
- Create Journey
- List Journey
- Update Journey
- Delete Journey
- Connect multiple restaurants to a Journey at once
- Disconnect multiple restaurants from a Journey at once
- Connect multiple destinations to a Journey at once
- Disconnect multiple destinations from a Journey at once

### Restaurants Management
- Create Restaurant
- List Restaurant
- Update Restaurant
- Delete Restaurant

### Destinations Management
- Create Destination
- List Destination
- Update Destination
- Delete Destination

---
<br/>

## Database

### Tables

#### AspUsers
- id
- Email
- passwordHash
- UserName

#### Journeys
- JourneyID
- JourneyName
- JourneyDescription
- UserID
  
#### Restaurants
- RestaurantID
- RestaurantName
- Location
- Rate
- RestaurantDescription

#### Destinations
- DestinationID
- DestinationName
- Location
- DestinationDescription

#### Bridge Table: RestaurantJourneys
- Establishes a many-to-many relationship between Restaurants and Journeys

#### Bridge Table: JourneyDestinations
- Establishes a many-to-many relationship between Destinations and Journeys
  
### Table Relationships
- Users to Journeys: 1 to Many
- Restaurants to Journeys: Many to Many
- Destinations to Journeys: Many to Many

### OPEN API
- Google Map API
