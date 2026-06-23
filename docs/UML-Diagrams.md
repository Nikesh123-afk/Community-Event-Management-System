# Community Event Management System — UML Diagrams

## 1. Class Diagram — Full System Architecture

```mermaid
classDiagram
    %% ─── ABSTRACT BASE ───────────────────────────────────────────
    class BaseEntity {
        <<abstract>>
        -DateTime _createdAt
        -DateTime _updatedAt
        +int Id
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +bool IsDeleted
        +string DisplayName*
        +IEnumerable~string~ Validate()*
        +bool IsValid()
        +void SoftDelete()
        +string ToString()
    }

    %% ─── DOMAIN MODELS ───────────────────────────────────────────
    class Event {
        -DateTime _eventDate
        -int _maxCapacity
        +string Name
        +string Description
        +DateTime EventDate
        +TimeSpan StartTime
        +TimeSpan EndTime
        +EventStatus Status
        +EventType Type
        +int MaxCapacity
        +int? VenueId
        +int ConfirmedRegistrationCount
        +int AvailableSpots
        +bool IsFull
        +bool HasPassed
        +bool IsUpcoming
        +string DisplayName
        +IEnumerable~string~ Validate()
        +void Publish()
        +void Cancel()
        +void MarkCompleted()
    }

    class Venue {
        +string Name
        +string Address
        +int Capacity
        +string Description
        +string ContactPhone
        +string ContactEmail
        +string DisplayName
        +IEnumerable~string~ Validate()
        +bool CanAccommodate(int attendeeCount)
    }

    class Participant {
        -string _email
        +string FirstName
        +string LastName
        +string Email
        +string PhoneNumber
        +string Bio
        +string ApplicationUserId
        +string FullName
        +string DisplayName
        +IEnumerable~string~ Validate()
    }

    class Activity {
        +string Name
        +string Description
        +ActivityType Type
        +int DurationMinutes
        +string DisplayName
        +IEnumerable~string~ Validate()
    }

    class Registration {
        +int ParticipantId
        +int EventId
        +RegistrationStatus Status
        +DateTime RegistrationDate
        +string Notes
        +string DisplayName
        +IEnumerable~string~ Validate()
        +void Confirm()
        +void Cancel()
        +void WaitList()
    }

    class EventActivity {
        +int EventId
        +int ActivityId
        +int DisplayOrder
        +TimeSpan? ScheduledStartTime
    }

    %% ─── ENUMS ───────────────────────────────────────────────────
    class EventStatus {
        <<enumeration>>
        Draft
        Published
        Cancelled
        Completed
    }

    class EventType {
        <<enumeration>>
        InPerson
        Virtual
        Hybrid
    }

    class RegistrationStatus {
        <<enumeration>>
        Pending
        Confirmed
        Cancelled
        WaitListed
    }

    class ActivityType {
        <<enumeration>>
        Workshop
        Talk
        Game
        Performance
        Networking
        Exhibition
        Other
    }

    %% ─── INHERITANCE ─────────────────────────────────────────────
    BaseEntity <|-- Event
    BaseEntity <|-- Venue
    BaseEntity <|-- Participant
    BaseEntity <|-- Activity
    BaseEntity <|-- Registration

    %% ─── ASSOCIATIONS ────────────────────────────────────────────
    Event "many" --> "1" Venue : held at
    Event "1" --> "many" Registration : has
    Event "1" --> "many" EventActivity : contains
    Activity "1" --> "many" EventActivity : featured in
    Participant "1" --> "many" Registration : makes

    %% ─── EXCEPTIONS ──────────────────────────────────────────────
    class AppException {
        +string ErrorCode
        +string Message
    }
    class ValidationException {
        +string FieldName
        +string Reason
        +ErrorCode: VALIDATION_ERROR
    }
    class DomainValidationException {
        +IEnumerable~string~ Errors
    }
    class NotFoundException {
        +ErrorCode: NOT_FOUND
    }
    class EventNotFoundException
    class ParticipantNotFoundException
    class VenueNotFoundException
    class RegistrationNotFoundException
    class BusinessRuleException
    class EventCapacityExceededException
    class AlreadyRegisteredException
    class EventAlreadyPassedException
    class InvalidStatusTransitionException
    class DuplicateEntityException {
        +ErrorCode: DUPLICATE_ENTITY
    }

    AppException <|-- ValidationException
    AppException <|-- DomainValidationException
    AppException <|-- NotFoundException
    AppException <|-- BusinessRuleException
    AppException <|-- DuplicateEntityException
    NotFoundException <|-- EventNotFoundException
    NotFoundException <|-- ParticipantNotFoundException
    NotFoundException <|-- VenueNotFoundException
    NotFoundException <|-- RegistrationNotFoundException
    BusinessRuleException <|-- EventCapacityExceededException
    BusinessRuleException <|-- AlreadyRegisteredException
    BusinessRuleException <|-- EventAlreadyPassedException
    BusinessRuleException <|-- InvalidStatusTransitionException
```

