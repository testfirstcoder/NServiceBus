﻿namespace NServiceBus.Core.Tests.Routing;

using System;
using System.Threading.Tasks;
using Extensibility;
using NUnit.Framework;
using Testing;

[TestFixture]
public class ApplyReplyToAddressBehaviorTests
{
    [Test]
    public async Task Should_use_public_return_address_if_specified()
    {
        var behavior = new ApplyReplyToAddressBehavior(new ReceiveAddresses("MyEndpoint", "MyInstance"), "PublicAddress");
        var options = new SendOptions();
        var context = CreateContext(options);

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.Headers[Headers.ReplyToAddress], Is.EqualTo("PublicAddress"));
    }

    static TestableOutgoingLogicalMessageContext CreateContext(ExtendableOptions options)
    {
        var context = new TestableOutgoingLogicalMessageContext
        {
            Extensions = options.Context
        };

        return context;
    }

    [Test]
    public async Task Should_default_to_setting_the_reply_to_header_to_this_endpoint()
    {
        var behavior = new ApplyReplyToAddressBehavior(new ReceiveAddresses("MyEndpoint", "MyInstance"), null);
        var options = new SendOptions();
        var context = CreateContext(options);

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.Headers[Headers.ReplyToAddress], Is.EqualTo("MyEndpoint"));
    }

    [Test]
    public async Task Should_set_the_reply_to_header_to_this_endpoint_when_requested()
    {
        var behavior = new ApplyReplyToAddressBehavior(new ReceiveAddresses("MyEndpoint", "MyInstance"), null);
        var options = new SendOptions();

        options.RouteReplyToAnyInstance();

        var context = CreateContext(options);
        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.Headers[Headers.ReplyToAddress], Is.EqualTo("MyEndpoint"));
    }

    [Test]
    public async Task Should_set_the_reply_to_header_to_this_instance_when_requested()
    {
        var behavior = new ApplyReplyToAddressBehavior(new ReceiveAddresses("MyEndpoint", "MyInstance"), null);
        var options = new SendOptions();

        options.RouteReplyToThisInstance();

        var context = CreateContext(options);
        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.Headers[Headers.ReplyToAddress], Is.EqualTo("MyInstance"));
    }

    [Test]
    public async Task Should_set_the_reply_to_header_a_specified_address_when_requested()
    {
        var behavior = new ApplyReplyToAddressBehavior(new ReceiveAddresses("MyEndpoint", "MyInstance"), null);
        var options = new SendOptions();

        options.RouteReplyTo("Destination");

        var context = CreateContext(options);
        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.Headers[Headers.ReplyToAddress], Is.EqualTo("Destination"));
    }

    [Test]
    public async Task Should_throw_when_trying_to_route_replies_to_this_instance_when_no_instance_id_is_used()
    {
        var behavior = new ApplyReplyToAddressBehavior(new ReceiveAddresses("MyEndpoint"), null);
        var options = new SendOptions();

        options.RouteReplyToThisInstance();

        var context = CreateContext(options);

        try
        {
            await behavior.Invoke(context, ctx => Task.CompletedTask);
            Assert.Fail("Expected exception");
        }
        catch (Exception ex) when (!ex.IsCausedBy(context.CancellationToken))
        {
            Assert.Pass();
        }
    }

    [Test]
    public async Task Should_throw_when_conflicting_settings_are_specified()
    {
        var behavior = new ApplyReplyToAddressBehavior(new ReceiveAddresses("MyEndpoint", "MyInstance"), null);

        var options = new SendOptions();
        var context = CreateContext(options);

        try
        {

            options.RouteReplyToAnyInstance();
            options.RouteReplyToThisInstance();

            await behavior.Invoke(context, ctx => Task.CompletedTask);
            Assert.Fail("Expected exception");
        }
        catch (Exception ex) when (!ex.IsCausedBy(context.CancellationToken))
        {
            Assert.Pass();
        }
    }
}