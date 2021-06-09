using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Domain.Games.Meta;
using Shared.Protos;

namespace TestServer.GameTests
{
    public class MockInGameClientInteractor: IInGameClientInteractor
    {
        public IReadOnlyList<InGameServerMessage> Messages => _messages;

        private readonly List<InGameServerMessage> _messages;
        public Guid Id { get; }
        public string? Name { get; }

        public MockInGameClientInteractor(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            _messages = new List<InGameServerMessage>();
        }
        
        public Task HandleInGameMessage(InGameServerMessage message)
        {
            _messages.Add(message);
            return Task.CompletedTask;
        }
    }
}