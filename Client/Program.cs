using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcService;
using ProtoBuf.Grpc.Client;
using Server.Routing;

namespace Client
{
    class Program
    {
        private static async IAsyncEnumerable<ClientMessageProtobufNet> ReadAllLines()
        {
            while (true)
            {
                yield return new ClientMessageProtobufNet()
                {
                    Data = await Console.In.ReadLineAsync()
                };
            }
        }

        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = channel.CreateGrpcService<IMainServiceContract>();
            
            var stream = client.Connect(ReadAllLines());
            if (stream is null) return;
            await foreach (var e in stream)
            {
                Console.WriteLine(e.Data);
            }
        }
    }
}