using CommunityEvents.Exceptions;
using CommunityEvents.Factories;
using CommunityEvents.Models;
using FluentAssertions;
using Xunit;

namespace CommunityEvents.Tests.Services;

/// <summary>
/// Tests for the EventFactory (Factory Design Pattern).
/// Validates that each factory method creates events in a correct, well-formed state.
/// </summary>
public class FactoryTests
{
    [Fact]
    public void CreateInPersonEvent_ValidArgs_ReturnsCorrectlyTypedDraftEvent()
    {
        var date = DateTime.Today.AddDays(10);

        var evt = EventFactory.CreateInPersonEvent(
            "Summer Fest", "A great community summer festival with lots of activities.",
            date, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0), venueId: 1, maxCapacity: 100);

        evt.Type.Should().Be(EventType.InPerson);
        evt.Status.Should().Be(EventStatus.Draft);
        evt.Name.Should().Be("Summer Fest");
        evt.MaxCapacity.Should().Be(100);
    }

    [Fact]
    public void CreateVirtualEvent_ValidArgs_ReturnsVirtualDraft()
    {
        var evt = EventFactory.CreateVirtualEvent(
            "Online Webinar", "Interactive webinar on advanced C# programming techniques.",
            DateTime.Today.AddDays(5), new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0), 500);

        evt.Type.Should().Be(EventType.Virtual);
        evt.Status.Should().Be(EventStatus.Draft);
        evt.MaxCapacity.Should().Be(500);
    }

    [Fact]
    public void CreateHybridEvent_ValidArgs_ReturnsHybridDraft()
    {
        var evt = EventFactory.CreateHybridEvent(
            "Hybrid Conference", "Conference with both in-person and online attendance options.",
            DateTime.Today.AddDays(20), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 1, 200);

        evt.Type.Should().Be(EventType.Hybrid);
        evt.Status.Should().Be(EventStatus.Draft);
    }

    [Fact]
    public void CreateInPersonEvent_EmptyName_ThrowsValidationException()
    {
        var act = () => EventFactory.CreateInPersonEvent(
            "", "Valid description here for the test.", DateTime.Today.AddDays(5),
            TimeSpan.Zero, TimeSpan.FromHours(2), 1, 50);

        act.Should().Throw<ValidationException>()
           .Which.FieldName.Should().Be("name");
    }

    [Fact]
    public void CreateVirtualEvent_NegativeCapacity_ThrowsValidationException()
    {
        var act = () => EventFactory.CreateVirtualEvent(
            "Webinar", "Some description for this online webinar.",
            DateTime.Today.AddDays(5), TimeSpan.Zero, TimeSpan.FromHours(2), -5);

        act.Should().Throw<ValidationException>()
           .Which.FieldName.Should().Be("maxCapacity");
    }

    [Fact]
    public void EventFactory_WhitespaceName_TrimsAndThrows()
    {
        var act = () => EventFactory.CreateInPersonEvent(
            "   ", "A valid description.", DateTime.Today.AddDays(5),
            TimeSpan.Zero, TimeSpan.FromHours(2), 1, 50);

        act.Should().Throw<ValidationException>();
    }
}
