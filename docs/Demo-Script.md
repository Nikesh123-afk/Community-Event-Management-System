# CET254 — Community Event Management System
# 10-Minute Video Demo Script (Targeting 100/100)

---

## BEFORE YOU START RECORDING

1. App is running at `https://localhost:7024`
2. Terminal open at project root
3. Browser tab open on app home page
4. Visual Studio / VS Code open with solution loaded

---

## SEGMENT 1 — Introduction (0:00–0:30)

**Say:**
> "This is my Community Event Management System built for CET254 Advanced Programming.
> The system uses Blazor Web App on .NET 8, Entity Framework Core Code-First with SQL Server,
> ASP.NET Core Identity for authentication and authorisation, and demonstrates advanced
> object-oriented programming with four design patterns: Factory, Strategy, Composite, and Observer."

**Show:** The running home page with stats dashboard.

---

## SEGMENT 2 — OOP: Inheritance & Polymorphism (0:30–2:00)

**Say:**
> "Let me show you the OOP architecture first."

**Open Visual Studio → Models/BaseEntity.cs**

**Say:**
> "Every domain model inherits from this abstract `BaseEntity` class. It encapsulates
> common audit fields — `CreatedAt`, `UpdatedAt`, `IsDeleted` for soft deletes — using
> private backing fields with protected setters. This is encapsulation."
>
> "It declares two abstract members: `DisplayName` and `Validate()`. This enforces a
> polymorphic contract across all subclasses."

**Open Models/Event.cs**

**Say:**
> "The `Event` class inherits from `BaseEntity` and overrides both abstract members.
> `DisplayName` returns the event name, and `Validate()` enforces business rules like
> minimum description length and valid date range."
>
> "I also use private backing fields with property validation here — setting
> `MaxCapacity` to a negative number throws a `ValidationException` immediately."

**Open Models/Participant.cs**

**Say:**
> "The `Participant` model similarly inherits `BaseEntity`. Notice `Email` has a regex
> validation in its `Validate()` override. All five domain models — Event, Venue,
> Participant, Activity, Registration — form a proper inheritance hierarchy."

---

## SEGMENT 3 — Custom Exception Hierarchy (2:00–2:45)

**Open Exceptions/AppExceptions.cs**

**Say:**
> "I built a full custom exception hierarchy rooted at `AppException` which adds an
> `ErrorCode` property. From there I have `ValidationException` for field-level errors,
> `NotFoundException` for missing entities, and `BusinessRuleException` for domain
> violations."
>
> "For example, `EventCapacityExceededException` inherits `BusinessRuleException`.
> `EventNotFoundException` inherits `NotFoundException`. This hierarchy means the UI
> can catch at different levels of specificity."

---

## SEGMENT 4 — Design Pattern: Factory (2:45–3:30)

**Open Factories/EventFactory.cs**

**Say:**
> "The Factory Pattern is in `EventFactory`. Instead of the UI directly instantiating
> events, it calls `CreateInPersonEvent`, `CreateVirtualEvent`, or `CreateHybridEvent`.
> Each factory method validates inputs — throwing `ValidationException` for empty name
> or zero capacity — and returns a properly typed, pre-configured `Event` in Draft status."

**Show in browser:** Go to Admin → Create Event

**Say:**
> "When I fill this form and save, the Blazor component calls `EventFactory` first,
> then passes the result to `EventService`. This ensures every event object is always
> in a valid initial state."

---

## SEGMENT 5 — Design Pattern: Strategy + Composite (3:30–4:30)

**Open Services/FilterStrategies.cs**

**Say:**
> "The Strategy Pattern is used for event filtering. The `IFilterStrategy` interface
> defines one method: `Apply(IQueryable<Event>)`. I have four concrete strategies:
> `DateFilterStrategy`, `VenueFilterStrategy`, `ActivityTypeFilterStrategy`, and
> `KeywordFilterStrategy`."
>
> "The `CompositeFilterStrategy` implements the Composite Pattern — it holds a list
> of strategies and chains them together with `Aggregate`, so all filters apply as
> AND logic."

**Show in browser:** Go to Events page → type a keyword → select a venue → select an activity type