---

## 2. Class Diagram — Repository Layer (Repository Pattern)

```mermaid
classDiagram
    class IRepository~T~ {
        <<interface>>
        +GetByIdAsync(int id) Task~T~
        +GetAllAsync() Task~IEnumerable~T~~
        +FindAsync(Expression predicate) Task~IEnumerable~T~~
        +AddAsync(T entity) Task~T~
        +UpdateAsync(T entity) Task
        +DeleteAsync(int id) Task
        +ExistsAsync(int id) Task~bool~
        +CountAsync(Expression predicate) Task~int~
    }

    class IEventRepository {
        <<interface>>
        +GetUpcomingEventsAsync() Task~IEnumerable~Event~~
        +GetEventsByVenueAsync(int venueId) Task~IEnumerable~Event~~
        +GetEventsByDateRangeAsync(DateTime from, DateTime to) Task~IEnumerable~Event~~
        +GetEventsByActivityTypeAsync(ActivityType type) Task~IEnumerable~Event~~
        +GetEventWithDetailsAsync(int id) Task~Event~
        +SearchEventsAsync(string query) Task~IEnumerable~Event~~
    }

    class BaseRepository~T~ {
        <<abstract>>
        #ApplicationDbContext _context
        #DbSet~T~ _dbSet
        +GetByIdAsync(int id) Task~T~
        +GetAllAsync() Task~IEnumerable~T~~
        +FindAsync(Expression predicate) Task~IEnumerable~T~~
        +AddAsync(T entity) Task~T~
        +UpdateAsync(T entity) Task
        +DeleteAsync(int id) Task
        +ExistsAsync(int id) Task~bool~
        +CountAsync(Expression predicate) Task~int~
    }

    class EventRepository {
        +GetUpcomingEventsAsync() Task~IEnumerable~Event~~
        +GetEventsByVenueAsync(int venueId) Task~IEnumerable~Event~~
        +GetEventWithDetailsAsync(int id) Task~Event~
        +SearchEventsAsync(string query) Task~IEnumerable~Event~~
    }

    class VenueRepository {
        +GetAllAsync() Task~IEnumerable~Venue~~
    }

    class ParticipantRepository {
        +GetByEmailAsync(string email) Task~Participant~
    }

    class RegistrationRepository {
        +GetByEventIdAsync(int eventId) Task~IEnumerable~Registration~~
        +GetByParticipantIdAsync(int pId) Task~IEnumerable~Registration~~
        +ExistsDuplicateAsync(int pId, int eId) Task~bool~
    }

    class ActivityRepository {
        +GetAllAsync() Task~IEnumerable~Activity~~
    }

    IRepository~T~ <|.. BaseRepository~T~
    IEventRepository <|-- IRepository~Event~
    BaseRepository~T~ <|-- EventRepository
    BaseRepository~T~ <|-- VenueRepository
    BaseRepository~T~ <|-- ParticipantRepository
    BaseRepository~T~ <|-- RegistrationRepository
    BaseRepository~T~ <|-- ActivityRepository
    IEventRepository <|.. EventRepository
```

---

## 3. Class Diagram — Service + Strategy + Factory + Observer Patterns

