﻿namespace NServiceBus.Settings;

using System;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class SettingsHolderTests
{
    [Test]
    public void Clear_ShouldDisposeAllDisposables()
    {
        var firstOverrideDisposable = new SomeDisposable();
        var secondOverrideDisposable = new SomeDisposable();
        var firstDefaultDisposable = new SomeDisposable();
        var secondDefaultDisposable = new SomeDisposable();

        var all = new[]
        {
            firstDefaultDisposable,
            secondDefaultDisposable,
            firstOverrideDisposable,
            secondOverrideDisposable
        };

        var settings = new SettingsHolder();
        settings.Set("1.Override", firstOverrideDisposable);
        settings.Set("2.Override", secondOverrideDisposable);
        settings.SetDefault("1.Default", firstDefaultDisposable);
        settings.SetDefault("2.Default", secondDefaultDisposable);

        settings.Clear();

        Assert.That(all.All(x => x.Disposed), Is.True);
    }

    [Test]
    public void Merge_ShouldMergeContentFromSource()
    {
        var settings = new SettingsHolder();
        settings.SetDefault("SomeDefaultSetting", "Value1");
        settings.Set("SomeSetting", "Value1");

        var mergeFrom = new SettingsHolder();
        mergeFrom.SetDefault("SomeDefaultSettingThatGetsMerged", "Value1");
        mergeFrom.Set("SomeSettingThatGetsMerged", "Value1");

        settings.Merge(mergeFrom);

        var result1 = settings.Get<string>("SomeDefaultSettingThatGetsMerged");
        var result2 = settings.Get<string>("SomeSettingThatGetsMerged");

        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.EqualTo("Value1"));
            Assert.That(result2, Is.EqualTo("Value1"));
        });
    }

    [Test]
    public void Merge_ThrowsWhenChangesArePrevented()
    {
        var settings = new SettingsHolder();
        var mergeFrom = new SettingsHolder();

        settings.PreventChanges();

        Assert.Throws<Exception>(() => settings.Merge(mergeFrom));
    }

    class SomeDisposable : IDisposable
    {
        public void Dispose()
        {
            Disposed = true;
        }

        public bool Disposed;
    }
}