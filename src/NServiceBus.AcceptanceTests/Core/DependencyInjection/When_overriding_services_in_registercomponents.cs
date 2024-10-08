﻿namespace NServiceBus.AcceptanceTests.Core.DependencyInjection;

using System.Threading;
using System.Threading.Tasks;
using AcceptanceTesting;
using EndpointTemplates;
using Features;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

public class When_overriding_services_in_registercomponents : NServiceBusAcceptanceTest
{
    [Test]
    public async Task RegisterComponents_calls_override_registrations()
    {
        var serviceCollection = new ServiceCollection();

        var context = await Scenario.Define<Context>()
            .WithEndpoint<EndpointWithOverrides>(b => b
                .ToCreateInstance(
                    config =>
                    {
                        serviceCollection.AddSingleton<IDependencyBeforeEndpointConfiguration, OriginallyDefinedDependency>();
                        return EndpointWithExternallyManagedContainer.Create(config, serviceCollection);
                    },
                    (configured, ct) =>
                    {
                        serviceCollection.AddSingleton<IDependencyBeforeEndpointStart, OriginallyDefinedDependency>();
                        return configured.Start(serviceCollection.BuildServiceProvider(), ct);
                    }))
            .Done(c => c.EndpointsStarted)
            .Run();

        Assert.Multiple(() =>
        {
            Assert.That(context.DependencyFromFeature, Is.InstanceOf<OverridenDependency>());
            Assert.That(context.DependencyBeforeEndpointConfiguration, Is.InstanceOf<OverridenDependency>());

            // Registrations after the startable endpoint has been created can't be overriden by the RegisterComponents API
            Assert.That(context.DependencyBeforeEndpointStart, Is.InstanceOf<OriginallyDefinedDependency>());
        });
    }

    public class Context : ScenarioContext
    {
        public IDependencyFromFeature DependencyFromFeature { get; set; }
        public IDependencyBeforeEndpointConfiguration DependencyBeforeEndpointConfiguration { get; set; }
        public IDependencyBeforeEndpointStart DependencyBeforeEndpointStart { get; set; }
    }

    public class EndpointWithOverrides : EndpointConfigurationBuilder
    {
        public EndpointWithOverrides() =>
            EndpointSetup<DefaultServer>(c => c.RegisterComponents(s =>
            {
                s.AddSingleton<IDependencyFromFeature, OverridenDependency>();
                s.AddSingleton<IDependencyBeforeEndpointConfiguration, OverridenDependency>();
                s.AddSingleton<IDependencyBeforeEndpointStart, OverridenDependency>();
            }));

        public class StartupFeature : Feature
        {
            public StartupFeature() => EnableByDefault();

            protected override void Setup(FeatureConfigurationContext context)
            {
                context.Services.AddSingleton<IDependencyFromFeature, OriginallyDefinedDependency>();

                context.Services.AddTransient<StartupFeatureWithDependencies>();
                context.RegisterStartupTask(sp => sp.GetRequiredService<StartupFeatureWithDependencies>());
            }

            public class StartupFeatureWithDependencies : FeatureStartupTask
            {
                public StartupFeatureWithDependencies(
                    Context testContext,
                    IDependencyFromFeature dependencyFromFeature,
                    IDependencyBeforeEndpointConfiguration dependencyBeforeEndpointConfiguration,
                    IDependencyBeforeEndpointStart dependencyBeforeEndpointStart)
                {
                    testContext.DependencyFromFeature = dependencyFromFeature;
                    testContext.DependencyBeforeEndpointConfiguration = dependencyBeforeEndpointConfiguration;
                    testContext.DependencyBeforeEndpointStart = dependencyBeforeEndpointStart;
                }

                protected override Task OnStart(IMessageSession session, CancellationToken cancellationToken = default)
                    => Task.CompletedTask;

                protected override Task OnStop(IMessageSession session, CancellationToken cancellationToken = default)
                    => Task.CompletedTask;
            }
        }
    }

    public interface IDependencyFromFeature
    {
    }

    public interface IDependencyBeforeEndpointConfiguration
    {
    }

    public interface IDependencyBeforeEndpointStart
    {
    }

    public class OriginallyDefinedDependency :
        IDependencyFromFeature,
        IDependencyBeforeEndpointConfiguration,
        IDependencyBeforeEndpointStart
    {
    }

    public class OverridenDependency :
        IDependencyFromFeature,
        IDependencyBeforeEndpointConfiguration,
        IDependencyBeforeEndpointStart
    {
    }
}