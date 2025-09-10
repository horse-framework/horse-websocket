using Horse.WebSocket.Client;
using Microsoft.Extensions.DependencyInjection;
using Sample.Binary.Client;

IServiceCollection services = new ServiceCollection();

HorseWebSocket client = new HorseWebSocket();
client.UsePipeModelProvider();
client.UseBinaryModelProvider();

client.UseServices(services);
client.AddHandlersTransient(typeof(Program));
client.RemoteHost = "ws://localhost:888";
client.UseProvider(services.BuildServiceProvider());

await client.ConnectAsync();

TestModel model = new TestModel
{
    Item1 = 1234,
    Item2 = true,
    Item3 = "mehmet",
    Item4 = 43543254
};

bool binary = true;
while (true)
{
    var line = Console.ReadLine()!;
    model.Item3 = line;
    await client.SendAsync(model, binary);
    binary = !binary;
}