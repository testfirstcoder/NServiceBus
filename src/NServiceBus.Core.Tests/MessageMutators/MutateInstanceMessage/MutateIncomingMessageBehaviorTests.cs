﻿namespace NServiceBus.Core.Tests.MessageMutators.MutateInstanceMessage;

using System.Threading.Tasks;
using MessageMutator;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Pipeline;
using NUnit.Framework;
using Testing;
using Unicast.Messages;

[TestFixture]
class MutateIncomingMessageBehaviorTests
{
    [Test]
    public async Task Should_invoke_all_explicit_mutators()
    {
        var mutator = new MutatorThatIndicatesIfItWasCalled();
        var otherMutator = new MutatorThatIndicatesIfItWasCalled();

        var behavior = new MutateIncomingMessageBehavior([mutator, otherMutator]);

        var context = new TestableIncomingLogicalMessageContext();

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.Multiple(() =>
        {
            Assert.That(mutator.MutateIncomingCalled, Is.True);
            Assert.That(otherMutator.MutateIncomingCalled, Is.True);
        });
    }

    [Test]
    public async Task Should_invoke_both_explicit_and_container_provided_mutators()
    {
        var explicitMutator = new MutatorThatIndicatesIfItWasCalled();
        var containerMutator = new MutatorThatIndicatesIfItWasCalled();

        var behavior = new MutateIncomingMessageBehavior([explicitMutator]);

        var context = new TestableIncomingLogicalMessageContext();
        context.Services.AddTransient<IMutateIncomingMessages>(sp => containerMutator);

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.Multiple(() =>
        {
            Assert.That(explicitMutator.MutateIncomingCalled, Is.True);
            Assert.That(containerMutator.MutateIncomingCalled, Is.True);
        });
    }

    [Test]
    public async Task Should_not_call_MutateIncoming_when_hasIncomingMessageMutators_is_false()
    {
        var behavior = new MutateIncomingMessageBehavior([]);

        var context = new TestableIncomingLogicalMessageContext();

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        var mutator = new MutatorThatIndicatesIfItWasCalled();
        context.Services.AddTransient<IMutateIncomingMessages>(sp => mutator);

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(mutator.MutateIncomingCalled, Is.False);
    }

    [Test]
    public void Should_throw_friendly_exception_when_IMutateIncomingMessages_MutateIncoming_returns_null()
    {
        var behavior = new MutateIncomingMessageBehavior([]);

        var logicalMessage = new LogicalMessage(new MessageMetadata(typeof(TestMessage)), new TestMessage());

        var context = new TestableIncomingLogicalMessageContext
        {
            Message = logicalMessage
        };

        context.Services.AddTransient<IMutateIncomingMessages>(sp => new MutateIncomingMessagesReturnsNull());

        Assert.That(async () => await behavior.Invoke(context, ctx => Task.CompletedTask), Throws.Exception.With.Message.EqualTo("Return a Task or mark the method as async."));
    }

    [Test]
    public async Task When_no_mutator_updates_the_body_should_not_update_the_body()
    {
        var behavior = new MutateIncomingMessageBehavior([]);

        var context = new InterceptUpdateMessageIncomingLogicalMessageContext();

        context.Services.AddTransient<IMutateIncomingMessages>(sp => new MutatorWhichDoesNotMutateTheBody());

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.UpdateMessageCalled, Is.False);
    }

    [Test]
    public async Task When_no_mutator_available_should_not_update_the_body()
    {
        var behavior = new MutateIncomingMessageBehavior([]);

        var context = new InterceptUpdateMessageIncomingLogicalMessageContext();

        context.Services.AddTransient(sp => System.Array.Empty<IMutateIncomingMessages>());

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.UpdateMessageCalled, Is.False);
    }

    [Test]
    public async Task When_mutator_modifies_the_body_should_update_the_body()
    {
        var behavior = new MutateIncomingMessageBehavior([]);

        var context = new InterceptUpdateMessageIncomingLogicalMessageContext();

        context.Services.AddTransient<IMutateIncomingMessages>(sp => new MutatorWhichMutatesTheBody());

        await behavior.Invoke(context, ctx => Task.CompletedTask);

        Assert.That(context.UpdateMessageCalled, Is.True);
    }

    class InterceptUpdateMessageIncomingLogicalMessageContext : TestableIncomingLogicalMessageContext
    {
        public bool UpdateMessageCalled { get; private set; }

        public override void UpdateMessageInstance(object newInstance)
        {
            base.UpdateMessageInstance(newInstance);

            UpdateMessageCalled = true;
        }
    }

    class MutatorThatIndicatesIfItWasCalled : IMutateIncomingMessages
    {
        public bool MutateIncomingCalled { get; set; }

        public Task MutateIncoming(MutateIncomingMessageContext context)
        {
            MutateIncomingCalled = true;

            return Task.CompletedTask;
        }
    }

    class MutatorWhichDoesNotMutateTheBody : IMutateIncomingMessages
    {
        public Task MutateIncoming(MutateIncomingMessageContext context)
        {
            return Task.CompletedTask;
        }
    }

    class MutatorWhichMutatesTheBody : IMutateIncomingMessages
    {
        public Task MutateIncoming(MutateIncomingMessageContext context)
        {
            context.Message = new object();

            return Task.CompletedTask;
        }
    }

    class MutateIncomingMessagesReturnsNull : IMutateIncomingMessages
    {
        public Task MutateIncoming(MutateIncomingMessageContext context)
        {
            return null;
        }
    }

    class TestMessage : IMessage
    { }
}
