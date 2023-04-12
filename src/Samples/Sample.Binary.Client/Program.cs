using Horse.WebSocket.Client;
using Horse.WebSocket.Protocol.Providers;
using Microsoft.Extensions.DependencyInjection;
using Sample.Binary.Client;

IServiceCollection services = new ServiceCollection();

HorseWebSocket client = new HorseWebSocket();
client.UseCustomModelProvider<BinaryModelProvider>();

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

while (true)
{
    Console.WriteLine("press enter to send");
    await Task.Delay(5000);
    await client.SendAsync(model);
    await Task.Delay(60000);
}