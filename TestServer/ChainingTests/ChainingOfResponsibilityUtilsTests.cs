using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server;
using Server.Domain.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace TestServer.ChainingTests
{
    public class MockEventSubscriber : IClientEventSubscriber<PreGameClientMessage>
    {
        public List<PreGameClientMessage> ReceivedMessages { get; } = new();

        public Task EventHandle(Guid? clientId, PreGameClientMessage clientMessage)
        {
            ReceivedMessages.Add(clientMessage);
            return Task.CompletedTask;
        }
    }

    [TestFixture]
    public class ChainingOfResponsibilityUtilsTests
    {
        [Test]
        public void MainScenario()
        {
            var services = new ServiceCollection()
                .AddMessageType<PreGameClientMessage>()
                .AddSingleton<ChainResolver>()
                .BuildServiceProvider();
            var chainResolver = services.GetService<ChainResolver>()!;
            var clientId = Guid.NewGuid();
            var chain = chainResolver.GetChainForClient(clientId);
            var subscriptionManager = services.GetService<ISubscriptionManager<PreGameClientMessage>>()!;
            var subscriber = new MockEventSubscriber();
            subscriptionManager.SubscribeForClient(clientId, subscriber);
            chain(new Greeting());
            chain(new StartGame());
            subscriptionManager.UnsubscribeFromClient(clientId);
            chain(new Greeting());
            Assert.AreEqual(1, subscriber.ReceivedMessages.Count);
        }
    }
}