**Say:**
> "Watch how filtering works. Each filter I apply adds a strategy to the composite.
> The EventService builds the composite dynamically at runtime based on which parameters
> are provided. This is open for extension — I can add new filter types without
> changing the service."

---

## SEGMENT 6 — Design Pattern: Observer / Notification (4:30–5:00)

**Open Interfaces/INotificationService.cs then Services/NotificationService.cs**

**Say:**
> "The Observer Pattern is implemented via `INotificationService`. When a registration
> is confirmed, the `RegistrationService` calls `NotifyRegistrationConfirmedAsync`.
> When an event is published or cancelled, `EventService` notifies observers."
>
> "In this demo the observer logs to `ILogger`, but the interface is decoupled — you
> could swap in an email service, SMS, or push notifications without changing any
> business logic."

---

## SEGMENT 7 — Repository Pattern & Database (5:00–5:45)

**Open Repositories/BaseRepository.cs**

**Say:**
> "The Repository Pattern abstracts data access. `BaseRepository<T>` provides generic
> CRUD with soft-delete filtering built in — `GetAllAsync` automatically excludes
> `IsDeleted = true` records. Concrete repositories like `EventRepository` extend this
> with domain-specific queries like `GetUpcomingEventsAsync` and `SearchEventsAsync`
> with eager loading."

**Open Data/ApplicationDbContext.cs**

**Say:**
> "The `ApplicationDbContext` inherits from `IdentityDbContext`, giving us ASP.NET Core
> Identity tables alongside our domain tables. EF Core Code-First migrations manage
> the schema. The context configures relationships, unique indexes, and seeds initial
> data — three venues and five activities."

---

## SEGMENT 8 — Authentication & Authorisation (5:45–6:30)

**Show in browser:** Log in at `/Account/Login`

- **Email:** `admin@community.events`
- **Password:** `Admin@1234`

**Say:**
> "Authentication uses ASP.NET Core Identity. The admin account is seeded automatically
> on startup. The `[Authorize]` attribute protects admin pages, and `[Authorize(Roles="Admin")]`
> restricts management pages to the Admin role."

**Show:** Admin dropdown in navbar appearing after login.

**Navigate to:** Manage Events, Manage Venues, Manage Participants, Manage Activities

**Say:**
> "These pages are only visible and accessible to admin users. Regular authenticated
> users can register for events and view their own registrations."

---

## SEGMENT 9 — Full CRUD Walkthrough (6:30–8:30)

### Create a Venue
1. Go to **Admin → Manage Venues**
2. Fill in: Name=`Tech Hub`, Address=`123 Main St`, Capacity=`200`
3. Click Save
> "Venue created with validation. The form uses Blazor's EditForm with DataAnnotations."

### Create an Event
1. Go to **Admin → Create Event**
2. Fill: Name=`Tech Summit 2025`, Type=`InPerson`, Date=`(future date)`, Venue=`Tech Hub`, Capacity=`50`
3. Select 2 activities (e.g. Workshop, Talk)
4. Click Save
> "The EventFactory creates the event in Draft status. Activities are linked via the EventActivity join table."

### Publish the Event
1. Go to **Manage Events** → find Tech Summit → click **Publish**
> "PublishEventAsync validates the event, transitions status from Draft to Published, and triggers the Observer notification."

### Register as a Participant
1. Log out → Register a new account
2. Go to **Events** → find Tech Summit → click Register
> "The RegistrationService checks for duplicate registration, checks capacity, auto-confirms and triggers the notification observer."

### View Registrations
1. Go to **My Registrations**
> "Shows registration status, event details, and option to cancel."

### Cancel the Event (admin)
1. Log back in as admin → Manage Events → Cancel Tech Summit
> "CancelEventAsync notifies all confirmed participants via the Observer pattern."

---

## SEGMENT 10 — Unit Tests (8:30–9:30)

**Switch to terminal**

```bash
cd CommunityEvents.Tests
dotnet test --verbosity normal
```

**While tests run, say:**
> "I have over 40 unit tests across 8 test classes."
>
> "EventServiceTests tests filtering, CRUD, publish/cancel workflows using EF InMemory."
>
> "RegistrationServiceTests tests capacity enforcement, duplicate detection, and wait-listing."
>
> "FilterStrategyTests tests each strategy individually and the CompositeFilterStrategy chaining."
>
> "FactoryTests verifies the Factory Pattern validates inputs correctly."
>
> "ExceptionHierarchyTests verifies the exception inheritance chain and error codes."
>
> "BaseRepositoryTests tests soft-delete behaviour, FindAsync with predicates, and CountAsync."

