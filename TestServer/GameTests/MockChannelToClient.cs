using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Application;
using Server.Application.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockChannelToClient : IChannelToClient<ServerMessage>
    {
        public IReadOnlyList<ServerMessage> Messages => _messages;

        private readonly List<ServerMessage> _messages;
        public Guid Id { get; }
        public string? Name { get; set; }

        public MockChannelToClient(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            _messages = new List<ServerMessage>();
        }

        public Task SendMessage(ServerMessage message)
        {
            _messages.Add(message);
            return Task.CompletedTask;
        }

        public IChannelToClient<TNew> AsNewHandler<TNew>() where TNew : ServerMessage => this;
    }
}