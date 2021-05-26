using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Client_lib;
using ProtoBuf.WellKnownTypes;
using Shared.Protos;
using Shared.Protos.HatSharedClasses;

namespace Client
{
    public class UsingLibWithHat
    {
        private static Client_lib.Client _client;
        private static Dictionary<Guid, string> _clients = new();
        private static Lobby? _lobby;

        public static async Task Main(string[] args)
        {
            var name = Console.ReadLine()!;
            _client = new Client_lib.Client(name);
            await _client.ConnectToServer("127.0.0.1:5000");

            var command = Console.ReadLine()!;
            Guid id;
            switch (command)
            {
                case "Create hat":
                    id = await _client.CreateGame(
                        new HatConfiguration
                        {
                            TimeToExplain = new TimeSpan(0, 0, 50),
                            HatGameModeConfiguration = new HatCircleChoosingModeConfiguration(),
                            WordsToBeWritten = 3,
                        }
                    );
                    break;
                case "Create clicker":
                    id = await _client.CreateGame(new ClickGameConfiguration()
                    {
                        ClicksToWin = 15,
                        IncrementValue = 3,
                    });
                    break;
                case "Join":
                    id = Guid.Parse(Console.ReadLine()!);
                    break;
                default:
                    await AsyncPrint("Wrong command");
                    return;
            }

            await AsyncPrint(id.ToString());

            _lobby = await _client.JoinGame(id);

            await AsyncPrint(Enum.GetName(_lobby.Type)!);
            await AsyncPrint(string.Join(", ", _lobby.AvailableRoles));
            await AsyncPrint("U r host - " + _lobby.AmHost);
            _lobby.LobbyUpdate += async () =>
            {
                _clients = _lobby.LastLobbyStatus!.ToDictionary(x => x.Id, x => x.Name);
                await AsyncPrint(string.Join("\n", _lobby.LastLobbyStatus!
                    .Select(p => $"{p.Name} {p.Id} {p.Role}"))
                );
            };

            _lobby.GameFinished += () => FinishGame(_lobby);

            _lobby.HandleGameStart += game =>
            {
                if (_lobby.Type == GameTypes.TheHat)
                    HandleGameStart(game, _lobby);
                else HandleClickerGameStart(game, _lobby);
            };

            try
            {
                await Task.WhenAll(HandleBeingHost(_lobby), _lobby.StartProcessing()).ConfigureAwait(false);
            }
            catch (OperationCanceledException _)
            {
            }
        }

        private static void FinishGame(Lobby lobby)
        {
            lobby.Disconnect();
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
                        if (_clients.Count < 2)
                        {
                            await AsyncPrint("not enough players");
                            continue;
                        }

                        await AsyncPrint("Got start command");
                        await lobby.StartGame();
                        break;
                    }
                }
            }
        }

        private static async void HandleGameStart(Game game, Lobby lobby)
        {
            await Console.Out.WriteLineAsync("Game started");
            game.MessageFromServer += async msg => await TreatMessageFromServer(msg);
            while (lobby.GameIsGoing)
            {
                var cmd = await Task.Run(async () => await Console.In.ReadLineAsync());
                if (cmd!.StartsWith("addWords"))
                {
                    var words = cmd.Split(" ").Skip(1);
                    await game.SendGameEvent(new AddWords
                    {
                        Value = words.ToList()
                    });
                }
                else if (cmd == "ready")
                    await game.SendGameEvent(new ClientIsReady());
                else if (cmd == "+")
                    await game.SendGameEvent(new GuessRight());
                else
                    await AsyncPrint("Unknown command from player " + cmd);
            }
        }

        private static async Task TreatMessageFromServer(ServerMessage message)
        {
            switch (message)
            {
                case HatAnnounceNextPair announceNextPair:
                    await AsyncPrint(_clients[announceNextPair.Explainer] + " explains to " +
                                     _clients[announceNextPair.Understander]);
                    return;
                case HatPointsUpdated pointsUpdated:
                    await AsyncPrint(String.Join("\n", pointsUpdated.GuidToPoints
                        .Select(kvp => _clients[kvp.Key] + ": " + kvp.Value)));
                    return;
                case HatExplanationStarted:
                    await AsyncPrint("you may start explanation");
                    return;
                case HatWordToGuess wordToGuess:
                    await AsyncPrint("You should explain the  following word ---- " + wordToGuess.Value);
                    return;
                case HatFinishMessage:
                    await AsyncPrint("Exiting by finish message...");
                    FinishGame(_lobby!);
                    return;
                case HatTimeIsUp:
                    await AsyncPrint("Your time is UP");
                    return;
                case HatStartGame:
                    await AsyncPrint("The game is on");
                    return;
                default:
                    await AsyncPrint("Unknown command: " + message);
                    return;
            }
        }

        private static Task AsyncPrint(string s) => Console.Out.WriteLineAsync("->> " + s);

        private static async void HandleClickerGameStart(Game game, Lobby lobby)
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