**When tests pass:**
> "All tests pass. The test suite uses xUnit, Moq for mocking `INotificationService`,
> FluentAssertions for readable assertions, and EF Core InMemory database for repository tests."

---

## SEGMENT 11 — Conclusion (9:30–10:00)

**Show home page dashboard**

**Say:**
> "To summarise: this system demonstrates full object-oriented principles —
> inheritance through `BaseEntity`, polymorphism via abstract `Validate()` and `DisplayName`,
> encapsulation with private backing fields, and abstraction through interfaces."
>
> "Four design patterns are implemented: Factory for event creation, Strategy for
> filtering, Composite for combining strategies, and Observer for notifications."
>
> "The architecture separates concerns cleanly: models hold domain logic, repositories
> handle data access, services orchestrate business rules, and Blazor components
> handle presentation."
>
> "The system is fully functional with authentication, role-based authorisation,
> 40+ passing unit tests, and a complete custom exception hierarchy."

---

## MARKING CRITERIA COVERAGE CHECKLIST

| Criterion | Implementation | Mark |
|-----------|---------------|------|
| **OOP — Inheritance** | All 5 models inherit `BaseEntity` abstract class | ✅ |
| **OOP — Polymorphism** | Abstract `DisplayName` + `Validate()` overridden in every subclass | ✅ |
| **OOP — Encapsulation** | Private `_eventDate`, `_maxCapacity`, `_email` backing fields with property validation | ✅ |
| **OOP — Abstraction** | Interfaces: `IRepository<T>`, `IEventService`, `IFilterStrategy`, `INotificationService` etc. | ✅ |
| **Design Pattern — Factory** | `EventFactory` with 3 static creation methods + validation | ✅ |
| **Design Pattern — Strategy** | `IFilterStrategy` + 4 concrete strategies | ✅ |
| **Design Pattern — Composite** | `CompositeFilterStrategy` combining strategies with Aggregate | ✅ |
| **Design Pattern — Observer** | `INotificationService` + `NotificationService` called by services | ✅ |
| **Design Pattern — Repository** | Generic `BaseRepository<T>` + 5 concrete repositories | ✅ |
| **Blazor Web App (.NET 8)** | Blazor Interactive Server render mode, 15 Razor components | ✅ |
| **EF Core Code-First** | Migrations, relationships, seed data, soft deletes | ✅ |
| **SQL Server LocalDB** | Connection string configured, LocalDB as data store | ✅ |
| **ASP.NET Core Identity** | User registration/login, admin role seeded, `[Authorize]` attributes | ✅ |
| **Bootstrap 5** | Responsive layout, cards, progress bars, badges, modals | ✅ |
| **CRUD — Events** | Full create/read/update/delete with status workflow | ✅ |
| **CRUD — Venues** | Full CRUD with capacity validation | ✅ |
| **CRUD — Participants** | Full CRUD with email uniqueness enforcement | ✅ |
| **CRUD — Activities** | Full CRUD with type enum and duration | ✅ |
| **Event Registration** | Register/cancel with capacity check + wait-listing | ✅ |
| **Wait-listing** | Auto wait-list when event is full, status tracked | ✅ |
| **Event Filtering** | Filter by date, venue, activity type, keyword — all combinable | ✅ |
| **Custom Exceptions** | 12 custom exception types in 3-level hierarchy | ✅ |
| **Unit Tests** | 40+ tests: services, repositories, factories, strategies, exceptions | ✅ |
| **Test Evidence** | `dotnet test` output shows all tests passing | ✅ |
| **UML Diagrams** | Class, sequence (3), component, ER diagrams | ✅ |
| **Many-to-Many** | Events ↔ Activities via `EventActivity` join table | ✅ |
| **Admin Role** | Separate admin pages with role-based authorisation | ✅ |
| **Soft Delete** | `IsDeleted` flag on all entities, filtered in repository | ✅ |
| **Dependency Injection** | All services/repos registered in `Program.cs`, injected via constructor | ✅ |
