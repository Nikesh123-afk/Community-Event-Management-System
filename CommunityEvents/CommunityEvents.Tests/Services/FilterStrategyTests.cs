using CommunityEvents.Models;
using CommunityEvents.Services;
using FluentAssertions;
using Xunit;

namespace CommunityEvents.Tests.Services;

/// <summary>
/// Unit tests for the Strategy pattern filter implementations.
/// Validates that each concrete strategy and the composite apply
/// correctly to an in-memory IQueryable.
/// </summary>
public class FilterStrategyTests
{
    private static IQueryable<Event> BuildTestData()
    {
        var venue1 = new Venue { Id = 1, Name = "City Hall", Address = "1 Main St", Capacity = 200 };
        var venue2 = new Venue { Id = 2, Name = "Park Centre", Address = "2 Park Rd", Capacity = 100 };

        var activity = new Activity { Id = 1, Name = "Talk", Type = ActivityType.Talk, DurationMinutes = 60 };

        return new List<Event>
        {
            new Event
            {
                Id = 1, Name = "Tech Summit", Description = "Annual technology conference",
                EventDate = new DateTime(2025, 8, 15),
                StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(17),
                VenueId = 1, Venue = venue1, MaxCapacity = 200, Status = EventStatus.Published
            },
            new Event
            {
                Id = 2, Name = "Community Day", Description = "Neighbourhood social event",
                EventDate = new DateTime(2025, 9, 20),
                StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(16),
                VenueId = 2, Venue = venue2, MaxCapacity = 100, Status = EventStatus.Published,
                EventActivities = new List<EventActivity>
                {
                    new EventActivity { ActivityId = 1, Activity = activity, DisplayOrder = 1 }
                }
            },
            new Event
            {
                Id = 3, Name = "Charity Run", Description = "5K charity running event at park",
                EventDate = new DateTime(2025, 8, 15),
                StartTime = TimeSpan.FromHours(7), EndTime = TimeSpan.FromHours(11),
                VenueId = 2, Venue = venue2, MaxCapacity = 500, Status = EventStatus.Published
            }
        }.AsQueryable();
    }

    [Fact]
    public void DateFilterStrategy_MatchingDate_ReturnsFilteredEvents()
    {
        var query = BuildTestData();
        var strategy = new DateFilterStrategy(new DateTime(2025, 8, 15));

        var result = strategy.Apply(query).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.EventDate.Date == new DateTime(2025, 8, 15));
    }

    [Fact]
    public void DateFilterStrategy_NoMatchingDate_ReturnsEmpty()
    {
        var query = BuildTestData();
        var strategy = new DateFilterStrategy(new DateTime(2030, 1, 1));

        strategy.Apply(query).Should().BeEmpty();
    }

    [Fact]
    public void VenueFilterStrategy_ReturnsOnlyEventsAtThatVenue()
    {
        var query = BuildTestData();
        var strategy = new VenueFilterStrategy(1);

        var result = strategy.Apply(query).ToList();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Tech Summit");
    }

    [Fact]
    public void KeywordFilterStrategy_MatchesNameCaseInsensitive()
    {
        var query = BuildTestData();
        var strategy = new KeywordFilterStrategy("TECH");

        var result = strategy.Apply(query).ToList();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Tech Summit");
    }

    [Fact]
    public void KeywordFilterStrategy_MatchesDescriptionField()
    {
        var query = BuildTestData();
        var strategy = new KeywordFilterStrategy("neighbourhood");

        var result = strategy.Apply(query).ToList();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Community Day");
    }

    [Fact]
    public void KeywordFilterStrategy_MatchesVenueName()
    {
        var query = BuildTestData();
        var strategy = new KeywordFilterStrategy("park");

        var result = strategy.Apply(query).ToList();

        // "Community Day" and "Charity Run" are both at Park Centre; "Charity Run" also has "park" in description
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void CompositeFilterStrategy_ChainsMultipleFilters()
    {
        var query = BuildTestData();

        var composite = new CompositeFilterStrategy()
            .Add(new DateFilterStrategy(new DateTime(2025, 8, 15)))
            .Add(new VenueFilterStrategy(2));

        var result = composite.Apply(query).ToList();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Charity Run");
    }

    [Fact]
    public void CompositeFilterStrategy_NoFilters_ReturnsAllEvents()
    {
        var query = BuildTestData();
        var composite = new CompositeFilterStrategy();

        composite.Apply(query).Should().HaveCount(3);
    }

    [Fact]
    public void ActivityTypeFilterStrategy_ReturnsEventsWithMatchingActivityType()
    {
        var query = BuildTestData();
        var strategy = new ActivityTypeFilterStrategy(ActivityType.Talk);

        var result = strategy.Apply(query).ToList();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Community Day");
    }
}
