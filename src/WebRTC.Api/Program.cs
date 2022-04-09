using Vpiska.WebSocket;
using WebRTC.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddWebSocketExceptionHandler<ExceptionHandler>();
builder.Services.AddVSocket<SocketListener>("/websockets/signaling", Array.Empty<string>(), Array.Empty<string>());
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.UseWebSockets();
app.UseVSocket();

app.Run();
