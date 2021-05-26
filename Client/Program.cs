using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using Shared.Protos;
using Greeting = Shared.Protos.Greeting;

namespace Client
{
    class Program
    {
        static async Task Main2(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = channel.CreateGrpcService<IMainServiceContract>();
            var requestStream = Channel.CreateUnbounded<ClientMessage>();


            var stream = client.Connect(requestStream.Reader.ReadAllAsync());
            if (stream is null) return;

            await requestStream.Writer.WriteAsync(new Greeting()
            {
                Name = await Console.In.ReadLineAsync()
            });

            var reader = stream.AsChannelReader();

            var next = await reader.ReadAsync();

            if (next is GreetingSuccessful greetingSuccessful)
            {
                await Console.Out.WriteLineAsync(greetingSuccessful.Guid.ToString());
            }

            var command = await Console.In.ReadLineAsync();
            Guid id;
            if (command == "Create")
            {
                await requestStream.Writer.WriteAsync(new CreateLobby()
                {
                    Configuration = new ClickGameConfiguration()
                    {
                        ClicksToWin = 15,
                        IncrementValue = 3
                    }
                });

                next = await reader.ReadAsync();

                if (next is not LobbyCreated gameCreatedEvent)
                {
                    await Finish(requestStream);
                    return;
                }

                id = gameCreatedEvent.Guid;
            }
            else if (command == "Join")
            {
                id = Guid.Parse((ReadOnlySpan<char>) await Console.In.ReadLineAsync());
            }
            else
            {
                await Console.Out.WriteLineAsync("Wrong command");
                return;
            }


            await Console.Out.WriteLineAsync(id.ToString());

            await requestStream.Writer.WriteAsync(new JoinLobby()
            {
                Id = id
            });

            next = await reader.ReadAsync();

            if (next is not JoinedLobby joinedGame)
            {
                await Finish(requestStream);
                return;
            }

            await Console.Out.WriteLineAsync(Enum.GetName(joinedGame.Type));
            await Console.Out.WriteLineAsync(string.Join(", ", joinedGame.AvailableRoles));
            await Console.Out.WriteLineAsync(joinedGame.IsHost.ToString());

            if (joinedGame.IsHost)
            {
                while (true)
                {
                    var cmd = await Console.In.ReadLineAsync();
                    if (cmd is null) continue;
                    if (cmd.StartsWith("Role"))
                    {
                        var splitted = cmd.Split(" ");
                        var playToSwitchRoleId = splitted[1];
                        var indexStr = splitted[2];
                        var targetRoleIndex = int.Parse(indexStr);
                        await requestStream.Writer.WriteAsync(new ChangeRole()
                        {
                            PlayerId = Guid.Parse(playToSwitchRoleId),
                            NewRole = joinedGame.AvailableRoles[targetRoleIndex]
                        });
                        continue;
                    }

                    if (cmd == "Start")
                    {
                        await requestStream.Writer.WriteAsync(new StartGame());
                        next = await reader.ReadAsync();
                        while (next is LobbyUpdate)
                        {
                            next = await reader.ReadAsync();
                        }

                        if (next is not GameCreated)
                        {
                            await Console.Out.WriteLineAsync(next.ToString());
                            await Console.Out.WriteLineAsync("Something went wrong");
                            return;
                        }

                        break;
                    }
                }
            }
            else
            {
                await DisplayLobbyUpdates(reader);
            }

            await BeginClickerGame(reader, requestStream.Writer);

            await Finish(requestStream);
        }

        private static async Task BeginClickerGame(ChannelReader<ServerMessage> reader,
            ChannelWriter<ClientMessage> requestStreamWriter)
        {
            var finished = false;
            Parallel.Invoke(async () =>
            {
                while (true)
                {
                    var next = await reader.ReadAsync();
                    if (next is ClickerNewValueEvent e)
                    {
                        await Console.Out.WriteLineAsync(e.Value.ToString());
                    }
            
                    if (next is GameFinished)
                    {
                        await Console.Out.WriteLineAsync("Game finished");
                        finished = true;
                        break;
                    }
                }
            });
            while (!finished)
            {
                await Console.In.ReadLineAsync();
                await Console.Out.WriteLineAsync("Sending increment");
                await requestStreamWriter.WriteAsync(new ClickerIncrementEvent());
                await Task.Delay(1000);
            }
        }

        private static async Task DisplayLobbyUpdates(ChannelReader<ServerMessage>? reader)
        {
            ServerMessage? next;
            if (reader is null) return;
            while (true)
            {
                next = await reader.ReadAsync();
                if (next is LobbyUpdate n)
                {
                    await Console.Out.WriteLineAsync(string.Join("\n",
                        n.Players.Select(p => $"{p.Name} {p.Id} {p.Role}")));
                    continue;
                }

                if (next is GameCreated)
                {
                    await Console.Out.WriteLineAsync("Game started");
                    break;
                }
            }
        }

        private static async Task Finish(Channel<ClientMessage> requestStream)
        {
            requestStream.Writer.Complete();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}