```mermaid
classDiagram
    %% ─── STRATEGY PATTERN ────────────────────────────────────────
    class IFilterStrategy {
        <<interface>>
        +Apply(IQueryable~Event~ query) IQueryable~Event~
    }

    class DateFilterStrategy {
        -DateTime _date
        +Apply(IQueryable~Event~ query) IQueryable~Event~
    }

    class VenueFilterStrategy {
        -int _venueId
        +Apply(IQueryable~Event~ query) IQueryable~Event~
    }

    class ActivityTypeFilterStrategy {
        -ActivityType _type
        +Apply(IQueryable~Event~ query) IQueryable~Event~
    }

    class KeywordFilterStrategy {
        -string _keyword
        +Apply(IQueryable~Event~ query) IQueryable~Event~
    }

    class CompositeFilterStrategy {
        -List~IFilterStrategy~ _strategies
        +Add(IFilterStrategy strategy) CompositeFilterStrategy
        +Apply(IQueryable~Event~ query) IQueryable~Event~
    }

    IFilterStrategy <|.. DateFilterStrategy
    IFilterStrategy <|.. VenueFilterStrategy
    IFilterStrategy <|.. ActivityTypeFilterStrategy
    IFilterStrategy <|.. KeywordFilterStrategy
    IFilterStrategy <|.. CompositeFilterStrategy
    CompositeFilterStrategy o-- IFilterStrategy : contains

    %% ─── FACTORY PATTERN ─────────────────────────────────────────
    class EventFactory {
        <<static>>
        +CreateInPersonEvent(name, desc, date, start, end, venueId, capacity) Event$
        +CreateVirtualEvent(name, desc, date, start, end, capacity) Event$
        +CreateHybridEvent(name, desc, date, start, end, venueId, capacity) Event$
        -Validate(name, description, capacity)$
    }

    %% ─── OBSERVER PATTERN ────────────────────────────────────────
    class INotificationService {
        <<interface>>
        +NotifyRegistrationConfirmedAsync(Registration reg) Task
        +NotifyEventCancelledAsync(Event evt, IEnumerable~Registration~ affected) Task
        +NotifyEventPublishedAsync(Event evt) Task
    }

    class NotificationService {
        -ILogger _logger
        +NotifyRegistrationConfirmedAsync(Registration reg) Task
        +NotifyEventCancelledAsync(Event evt, IEnumerable~Registration~ regs) Task
        +NotifyEventPublishedAsync(Event evt) Task
    }

    INotificationService <|.. NotificationService

    %% ─── SERVICE INTERFACES ──────────────────────────────────────
    class IEventService {
        <<interface>>
        +GetAllEventsAsync() Task~IEnumerable~Event~~
        +GetUpcomingEventsAsync() Task~IEnumerable~Event~~
        +FilterEventsAsync(date, venueId, type, query) Task~IEnumerable~Event~~
        +GetEventDetailsAsync(int id) Task~Event~
        +CreateEventAsync(Event evt, int[] activityIds) Task~Event~
        +UpdateEventAsync(Event evt, int[] activityIds) Task
        +PublishEventAsync(int id) Task
        +CancelEventAsync(int id) Task
        +DeleteEventAsync(int id) Task
    }

    class EventService {
        -IEventRepository _repo
        -ApplicationDbContext _context
        -INotificationService _notifications
        +FilterEventsAsync() Task~IEnumerable~Event~~
        +PublishEventAsync(int id) Task
        +CancelEventAsync(int id) Task
    }

    IEventService <|.. EventService
    EventService --> INotificationService : notifies
    EventService --> IFilterStrategy : uses
    EventService --> CompositeFilterStrategy : builds
```

---

## 4. Sequence Diagram — User Registers for an Event

