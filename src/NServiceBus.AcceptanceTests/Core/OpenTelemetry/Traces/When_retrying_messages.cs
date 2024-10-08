﻿namespace NServiceBus.AcceptanceTests.Core.OpenTelemetry.Traces;

using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.AcceptanceTesting;
using NUnit.Framework;

public class When_retrying_messages : OpenTelemetryAcceptanceTest
{
    [Test]
    public async Task Should_correlate_immediate_retry_with_send()
    {
        await Scenario.Define<Context>()
            .WithEndpoint<RetryingEndpoint>(e => e
                .CustomConfig(c => c.Recoverability().Immediate(i => i.NumberOfRetries(1)))
                .DoNotFailOnErrorMessages()
                .When(s => s.SendLocal(new FailingMessage())))
            .Done(c => c.InvocationCounter == 2)
            .Run();

        var receiveActivities = NServicebusActivityListener.CompletedActivities.GetReceiveMessageActivities();
        var sendActivities = NServicebusActivityListener.CompletedActivities.GetSendMessageActivities();

        Assert.Multiple(() =>
        {
            Assert.That(sendActivities, Has.Count.EqualTo(1));
            Assert.That(receiveActivities, Has.Count.EqualTo(2), "the message should be processed twice due to one immediate retry");
        });
        Assert.Multiple(() =>
        {
            Assert.That(receiveActivities[0].ParentId, Is.EqualTo(sendActivities[0].Id), "should not change parent span");
            Assert.That(receiveActivities[1].ParentId, Is.EqualTo(sendActivities[0].Id), "should not change parent span");

            Assert.That(sendActivities.Concat(receiveActivities).All(a => a.TraceId == sendActivities[0].TraceId), Is.True, "all activities should be part of the same trace");
        });
    }

    [Test]
    public async Task Should_correlate_delayed_retry_with_send()
    {
        Requires.DelayedDelivery();

        await Scenario.Define<Context>()
            .WithEndpoint<RetryingEndpoint>(e => e
                .CustomConfig(c => c.Recoverability().Delayed(i => i.NumberOfRetries(1).TimeIncrease(TimeSpan.FromMilliseconds(1))))
                .DoNotFailOnErrorMessages()
                .When(s => s.SendLocal(new FailingMessage())))
            .Done(c => c.InvocationCounter == 2)
            .Run();

        var receiveActivities = NServicebusActivityListener.CompletedActivities.GetReceiveMessageActivities();
        var sendActivities = NServicebusActivityListener.CompletedActivities.GetSendMessageActivities();

        Assert.Multiple(() =>
        {
            Assert.That(sendActivities, Has.Count.EqualTo(1));
            Assert.That(receiveActivities, Has.Count.EqualTo(2), "the message should be processed twice due to one immediate retry");
        });
        Assert.Multiple(() =>
        {
            Assert.That(receiveActivities[0].ParentId, Is.EqualTo(sendActivities[0].Id), "should not change parent span");
            Assert.That(receiveActivities[1].ParentId, Is.EqualTo(sendActivities[0].Id), "should not change parent span");

            Assert.That(sendActivities.Concat(receiveActivities).All(a => a.TraceId == sendActivities[0].TraceId), Is.True, "all activities should be part of the same trace");
        });
    }

    class Context : ScenarioContext
    {
        public int InvocationCounter { get; set; }
    }

    class RetryingEndpoint : EndpointConfigurationBuilder
    {
        public RetryingEndpoint()
        {
            EndpointSetup<OpenTelemetryEnabledEndpoint>();
        }

        class Handler : IHandleMessages<FailingMessage>
        {
            Context testContext;

            public Handler(Context testContext)
            {
                this.testContext = testContext;
            }

            public Task Handle(FailingMessage message, IMessageHandlerContext context)
            {
                testContext.InvocationCounter++;

                if (testContext.InvocationCounter == 1)
                {
                    throw new SimulatedException("first attempt fails");
                }

                return Task.CompletedTask;
            }
        }
    }

    public class FailingMessage : IMessage
    {
    }
}