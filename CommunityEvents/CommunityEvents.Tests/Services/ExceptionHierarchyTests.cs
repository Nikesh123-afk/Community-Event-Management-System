using CommunityEvents.Exceptions;
using FluentAssertions;
using Xunit;

namespace CommunityEvents.Tests.Services;

/// <summary>
/// Tests verifying the custom exception hierarchy.
/// Demonstrates that exceptions carry correct messages and error codes,
/// and that the inheritance hierarchy is correctly structured.
/// </summary>
public class ExceptionHierarchyTests
{
    [Fact]
    public void ValidationException_InheritsFromAppException()
    {
        var ex = new ValidationException("Email", "must be valid");
        ex.Should().BeAssignableTo<AppException>();
        ex.ErrorCode.Should().Be("VALIDATION_ERROR");
        ex.FieldName.Should().Be("Email");
        ex.Message.Should().Contain("Email");
    }

    [Fact]
    public void EventNotFoundException_InheritsFromNotFoundException_AndAppException()
    {
        var ex = new EventNotFoundException(42);
        ex.Should().BeAssignableTo<NotFoundException>();
        ex.Should().BeAssignableTo<AppException>();
        ex.Message.Should().Contain("42");
        ex.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public void AlreadyRegisteredException_InheritsFromBusinessRuleException()
    {
        var ex = new AlreadyRegisteredException("alice@test.com", "Tech Summit");
        ex.Should().BeAssignableTo<BusinessRuleException>();
        ex.Message.Should().Contain("alice@test.com");
        ex.Message.Should().Contain("Tech Summit");
    }

    [Fact]
    public void EventCapacityExceededException_CarriesCorrectDetails()
    {
        var ex = new EventCapacityExceededException("Summer Fest", 100);
        ex.Message.Should().Contain("Summer Fest");
        ex.Message.Should().Contain("100");
    }

    [Fact]
    public void DomainValidationException_ExposesErrorsList()
    {
        var errors = new[] { "Name required", "Date is in the past" };
        var ex = new DomainValidationException(errors);

        ex.Errors.Should().HaveCount(2);
        ex.Errors.Should().Contain("Name required");
        ex.Should().BeAssignableTo<AppException>();
    }

    [Fact]
    public void DuplicateEntityException_HasCorrectErrorCode()
    {
        var ex = new DuplicateEntityException("Participant", "email already used");
        ex.ErrorCode.Should().Be("DUPLICATE_ENTITY");
        ex.Message.Should().Contain("Participant");
    }

    [Fact]
    public void InvalidStatusTransitionException_CarriesTransitionDetails()
    {
        var ex = new InvalidStatusTransitionException("Cancelled", "Published");
        ex.Message.Should().Contain("Cancelled");
        ex.Message.Should().Contain("Published");
        ex.Should().BeAssignableTo<BusinessRuleException>();
    }
}