```mermaid
sequenceDiagram
    actor User
    participant EventDetail as EventDetail.razor
    participant RegService as RegistrationService
    participant EventRepo as EventRepository
    participant RegRepo as RegistrationRepository
    participant Notification as NotificationService
    participant DB as SQL Server

    User->>EventDetail: Click "Register"
    EventDetail->>RegService: RegisterParticipantAsync(participantId, eventId, notes)
    RegService->>EventRepo: GetByIdAsync(eventId)
    EventRepo->>DB: SELECT Event WHERE Id=eventId
    DB-->>EventRepo: Event entity
    EventRepo-->>RegService: Event

    alt Event has passed
        RegService-->>EventDetail: throws EventAlreadyPassedException
        EventDetail-->>User: "This event has already passed"
    else Already registered
        RegService->>RegRepo: ExistsDuplicateAsync(participantId, eventId)
        RegRepo->>DB: SELECT Registration WHERE ParticipantId=x AND EventId=y AND Status != Cancelled
        DB-->>RegRepo: exists = true
        RegRepo-->>RegService: true
        RegService-->>EventDetail: throws AlreadyRegisteredException
        EventDetail-->>User: "You are already registered"
    else Event is full
        RegService->>RegRepo: AddAsync(registration with Status=WaitListed)
        RegRepo->>DB: INSERT Registration
        DB-->>RegRepo: Registration saved
        RegService-->>EventDetail: Registration (WaitListed)
        EventDetail-->>User: "Added to wait list"
    else Space available
        RegService->>RegRepo: AddAsync(registration with Status=Confirmed)
        RegRepo->>DB: INSERT Registration
        DB-->>RegRepo: Registration saved
        RegService->>Notification: NotifyRegistrationConfirmedAsync(registration)
        Notification-->>RegService: Logged
        RegService-->>EventDetail: Registration (Confirmed)
        EventDetail-->>User: "Registration confirmed!"
    end
```

---

## 5. Sequence Diagram — Admin Publishes an Event

```mermaid
sequenceDiagram
    actor Admin
    participant ManageEvents as ManageEvents.razor
    participant EventService as EventService
    participant EventRepo as EventRepository
    participant Notification as NotificationService
    participant DB as SQL Server

    Admin->>ManageEvents: Click "Publish" on Draft event
    ManageEvents->>EventService: PublishEventAsync(eventId)
    EventService->>EventRepo: GetByIdAsync(eventId)
    EventRepo->>DB: SELECT Event with Venue, Registrations
    DB-->>EventRepo: Event entity
    EventRepo-->>EventService: Event

    alt Event not found
        EventService-->>ManageEvents: throws EventNotFoundException
        ManageEvents-->>Admin: "Event not found"
    else Already published/cancelled/completed
        EventService-->>ManageEvents: throws InvalidStatusTransitionException
        ManageEvents-->>Admin: "Cannot publish from current status"
    else Validation fails
        EventService->>Event: Validate()
        Event-->>EventService: errors list
        EventService-->>ManageEvents: throws DomainValidationException
        ManageEvents-->>Admin: Shows validation errors
    else Valid Draft event
        EventService->>Event: Publish()
        Event-->>EventService: Status = Published
        EventService->>EventRepo: UpdateAsync(event)
        EventRepo->>DB: UPDATE Event SET Status=Published
        DB-->>EventRepo: Saved
        EventService->>Notification: NotifyEventPublishedAsync(event)
        Notification-->>EventService: Logged
        EventService-->>ManageEvents: Success
        ManageEvents-->>Admin: Event now shows as "Published"
    end
```

---

## 6. Sequence Diagram — Admin Creates Event via Factory

```mermaid
sequenceDiagram
    actor Admin
    participant CreateEdit as CreateEditEvent.razor
    participant Factory as EventFactory
    participant EventService as EventService
    participant EventRepo as EventRepository
    participant DB as SQL Server

    Admin->>CreateEdit: Fill form (name, type, date, venue, activities)
    Admin->>CreateEdit: Click "Save"
    CreateEdit->>Factory: CreateInPersonEvent(name, desc, date, start, end, venueId, capacity)

    alt Name is empty/whitespace
        Factory-->>CreateEdit: throws ValidationException(FieldName="name")
        CreateEdit-->>Admin: "Name is required"
    else Capacity <= 0
        Factory-->>CreateEdit: throws ValidationException(FieldName="maxCapacity")
        CreateEdit-->>Admin: "Capacity must be greater than zero"
    else Valid inputs
        Factory-->>CreateEdit: new Event { Type=InPerson, Status=Draft }
        CreateEdit->>EventService: CreateEventAsync(event, activityIds[])
        EventService->>EventRepo: AddAsync(event)
        EventRepo->>DB: INSERT Event
        DB-->>EventRepo: Event with Id assigned
        EventService->>DB: INSERT EventActivity rows
        DB-->>EventService: Saved
        EventService-->>CreateEdit: Created Event
        CreateEdit-->>Admin: Redirect to Manage Events
    end
```

