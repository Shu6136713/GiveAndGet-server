﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services.Dtos;
using Services.Interfaces;
using Services.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        // שמירת חיבורים בזיכרון - לא ב-DB
        private static ConcurrentDictionary<int, List<string>> connections = new();

        private readonly IChatService _chatService;
        private readonly IExchangeForChat _exchangeService;

        public ChatHub(IChatService chatService, IExchangeForChat exchangeService)
        {
            _chatService = chatService;
            _exchangeService = exchangeService;
        }

        public async Task Join(int userId, int exchangeId)
        {
            // קודם כל, ודא שהמשתמש שייך לעסקה
            bool isUserInExchange = await _exchangeService.IsUserInExchangeAsync(userId, exchangeId);

            if (!isUserInExchange)
            {
                // אם המשתמש לא שייך לעסקה, דחה את הצטרפותו
                await Clients.Caller.SendAsync("ErrorMessage", "You are not part of this exchange!");
                return;
            }

            // אם המשתמש כן שייך לעסקה, הוסף אותו לחיבור
            if (!connections.ContainsKey(userId))
                connections[userId] = new List<string>();

            connections[userId].Add(Context.ConnectionId);

            // צירוף לקבוצה לפי ExchangeId מאפשר שידור ממוקד
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Exchange_{exchangeId}");
        }

        // מתודה לשליחת הודעה בקבוצת הצ'אט
        public async Task SendMessage(int exchangeId, int fromUserId, string messageText)
        {
            // שמירת ההודעה ב-DB
            await _chatService.SaveMessageAsync(exchangeId, fromUserId, messageText);

            // שידור לכל מי שמחובר לאותה עסקה
            await Clients.Group($"Exchange_{exchangeId}")
                .SendAsync("ReceiveMessage", fromUserId, messageText, DateTime.Now);
        }

        // מתודה לניהול ניתוק המשתמש
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
