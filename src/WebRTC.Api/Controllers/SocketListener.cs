using Vpiska.WebSocket;

namespace WebRTC.Api.Controllers;

public sealed class SocketListener : IWebSocketListener
{
    private readonly IWebSocketInteracting<SocketListener> _socketInteracting;

    private static string Offer { get; set; }
    private static Guid OfferConnection { get; set; }

    private static List<string> OffersIce { get; } = new();

    public SocketListener(IWebSocketInteracting<SocketListener> socketInteracting)
    {
        _socketInteracting = socketInteracting;
    }

    public Task OnConnect(WebSocketContext socketContext) => _socketInteracting.SendRawMessage(
        socketContext.ConnectionId, "connection",
        socketContext.ConnectionId.ToString());

    public Task OnDisconnect(WebSocketContext socketContext)
    {
        if (socketContext.ConnectionId == OfferConnection)
        {
            Offer = null;
            OfferConnection = Guid.Empty;
            OffersIce.Clear();
        }
        return Task.CompletedTask;
    }

    public async Task Receive(WebSocketContext socketContext, string route, string message)
    {
        switch (route)
        {
            case "offerOwner":
            {
                Offer = message;
                OfferConnection = socketContext.ConnectionId;
                return;
            }

            case "sdpCheck":
            {
                var tasks = socketContext.Connections.Length > 1
                    ? socketContext.Connections.Except(new[] { OfferConnection }).Select(connection =>
                        _socketInteracting.SendRawMessage(connection, "sdp", Offer))
                    : socketContext.Connections.Select(connection =>
                        _socketInteracting.SendRawMessage(connection, "sdp", Offer));
                await Task.WhenAll(tasks);
                return;
            }

            case "answer":
            {
                await _socketInteracting.SendRawMessage(OfferConnection, "answer", message);
                
                if (socketContext.Connections.Length == 1)
                {
                    foreach (var ice in OffersIce)
                    {
                        await _socketInteracting.SendRawMessage(socketContext.ConnectionId, "offer-ice", ice);
                    }
                    return;
                }

                foreach (var connection in socketContext.Connections.Except(new[] { OfferConnection }))
                {
                    foreach (var ice in OffersIce)
                    {
                        await _socketInteracting.SendRawMessage(connection, "offer-ice", ice);
                    }
                }
                return;
            }

            case "client-ice":
            {
                await _socketInteracting.SendRawMessage(OfferConnection, "client-ice", message);
                return;
            }

            case "offer-ice":
            {
                OffersIce.Add(message);
                /*if (socketContext.Connections.Length == 1)
                {
                    await _socketInteracting.SendRawMessage(socketContext.ConnectionId, "offer-ice", message);
                    return;
                }

                var tasks = socketContext.Connections.Except(new[] { OfferConnection }).Select(connection =>
                    _socketInteracting.SendRawMessage(connection, "offer-ice", message));
                await Task.WhenAll(tasks);*/
                return;
            }
        }
    }
}

public sealed class ExceptionHandler : IWebSocketExceptionHandler
{
    public void Handle(WebSocketContext socketContext, Exception exception)
    {
        Console.Write(exception);
    }
}