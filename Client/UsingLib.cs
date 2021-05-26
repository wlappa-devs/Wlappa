using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Client_lib;
using Shared.Protos;

namespace Client
{
    public class UsingLib
    {
        static async Task Main(string[] args)
        {
            var name = Console.ReadLine()!;
            var client = new Client_lib.Client(name);
            await client.ConnectToServer("localhost:5000");

            var command = Console.ReadLine()!;
            Guid id;
            switch (command)
            {
                case "Create":
                    id = await client.CreateGame(new ClickGameConfiguration()
                    {
                        ClicksToWin = 15,
                        IncrementValue = 3,
                    });
                    break;
                case "Join":
                    id = Guid.Parse(Console.ReadLine()!);
                    break;
                default:
                    await Console.Out.WriteLineAsync("Wrong command");
                    return;
            }

            await Console.Out.WriteLineAsync(id.ToString());

            var lobby = await client.JoinGame(id);

            await Console.Out.WriteLineAsync(Enum.GetName(lobby.Type));
            await Console.Out.WriteLineAsync(string.Join(", ", lobby.AvailableRoles));
            await Console.Out.WriteLineAsync(lobby.AmHost.ToString());
            lobby.LobbyUpdate += async () =>
            {
                await Console.Out.WriteLineAsync(string.Join("\n", lobby.LastLobbyStatus!
                    .Select(p => $"{p.Name} {p.Id} {p.Role}"))
                );
            };

            lobby.GameFinished += () => FinishGame(lobby);

            lobby.HandleGameStart += game => HandleGameStart(game, lobby);

            try
            {
                await Task.WhenAll(HandleBeingHost(lobby), lobby.StartProcessing()).ConfigureAwait(false);
            }
            catch (OperationCanceledException _)
            {
            }
        }

        private static async Task HandleBeingHost(Lobby lobby)
        {
            if (lobby.AmHost)
            {
                while (true)
                {
                    var cmd = await Task.Run(async () => await Console.In.ReadLineAsync());
                    if (cmd!.StartsWith("Role"))
                    {
                        var splitted = cmd.Split(" ");
                        var playToSwitchRoleId = splitted[1];
                        var indexStr = splitted[2];
                        var targetRoleIndex = int.Parse(indexStr);
                        await lobby.ChangeRole(Guid.Parse(playToSwitchRoleId), lobby.AvailableRoles[targetRoleIndex]);
                        continue;
                    }

                    if (cmd! == "Start")
                    {
                        await Console.Out.WriteLineAsync("Got start command");
                        await lobby.StartGame();
                        break;
                    }
                }
            }
        }

        private static void FinishGame(Lobby lobby)
        {
            lobby.Disconnect();
        }

        private static async void HandleGameStart(Game game, Lobby lobby)
        {
            await Console.Out.WriteLineAsync("Game started");
            game.MessageFromServer += async message =>
            {
                if (message is ClickerNewValueEvent newValueEvent)
                    await Console.Out.WriteLineAsync(newValueEvent.Value.ToString());
            };
            while (lobby.GameIsGoing)
            {
                await Task.Run(async () => await Console.In.ReadLineAsync());
                await game.SendGameEvent(new ClickerIncrementEvent());
            }
        }
    }
}