---

## 7. Component Diagram — Application Architecture

```mermaid
graph TB
    subgraph "Presentation Layer (Blazor Components)"
        UI1[Home.razor]
        UI2[EventList.razor]
        UI3[EventDetail.razor]
        UI4[ManageEvents.razor]
        UI5[CreateEditEvent.razor]
        UI6[ManageVenues.razor]
        UI7[ManageActivities.razor]
        UI8[ManageParticipants.razor]
        UI9[MyRegistrations.razor]
        UI10[AllRegistrations.razor]
    end

    subgraph "Business Logic Layer (Services)"
        S1[EventService]
        S2[RegistrationService]
        S3[ParticipantService]
        S4[VenueService]
        S5[ActivityService]
        S6[NotificationService]
    end

    subgraph "Design Patterns"
        P1[EventFactory]
        P2[CompositeFilterStrategy]
        P3[DateFilterStrategy]
        P4[VenueFilterStrategy]
        P5[KeywordFilterStrategy]
        P6[ActivityTypeFilterStrategy]
    end

    subgraph "Data Access Layer (Repositories)"
        R1[EventRepository]
        R2[RegistrationRepository]
        R3[ParticipantRepository]
        R4[VenueRepository]
        R5[ActivityRepository]
        R6[BaseRepository<T>]
    end

    subgraph "Infrastructure"
        DB[(SQL Server LocalDB)]
        ID[ASP.NET Core Identity]
        EF[Entity Framework Core]
    end

    UI1 --> S1
    UI2 --> S1
    UI3 --> S1
    UI3 --> S2
    UI4 --> S1
    UI5 --> S1
    UI5 --> P1
    UI6 --> S4
    UI7 --> S5
    UI8 --> S3
    UI9 --> S2
    UI10 --> S2

    S1 --> P2
    P2 --> P3
    P2 --> P4
    P2 --> P5
    P2 --> P6
    S1 --> S6
    S2 --> S6

    S1 --> R1
    S2 --> R2
    S3 --> R3
    S4 --> R4
    S5 --> R5

    R1 --> R6
    R2 --> R6
    R3 --> R6
    R4 --> R6
    R5 --> R6

    R6 --> EF
    EF --> DB
    ID --> DB
```

---

## 8. Entity Relationship Diagram (Database Schema)

```mermaid
erDiagram
    Events {
        int Id PK
        string Name
        string Description
        datetime EventDate
        time StartTime
        time EndTime
        int Status
        int Type
        int MaxCapacity
        int VenueId FK
        datetime CreatedAt
        datetime UpdatedAt
        bool IsDeleted
    }

    Venues {
        int Id PK
        string Name
        string Address
        int Capacity
        string Description
        string ContactPhone
        string ContactEmail
        datetime CreatedAt
        datetime UpdatedAt
        bool IsDeleted
    }

    Participants {
        int Id PK
        string FirstName
        string LastName
        string Email
        string PhoneNumber
        string Bio
        string ApplicationUserId FK
        datetime CreatedAt
        datetime UpdatedAt
        bool IsDeleted
    }

    Activities {
        int Id PK
        string Name
        string Description
        int Type
        int DurationMinutes
        datetime CreatedAt
        datetime UpdatedAt
        bool IsDeleted
    }

    Registrations {
        int Id PK
        int ParticipantId FK
        int EventId FK
        int Status
        datetime RegistrationDate
        string Notes
        datetime CreatedAt
        datetime UpdatedAt
        bool IsDeleted
    }

    EventActivities {
        int EventId PK,FK
        int ActivityId PK,FK
        int DisplayOrder
        time ScheduledStartTime
    }

    AspNetUsers {
        string Id PK
        string Email
        string UserName
        string PasswordHash
    }

    AspNetRoles {
        string Id PK
        string Name
    }

    Events ||--o{ Registrations : "has"
    Events ||--o{ EventActivities : "scheduled for"
    Activities ||--o{ EventActivities : "appears in"
    Participants ||--o{ Registrations : "makes"
    Venues ||--o{ Events : "hosts"
    AspNetUsers ||--o| Participants : "linked to"
```
