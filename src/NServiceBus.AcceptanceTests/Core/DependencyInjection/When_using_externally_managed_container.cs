﻿namespace NServiceBus.AcceptanceTests.Core.DependencyInjection;

using System;
using System.Threading.Tasks;
using AcceptanceTesting;
using EndpointTemplates;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

public class When_using_externally_managed_container : NServiceBusAcceptanceTest
{
    static MyComponent myComponent = new MyComponent();

    [Test]
    public async Task Should_use_it_for_component_resolution()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(MyComponent), myComponent);

        var result = await Scenario.Define<Context>()
        .WithEndpoint<ExternallyManagedContainerEndpoint>(b =>
        {
            b.ToCreateInstance(
                    config => EndpointWithExternallyManagedContainer.Create(config, serviceCollection),
                    (configured, ct) => configured.Start(serviceCollection.BuildServiceProvider(), ct)
                )
                .When((session, c) => session.SendLocal(new SomeMessage()));
        })
        .Done(c => c.MessageReceived)
        .Run();

        Assert.Multiple(() =>
        {
            Assert.That(result.ServiceProvider, Is.Not.Null, "IServiceProvider should be injectable");
            Assert.That(result.CustomService, Is.SameAs(myComponent), "Should inject custom services");
        });
    }

    class Context : ScenarioContext
    {
        public bool MessageReceived { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public MyComponent CustomService { get; set; }
    }

    public class ExternallyManagedContainerEndpoint : EndpointConfigurationBuilder
    {
        public ExternallyManagedContainerEndpoint() => EndpointSetup<DefaultServer>();

        class SomeMessageHandler : IHandleMessages<SomeMessage>
        {
            public SomeMessageHandler(Context context, MyComponent component, IServiceProvider serviceProvider)
            {
                testContext = context;
                myComponent = component;

                testContext.CustomService = component;
                testContext.ServiceProvider = serviceProvider;
            }

            public Task Handle(SomeMessage message, IMessageHandlerContext context)
            {
                testContext.MessageReceived = true;
                return Task.CompletedTask;
            }

            readonly Context testContext;
        }
    }

    public class MyComponent
    {
    }

    public class SomeMessage : IMessage
    {
    }
}