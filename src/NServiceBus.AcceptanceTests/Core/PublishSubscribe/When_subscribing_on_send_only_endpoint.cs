﻿namespace NServiceBus.AcceptanceTests.PublishSubscribe;

using System;
using AcceptanceTesting;
using EndpointTemplates;
using NUnit.Framework;

public class When_subscribing_on_send_only_endpoint : NServiceBusAcceptanceTest
{
    [Test]
    public void Should_throw_InvalidOperationException_on_native_pubsub()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => Scenario.Define<ScenarioContext>()
            .WithEndpoint<NativePubSubSendOnlyEndpoint>(e => e
                .When(s => s.Subscribe<SomeEvent>()))
            .Done(c => c.EndpointsStarted)
            .Run());

        Assert.That(exception.Message, Does.Contain("Send-only endpoints cannot subscribe to events"));
    }

    [Test]
    public void Should_throw_InvalidOperationException_on_message_driven_pubsub()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => Scenario.Define<ScenarioContext>()
            .WithEndpoint<MessageDrivenPubSubSendOnlyEndpoint>(e => e
                .When(s => s.Subscribe<SomeEvent>()))
            .Done(c => c.EndpointsStarted)
            .Run());

        Assert.That(exception.Message, Does.Contain("Send-only endpoints cannot subscribe to events"));
    }

    class NativePubSubSendOnlyEndpoint : EndpointConfigurationBuilder
    {
        public NativePubSubSendOnlyEndpoint()
        {
            var template = new DefaultServer
            {
                TransportConfiguration = new ConfigureEndpointAcceptanceTestingTransport(true, true)
            };
            EndpointSetup(template, (configuration, _) => configuration.SendOnly());
        }
    }

    class MessageDrivenPubSubSendOnlyEndpoint : EndpointConfigurationBuilder
    {
        public MessageDrivenPubSubSendOnlyEndpoint()
        {
            var template = new DefaultServer
            {
                TransportConfiguration = new ConfigureEndpointAcceptanceTestingTransport(false, true),
                PersistenceConfiguration = new ConfigureEndpointAcceptanceTestingPersistence()
            };

            EndpointSetup(template, (configuration, _) => configuration.SendOnly());
        }
    }

    public class SomeEvent : IEvent
    {
    }
}