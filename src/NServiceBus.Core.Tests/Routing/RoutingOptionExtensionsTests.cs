﻿namespace NServiceBus.Core.Tests.Routing;

using NUnit.Framework;

[TestFixture]
public class RoutingOptionExtensionsTests
{
    [Test]
    public void ReplyOptions_GetDestination_Should_Return_Configured_Destination()
    {
        const string expectedDestination = "custom reply destination";
        var options = new ReplyOptions();
        options.SetDestination(expectedDestination);

        var destination = options.GetDestination();

        Assert.That(destination, Is.EqualTo(expectedDestination));
    }

    [Test]
    public void ReplyOptions_GetDestination_Should_Return_Null_When_No_Destination_Configured()
    {
        var options = new ReplyOptions();

        var destination = options.GetDestination();

        Assert.That(destination, Is.Null);
    }

    [Test]
    public void SendOptions_GetDestination_Should_Return_Configured_Destination()
    {
        const string expectedDestination = "custom send destination";
        var options = new SendOptions();
        options.SetDestination(expectedDestination);

        var destination = options.GetDestination();

        Assert.That(destination, Is.EqualTo(expectedDestination));
    }

    [Test]
    public void SendOptions_GetDestination_Should_Return_Null_When_No_Destination_Configured()
    {
        var options = new SendOptions();

        var destination = options.GetDestination();

        Assert.That(destination, Is.Null);
    }

    [Test]
    public void IsRoutingToThisEndpoint_Should_Return_False_When_Not_Routed_To_This_Endpoint()
    {
        var options = new SendOptions();

        Assert.That(options.IsRoutingToThisEndpoint(), Is.False);
    }

    [Test]
    public void IsRoutingToThisEndpoint_Should_Return_True_When_Routed_To_This_Endpoint()
    {
        var options = new SendOptions();

        options.RouteToThisEndpoint();

        Assert.That(options.IsRoutingToThisEndpoint(), Is.True);
    }

    [Test]
    public void IsRoutingToThisInstance_Should_Return_False_When_Not_Routed_To_This_Instance()
    {
        var options = new SendOptions();

        Assert.That(options.IsRoutingToThisInstance(), Is.False);
    }

    [Test]
    public void IsRoutingToThisInstance_Should_Return_True_When_Routed_To_This_Instance()
    {
        var options = new SendOptions();

        options.RouteToThisInstance();

        Assert.That(options.IsRoutingToThisInstance(), Is.True);
    }

    [Test]
    public void GetRouteToSpecificInstance_Should_Return_Configured_Instance()
    {
        const string expectedInstanceId = "custom instance id";
        var options = new SendOptions();

        options.RouteToSpecificInstance(expectedInstanceId);

        Assert.That(options.GetRouteToSpecificInstance(), Is.EqualTo(expectedInstanceId));
    }

    [Test]
    public void GetRouteToSpecificInstance_Should_Return_Null_When_No_Instance_Configured()
    {
        var options = new SendOptions();

        Assert.That(options.GetRouteToSpecificInstance(), Is.Null);
    }

    [Test]
    public void SendOptions_IsRoutingReplyToThisInstance_Should_Return_True_When_Routing_Reply_To_This_Instance()
    {
        var options = new SendOptions();

        options.RouteReplyToThisInstance();

        Assert.That(options.IsRoutingReplyToThisInstance(), Is.True);
    }

    [Test]
    public void SendOptions_IsRoutingReplyToThisInstance_Should_Return_False_When_Not_Routing_Reply_To_This_Instance()
    {
        var options = new SendOptions();

        Assert.That(options.IsRoutingReplyToThisInstance(), Is.False);
    }

    [Test]
    public void ReplyOptions_IsRoutingReplyToThisInstance_Should_Return_True_When_Routing_Reply_To_This_Instance()
    {
        var options = new ReplyOptions();

        options.RouteReplyToThisInstance();

        Assert.That(options.IsRoutingReplyToThisInstance(), Is.True);
    }

    [Test]
    public void ReplyOptions_IsRoutingReplyToThisInstance_Should_Return_False_When_Not_Routing_Reply_To_This_Instance()
    {
        var options = new ReplyOptions();

        Assert.That(options.IsRoutingReplyToThisInstance(), Is.False);
    }

    [Test]
    public void SendOptions_IsRoutingReplyToAnyInstance_Should_Return_True_When_Routing_Reply_To_Any_Instance()
    {
        var options = new SendOptions();

        options.RouteReplyToAnyInstance();

        Assert.That(options.IsRoutingReplyToAnyInstance(), Is.True);
    }

    [Test]
    public void SendOptions_IsRoutingReplyToAnyInstance_Should_Return_False_When_Not_Routing_Reply_To_Any_Instance()
    {
        var options = new SendOptions();

        Assert.That(options.IsRoutingReplyToAnyInstance(), Is.False);
    }

    [Test]
    public void ReplyOptions_IsRoutingReplyToAnyInstance_Should_Return_True_When_Routing_Reply_To_Any_Instance()
    {
        var options = new ReplyOptions();

        options.RouteReplyToAnyInstance();

        Assert.That(options.IsRoutingReplyToAnyInstance(), Is.True);
    }

    [Test]
    public void ReplyOptions_IsRoutingReplyToAnyInstance_Should_Return_False_When_Not_Routing_Reply_To_Any_Instance()
    {
        var options = new ReplyOptions();

        Assert.That(options.IsRoutingReplyToAnyInstance(), Is.False);
    }

    [Test]
    public void ReplyOptions_GetReplyToRoute_Should_Return_Configured_Reply_Route()
    {
        const string expectedReplyToAddress = "custom replyTo address";
        var options = new ReplyOptions();

        options.RouteReplyTo(expectedReplyToAddress);

        Assert.That(options.GetReplyToRoute(), Is.EqualTo(expectedReplyToAddress));
    }

    [Test]
    public void ReplyOptions_GetReplyToRoute_Should_Return_Null_When_No_Route_Configured()
    {
        var options = new ReplyOptions();

        Assert.That(options.GetReplyToRoute(), Is.Null);
    }

    [Test]
    public void SendOptions_GetReplyToRoute_Should_Return_Configured_Reply_Route()
    {
        const string expectedReplyToAddress = "custom replyTo address";
        var options = new SendOptions();

        options.RouteReplyTo(expectedReplyToAddress);

        Assert.That(options.GetReplyToRoute(), Is.EqualTo(expectedReplyToAddress));
    }

    [Test]
    public void SendOptions_GetReplyToRoute_Should_Return_Null_When_No_Route_Configured()
    {
        var options = new SendOptions();

        Assert.That(options.GetReplyToRoute(), Is.Null);
    }
}