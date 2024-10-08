﻿namespace NServiceBus.AcceptanceTests.Core.TransportSeam;

using System.Collections.Generic;
using System.Threading.Tasks;
using AcceptanceTesting;
using EndpointTemplates;
using FakeTransport;
using NUnit.Framework;
using Transport;

public class When_initializing_transport : NServiceBusAcceptanceTest
{
    [Test]
    public async Task Should_follow_the_startup_sequence()
    {
        var context = await Scenario.Define<Context>()
            .WithEndpoint<Endpoint>()
            .Done(c => c.EndpointsStarted)
            .Run();

        Assert.That(context.StartUpSequence, Is.EqualTo(new List<string>
        {
            $"{nameof(TransportDefinition)}.{nameof(TransportDefinition.Initialize)}",
            $"{nameof(IMessageReceiver)}.{nameof(IMessageReceiver.Initialize)} for receiver Main",
            $"{nameof(IMessageReceiver)}.{nameof(IMessageReceiver.StartReceive)} for receiver Main",
            $"{nameof(IMessageReceiver)}.{nameof(IMessageReceiver.StopReceive)} for receiver Main",
            $"{nameof(TransportInfrastructure)}.{nameof(TransportInfrastructure.Shutdown)}",
        }).AsCollection);
    }

    [Test]
    public async Task Should_follow_the_startup_sequence_for_send_only_endpoints()
    {
        var context = await Scenario.Define<Context>()
            .WithEndpoint<Endpoint>(e => e.CustomConfig(c => c.SendOnly()))
            .Done(c => c.EndpointsStarted)
            .Run();

        Assert.That(context.StartUpSequence, Is.EqualTo(new List<string>
        {
            $"{nameof(TransportDefinition)}.{nameof(TransportDefinition.Initialize)}",
            $"{nameof(TransportInfrastructure)}.{nameof(TransportInfrastructure.Shutdown)}",
        }).AsCollection);
    }

    class Context : ScenarioContext
    {
        public FakeTransport.StartUpSequence StartUpSequence { get; set; }
    }

    class Endpoint : EndpointConfigurationBuilder
    {
        public Endpoint()
        {
            EndpointSetup<DefaultServer, Context>((endpointConfig, context) =>
            {
                var fakeTransport = new FakeTransport();
                context.StartUpSequence = fakeTransport.StartupSequence;
                endpointConfig.UseTransport(fakeTransport);
            });
        }
    }
}
