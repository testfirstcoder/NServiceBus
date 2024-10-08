﻿namespace NServiceBus.Core.Tests.Routing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using NServiceBus.Routing;
using NUnit.Framework;
using Testing;
using Transport;

[TestFixture]
public class DetermineRouteForReplyBehaviorTests
{
    [Test]
    public async Task Should_default_to_reply_address_of_incoming_message_for_replies()
    {
        var behavior = new ReplyConnector();

        var context = CreateContext(new OutgoingLogicalMessage(typeof(MyReply), new MyReply()));

        context.Extensions.Set(new IncomingMessage(
            "id",
            new Dictionary<string, string>
            {
                {Headers.ReplyToAddress, "ReplyAddressOfIncomingMessage"}
            },
            Array.Empty<byte>()));

        UnicastAddressTag addressTag = null;
        await behavior.Invoke(context, c =>
        {
            addressTag = (UnicastAddressTag)c.RoutingStrategies.Single().Apply([]);
            return Task.CompletedTask;
        });

        Assert.That(addressTag.Destination, Is.EqualTo("ReplyAddressOfIncomingMessage"));
    }

    [Test]
    public void Should_throw_if_incoming_message_has_no_reply_to_address()
    {
        var behavior = new ReplyConnector();

        var context = CreateContext(new OutgoingLogicalMessage(typeof(MyReply), new MyReply()));

        context.Extensions.Set(new IncomingMessage(
            "id",
            [],
            Array.Empty<byte>()));

        Assert.That(async () => await behavior.Invoke(context, _ => Task.CompletedTask), Throws.InstanceOf<Exception>().And.Message.Contains(typeof(MyReply).FullName));
    }

    [Test]
    public async Task Should_use_explicit_route_for_replies_if_present()
    {
        var behavior = new ReplyConnector();
        var options = new ReplyOptions();

        options.SetDestination("CustomReplyToAddress");

        var context = CreateContext(new OutgoingLogicalMessage(typeof(MyReply), new MyReply()));
        context.Extensions = options.Context;

        UnicastAddressTag addressTag = null;
        await behavior.Invoke(context, c =>
        {
            addressTag = (UnicastAddressTag)c.RoutingStrategies.Single().Apply([]);
            return Task.CompletedTask;
        });

        Assert.That(addressTag.Destination, Is.EqualTo("CustomReplyToAddress"));
    }

    static TestableOutgoingReplyContext CreateContext(OutgoingLogicalMessage message)
    {
        return new TestableOutgoingReplyContext
        {
            Message = message
        };
    }

    class MyReply
    {
    }
}