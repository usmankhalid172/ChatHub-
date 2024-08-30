using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatHub
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserRooms = new();
        public async Task SendMessage(string room, string user, string message)
        {
            if (UserRooms.TryGetValue(Context.ConnectionId, out var userRoom) && userRoom == room)
            {
                Console.WriteLine($"Room: {room}, User: {user}, Message: {message}");
                await Clients.Caller.SendAsync("ShowMessage", user, message);
                await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
            }
            else
            {
                await Clients.Caller.SendAsync("ShowMessage", user, message);
            }
        }

        public async Task SendMessageToUser(string user, string message)
        {
            await Clients.Caller.SendAsync("ShowMessage", user, message);
        }

        public async Task JoinRoom(string room)
        {
            UserRooms[Context.ConnectionId] = room;
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("ShowMessage", $"{Context.ConnectionId} has joined the room {room}.");
            Console.WriteLine($"{Context.ConnectionId} has joined the room {room}.");
        }

        public async Task LeaveRoom(string room)
        {
            if (UserRooms.TryRemove(Context.ConnectionId, out var userRoom) && userRoom == room)
            {

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
                await Clients.Group(room).SendAsync("ShowMessage", $"{Context.ConnectionId} has left the room {room}.");
                Console.WriteLine($"{Context.ConnectionId} has left the room {room}.");
            }

            else
            {
                await Clients.Caller.SendAsync($"ShowMessage", "you are not in the room");
                Console.WriteLine($"{Context.ConnectionId} tried to leave the room {room} but was not part of it.");
            }
        }
        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);

            Console.WriteLine($"Notification sent to user {userId}: {message}");
        }

        // Method for sending a push notification to a group of users
        public async Task SendNotificationToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Notification sent to group {groupName}: {message}");
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (UserRooms.TryRemove(Context.ConnectionId, out var userRoom))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userRoom);
                await Clients.Group(userRoom).SendAsync("ShowMessage", $"{Context.ConnectionId} has left the room {userRoom}.");
                Console.WriteLine($"{Context.ConnectionId} has left the room {userRoom}.");
            }
            await base.OnDisconnectedAsync(exception);
            if (exception != null)
            {
                Console.WriteLine($"Connection {Context.ConnectionId} disconnected with exception: {exception.Message}");
            }
        }
    }
}
