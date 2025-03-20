//using Microsoft.AspNetCore.SignalR;
//using System.Collections.Concurrent;
//using System.Text.RegularExpressions;

//namespace WebAPI.Hubs
//{
//    public class ChatHub : Hub
//    {
//        // שמירת כל החיבורים החיים בזיכרון
//        private static ConcurrentDictionary<int, List<string>> connections = new();

//        //private readonly ChatService _chatService;

//        public ChatHub(ChatService chatService)
//        {
//            _chatService = chatService;
//        }

//        // קריאה מהלקוח להתחבר לצ'אט של עסקה מסוימת
//        public async Task Join(int userId, int exchangeId)
//        {
//            // שומר את החיבור של המשתמש
//            if (!connections.ContainsKey(userId))
//                connections[userId] = new List<string>();

//            connections[userId].Add(Context.ConnectionId);

//            // מצרף את החיבור הזה לקבוצת ה-SignalR לפי ExchangeId
//            await Groups.AddToGroupAsync(Context.ConnectionId, $"Exchange_{exchangeId}");
//        }

//        // קבלת הודעה מהלקוח ושליחה לכל הצדדים בעסקה
//        public async Task SendMessage(int exchangeId, int fromUserId, string messageText)
//        {
//            // שמירה ל-DB דרך ה-ChatService
//            await _chatService.SaveMessageAsync(exchangeId, fromUserId, messageText);

//            // שידור לכל חברי הקבוצה של ה-Exchange הזה
//            await Clients.Group($"Exchange_{exchangeId}")
//                .SendAsync("ReceiveMessage", fromUserId, messageText, DateTime.Now);
//        }

//        // ניתוק חיבור - מסיר מהזיכרון
//        public override async Task OnDisconnectedAsync(Exception? exception)
//        {
//            // מוצא את המשתמש לפי ה-ConnectionId שלו
//            var user = connections.FirstOrDefault(kvp => kvp.Value.Contains(Context.ConnectionId));

//            if (user.Key != 0)
//            {
//                // מסיר את החיבור מהרשימה של המשתמש
//                user.Value.Remove(Context.ConnectionId);
//                if (user.Value.Count == 0)
//                    connections.TryRemove(user.Key, out _);
//            }

//            await base.OnDisconnectedAsync(exception);
//        }
//    }

//}
