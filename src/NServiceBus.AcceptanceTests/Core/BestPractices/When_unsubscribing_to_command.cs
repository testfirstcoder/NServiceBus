﻿namespace NServiceBus.AcceptanceTests.Core.BestPractices;

using System;
using System.Threading.Tasks;
using AcceptanceTesting;
using EndpointTemplates;
using NUnit.Framework;

public class When_unsubscribing_to_command : NServiceBusAcceptanceTest
{
    [Test]
    public async Task Should_throw()
    {
        var context = await Scenario.Define<Context>()
            .WithEndpoint<Endpoint>(b => b.When(async (session, c) =>
            {
                try
                {
                    await session.Unsubscribe<MyCommand>();
                }
                catch (Exception ex)
                {
                    c.Exception = ex;
                    c.GotTheException = true;
                }
            }))
            .Done(c => c.GotTheException)
            .Run();

        Assert.That(context.Exception, Is.InstanceOf<Exception>());
    }

    public class Context : ScenarioContext
    {
        public bool GotTheException { get; set; }
        public Exception Exception { get; set; }
    }

    public class Endpoint : EndpointConfigurationBuilder
    {
        public Endpoint()
        {
            EndpointSetup<DefaultServer>();
        }
    }

    public class MyCommand : ICommand
    {
    }
}