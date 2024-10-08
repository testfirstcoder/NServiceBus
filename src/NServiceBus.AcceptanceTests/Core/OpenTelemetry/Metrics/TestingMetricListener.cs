namespace NServiceBus.AcceptanceTests.Core.OpenTelemetry.Metrics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using NUnit.Framework;

class TestingMetricListener : IDisposable
{
    readonly MeterListener meterListener;

    public TestingMetricListener(string sourceName)
    {
        meterListener = new()
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == sourceName)
                {
                    TestContext.Out.WriteLine($"Subscribing to {instrument.Meter.Name}\\{instrument.Name}");
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };

        meterListener.SetMeasurementEventCallback((Instrument instrument,
            long measurement,
            ReadOnlySpan<KeyValuePair<string, object>> t,
            object _) =>
        {
            TestContext.Out.WriteLine($"{instrument.Meter.Name}\\{instrument.Name}:{measurement}");

            var tags = t.ToArray();
            ReportedMeters.AddOrUpdate(instrument.Name, measurement, (_, val) => val + measurement);
            Tags.AddOrUpdate(instrument.Name, _ => tags, (_, _) => tags);
        });
        meterListener.SetMeasurementEventCallback((Instrument instrument,
            double measurement,
            ReadOnlySpan<KeyValuePair<string, object>> t,
            object _) =>
        {
            TestContext.Out.WriteLine($"{instrument.Meter.Name}\\{instrument.Name}:{measurement}");
            var tags = t.ToArray();
            ReportedMeters.AddOrUpdate(instrument.Name, 1, (_, val) => val + 1);
            Tags.AddOrUpdate(instrument.Name, _ => tags, (_, _) => tags);
        });
        meterListener.Start();
    }

    public static TestingMetricListener SetupNServiceBusMetricsListener() =>
        SetupMetricsListener("NServiceBus.Core.Pipeline.Incoming");

    public static TestingMetricListener SetupMetricsListener(string sourceName)
    {
        var testingMetricListener = new TestingMetricListener(sourceName);
        return testingMetricListener;
    }

    public void Dispose() => meterListener?.Dispose();

    public ConcurrentDictionary<string, long> ReportedMeters { get; } = new();
    public ConcurrentDictionary<string, KeyValuePair<string, object>[]> Tags { get; } = new();

    public void AssertMetric(string metricName, long expected)
    {
        if (expected == 0)
        {
            Assert.That(ReportedMeters.ContainsKey(metricName), Is.False, $"Should not have '{metricName}' metric reported.");
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.That(ReportedMeters.ContainsKey(metricName), Is.True, $"'{metricName}' metric was not reported.");
                Assert.That(ReportedMeters[metricName], Is.EqualTo(expected));
            });
        }
    }

    public object AssertTagKeyExists(string metricName, string tagKey)
    {
        if (!Tags.ContainsKey(metricName))
        {
            Assert.Fail($"'{metricName}' metric was not reported");
        }

        var emptyTag = default(KeyValuePair<string, object>);
        var meterTag = Tags[metricName].FirstOrDefault(t => t.Key == tagKey);
        if (meterTag.Equals(emptyTag))
        {
            Assert.Fail($"'{tagKey}' tag was not found.");
        }

        return meterTag.Value;
    }

    public void AssertTags(string metricName, Dictionary<string, object> expectedTags)
    {
        foreach (var kvp in expectedTags)
        {
            var actualTagValue = AssertTagKeyExists(metricName, kvp.Key);
            Assert.That(actualTagValue, Is.EqualTo(kvp.Value));
        }
    }
}