using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcService;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new MainController.MainControllerClient(channel);
            using var stream = client.Connect();
        }
    }
}