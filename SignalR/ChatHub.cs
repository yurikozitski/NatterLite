using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using NatterLite.Models;
using Microsoft.EntityFrameworkCore;

namespace NatterLite.SignalR
{
    public class ChatHub : Hub
    {
        private readonly ApplicationContext db;
        public ChatHub(ApplicationContext context)
        {
            db = context;
        }
        public async Task Send(string message,string reciever,string chatId)
        {
            if (!db.Users.Include(u => u.BlackList)
                .FirstOrDefault(u => u.UserName == reciever)
                .BlackList
                .Exists(u => u.UserName == Context.UserIdentifier))
            {
                Message mes = new Message();
                mes.SenderUserName = Context.User.Identity.Name;
                mes.Text = message;
                mes.Time = DateTime.Now;

                Chat currentChat = db.Chats.FirstOrDefault(c => c.Id.ToString() == chatId);
                currentChat.Messages.Add(mes);

                await Clients.User(Context.UserIdentifier).SendAsync("Receive", message, chatId, "fromCurrentUser");
                await Clients.User(reciever).SendAsync("Receive", message, chatId, "NotfromCurrentUser");

                await db.SaveChangesAsync();
            }
        }
    }
}
