﻿namespace NServiceBus.Core.Tests.Pipeline.Outgoing;

using NUnit.Framework;

[TestFixture]
public class MessageIdExtensionsTests
{
    [Test]
    public void GetMessageId_Should_Return_Generated_Id_When_No_Id_Specified()
    {
        var options = new SendOptions();

        Assert.That(options.GetMessageId(), Is.Not.Empty);
    }

    [Test]
    public void GetMessageId_Should_Return_Defined_Id()
    {
        const string expectedMessageID = "expected message id";
        var options = new PublishOptions();
        options.SetMessageId(expectedMessageID);

        Assert.That(options.GetMessageId(), Is.EqualTo(expectedMessageID));
    }
}