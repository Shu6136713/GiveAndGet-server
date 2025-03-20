using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;
using Services.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class ChatHub : Hub
    {
        // שמירת חיבורים בזיכרון - לא ב-DB
        private static ConcurrentDictionary<int, List<string>> connections = new();

        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task Join(int userId, int exchangeId)
        {
            if (!connections.ContainsKey(userId))
                connections[userId] = new List<string>();

            connections[userId].Add(Context.ConnectionId);

            // צירוף לקבוצה לפי ExchangeId מאפשר שידור ממוקד
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Exchange_{exchangeId}");
        }

        public async Task SendMessage(int exchangeId, int fromUserId, string messageText)
        {
            // שמירת ההודעה ב-DB
            await _chatService.SaveMessageAsync(exchangeId, fromUserId, messageText);

            // שידור לכל מי שמחובר לאותה עסקה
            await Clients.Group($"Exchange_{exchangeId}")
                .SendAsync("ReceiveMessage", fromUserId, messageText, DateTime.Now);
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var user = connections.FirstOrDefault(kvp => kvp.Value.Contains(Context.ConnectionId));
            if (user.Key != 0)
            {
                user.Value.Remove(Context.ConnectionId);
                if (user.Value.Count == 0)
                    connections.TryRemove(user.Key, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
