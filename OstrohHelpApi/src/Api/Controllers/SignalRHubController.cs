using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// SignalR Hub Documentation
/// Real-time chat functionality
[ApiController]
[Route("api/[controller]")]
public class SignalRHubController : ControllerBase
{
    /// SignalR Chat Hub Information
    /// WebSocket connection to: /hubs/chat
    /// Required JWT Token in query string or header
    /// 
    /// Available Methods:
    /// 
    /// 1. JoinConsultation(consultationId: string)
    ///    - Join a consultation chat room
    /// 
    /// 2. LeaveConsultation(consultationId: string)
    ///    - Leave a consultation chat room
    /// 
    /// 3. SendMessage(consultationId: string, text: string, attachments?: list)
    ///    - Send a message to consultation
    ///    - receiverId is determined automatically
    /// 
    /// 4. MarkAsRead(messageId: string, consultationId: string)
    ///    - Mark a message as read
    /// 
    /// 5. Typing(consultationId: string)
    ///    - Show typing indicator
    /// 
    /// 6. StopTyping(consultationId: string)
    ///    - Hide typing indicator
    /// 
    /// 7. DeleteMessage(messageId: string, consultationId: string)
    ///    - Delete a message
    /// 
    /// Events to listen:
    /// - ReceiveMessage: New message received
    /// - UserJoined: User joined consultation
    /// - UserLeft: User left consultation
    /// - UserTyping: User is typing
    /// - UserStoppedTyping: User stopped typing
    /// - MessageRead: Message was read
    /// - MessageDeleted: Message was deleted
    /// - Error: Error occurred
    /// <response code="200">SignalR Hub is available</response>
    [HttpGet("info")]
    [ProducesResponseType(typeof(object), 200)]
    public IActionResult GetHubInfo()
    {
        return Ok(new
        {
            hubName = "ChatHub",
            url = "/hubs/chat",
            protocol = "signalr",
            requiresAuthentication = true,
            description = "Real-time chat for consultations",
            methods = new[]
            {
                "JoinConsultation",
                "LeaveConsultation",
                "SendMessage",
                "MarkAsRead",
                "Typing",
                "StopTyping",
                "DeleteMessage"
            },
            events = new[]
            {
                "ReceiveMessage",
                "UserJoined",
                "UserLeft",
                "UserTyping",
                "UserStoppedTyping",
                "MessageRead",
                "MessageDeleted",
                "Error"
            },
            exampleConnection = new
            {
                url = "wss://localhost:7000/hubs/chat",
                headers = new
                {
                    Authorization = "Bearer YOUR_JWT_TOKEN"
                }
            }
        });
    }

    /// <summary>
    /// Connection example for JavaScript/TypeScript
    /// </summary>
    /// <response code="200">JavaScript example code</response>
    [HttpGet("example-js")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult GetJavaScriptExample()
    {
        var example = @"
// JavaScript/TypeScript Example
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/chat', {
        accessTokenFactory: () => 'YOUR_JWT_TOKEN'
    })
    .withAutomaticReconnect()
    .build();

connection.on('ReceiveMessage', (message) => {
    console.log('New message:', message);
});

await connection.start();
await connection.invoke('JoinConsultation', 'consultation-id');
await connection.invoke('SendMessage', 'consultation-id', 'Hello!', []);
        ";
        return Ok(new { example });
    }

    /// Connection example for Flutter
    /// <response code="200">Flutter example code</response>
    [HttpGet("example-flutter")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult GetFlutterExample()
    {
        var example = @"
// Flutter Example
import 'package:signalr_netcore/signalr_client.dart';

final httpConnectionOptions = HttpConnectionOptions(
    accessTokenFactory: () async => 'YOUR_JWT_TOKEN',
    transport: HttpTransportType.WebSockets,
);

final hubConnection = HubConnectionBuilder()
    .withUrl('https://localhost:7000/hubs/chat', options: httpConnectionOptions)
    .build();

hubConnection.on('ReceiveMessage', (arguments) {
    print('New message: $arguments');
});

await hubConnection.start();
await hubConnection.invoke('JoinConsultation', args: ['consultation-id']);
await hubConnection.invoke('SendMessage', args: ['consultation-id', 'Hello!', []]);
        ";
        return Ok(new { example });
    }

    /// Connection example for C#/.NET
    /// <response code="200">C# example code</response>
    [HttpGet("example-csharp")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult GetCSharpExample()
    {
        var example = @"
// C#/.NET Example
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl(""https://localhost:7000/hubs/chat"", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(""YOUR_JWT_TOKEN"");
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<MessageDto>(""ReceiveMessage"", message =>
{
    Console.WriteLine($""New message: {message.Text}"");
});

await connection.StartAsync();
await connection.InvokeAsync(""JoinConsultation"", ""consultation-id"");
await connection.InvokeAsync(""SendMessage"", ""consultation-id"", ""Hello!"", new List<object>());
        ";
        return Ok(new { example });
    }
}
