using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Application;
using Server.Application.ChainOfResponsibilityUtils;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockInGameClientInteractor : IChannelToClient<InGameServerMessage>
    {
        public IReadOnlyList<InGameServerMessage> Messages => _messages;

        private readonly List<InGameServerMessage> _messages;
        public Guid Id { get; }
        public string? Name { get; set; }

        public MockInGameClientInteractor(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            _messages = new List<InGameServerMessage>();
        }

        public Task SendMessage(InGameServerMessage message)
        {
            _messages.Add(message);
            return Task.CompletedTask;
        }

        public IChannelToClient<TNew> AsNewHandler<TNew>() where TNew : ServerMessage
        {
            throw new NotSupportedException();
        }
    }
}