using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Domain.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockChannelToClient : IChannelToClient<ServerMessage>
    {
        public List<ServerMessage> Messages { get; } = new();

        public Guid Id { get; }
        public string? Name { get; set; }

        public MockChannelToClient(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public Task SendMessage(ServerMessage message)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public IChannelToClient<TNew> AsNewHandler<TNew>() where TNew : ServerMessage => this;
    }
}