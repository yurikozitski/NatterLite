using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NatterLite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NatterLite.Filters;

namespace NatterLite.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(IsBannedFilter))]
    public class ChatController : Controller
    {
        private readonly ApplicationContext db;
        private readonly UserManager<User> userManager;
        public ChatController(ApplicationContext context,
            UserManager<User> _userManager)
        {
            db = context;
            userManager = _userManager;
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(string userId)
        {
            //var currentUserFromManager = await userManager.GetUserAsync(this.User);
            var currentUser = await db.Users.Include(u => u.Chats)
                .ThenInclude(c => c.Users)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var companionUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser.Chats.Count != 0)
            {
                foreach (Chat currentUserChat in currentUser.Chats)
                {
                    if (currentUserChat.Users.Count == 2 &&
                        currentUserChat.Users.Any(u => u.Id == companionUser.Id))
                    {
                        return RedirectToAction("ChatMenu", "Chat");
                    }
                }
            }
            Chat chat = new Chat();
            chat.CreatorUserName = currentUser.UserName;
            chat.CreationTime = DateTime.Now;
            chat.LastVisitedBy = User.Identity.Name + "=" + new DateTime().ToString() + ","
                + companionUser.UserName + "=" + new DateTime().ToString() + ",";
            chat.Users.Add(currentUser);
            chat.Users.Add(companionUser);
            await db.Chats.AddAsync(chat);
            await db.SaveChangesAsync();
            return RedirectToAction("ChatMenu", "Chat");
        }
        [HttpGet]
        public IActionResult ChatMenu()
        {          
            return View("ChatMenu");
        }
        [HttpPost]
        public async Task<IActionResult> GetChats(string currentchatId = null)
        {
            var currentUser = await db.Users.Include(u=>u.Chats)
                .ThenInclude(c=>c.Users)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var currentUserWithChatMessages = await db.Users.Include(u => u.Chats)
                .ThenInclude(c => c.Messages)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var currentUserWithblacklist = await db.Users.Include(u => u.BlackList)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            List<Chat> currentUserChats = new List<Chat>();
            currentUserChats= currentUser.Chats;
            List<ChatViewModel> chatViewModelList=new List<ChatViewModel>();
            if (currentUserChats.Count != 0)
            {
                foreach (Chat chat in currentUserChats)
                {
                    if (chat.CreatorUserName == User.Identity.Name||chat.Messages.Count!=0)
                    {
                        ChatViewModel chatViewModel = new ChatViewModel();
                        chatViewModel.ChatId = chat.Id.ToString();

                        if (chat.Messages.Count != 0)
                        {
                            chatViewModel.TimeForCompare= chat.Messages[chat.Messages.Count - 1].Time;
                            chatViewModel.LastMessageTime = chat.Messages[chat.Messages.Count - 1].Time;
                            string LastMessageText = chat.Messages[chat.Messages.Count - 1].Text;
                            if (LastMessageText.Length <= 80)
                            {
                                chatViewModel.LastMessageText = LastMessageText;
                            }
                            else
                            {
                                chatViewModel.LastMessageText = LastMessageText.Substring(0,77)+"...";
                            }
                            if (chat.LastVisitedBy != null)
                            {
                                DateTime timeToCompare = new DateTime();
                                char[] separators = new char[] { ',', '=' };
                                string[] subs = chat.LastVisitedBy.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < subs.Length; i += 2)
                                {
                                    if (subs[i] == User.Identity.Name)
                                    {
                                        timeToCompare =DateTime.Parse(subs[i + 1]);
                                    }
                                }
                                if (currentchatId == chatViewModel.ChatId)
                                {
                                    chatViewModel.UnreadMessagesCount = 0.ToString();
                                }
                                else
                                {
                                    int mesCount = chat.Messages
                                        .Where(m => m.Time > timeToCompare)
                                        .ToList().Count;
                                    if (mesCount < 100)
                                    {
                                        chatViewModel.UnreadMessagesCount = mesCount.ToString();
                                    }
                                    else
                                    {
                                        chatViewModel.UnreadMessagesCount = "99+";
                                    }
                                }
                            }
                            else
                            {
                                chatViewModel.UnreadMessagesCount = 0.ToString();
                            }
                        }
                        else
                        {
                            chatViewModel.TimeForCompare = chat.CreationTime;
                            chatViewModel.LastMessageTime =new DateTime();
                            chatViewModel.LastMessageText = " ";
                            chatViewModel.UnreadMessagesCount = 0.ToString();
                        }
                        
                        chatViewModel.CompanionUserName = chat.Users.Find(u => u.Id != currentUser.Id).FullName;
                        chatViewModel.CompanionUserProfilePicture = chat.Users.Find(u => u.Id != currentUser.Id).ProfilePicture;
                        chatViewModelList.Add(chatViewModel);
                    }
                }
                chatViewModelList.Sort((This,Next)=> This.TimeForCompare< Next.TimeForCompare?1:-1);
            }
            return PartialView("ChatListPartial", chatViewModelList);
        }
        [HttpPost]
        public async Task<IActionResult> WriteLastVisitedTimeForChat(string chatId)
        {
            Chat chat = await db.Chats.FirstOrDefaultAsync(c=>c.Id.ToString()==chatId);
            if (chat.LastVisitedBy != null)
            {
                char[] separators = new char[] { ',', '=' };
                string[] subs = chat.LastVisitedBy.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                bool isTheSame = false;
                for (int i = 0; i < subs.Length; i += 2)
                {
                    if (subs[i] == User.Identity.Name)
                    {
                        subs[i + 1] = DateTime.Now.ToString();
                        isTheSame = true;
                    }
                }
                for (int i = 0; i < subs.Length; i++)
                {
                    if (i == 0 || i % 2 == 0)
                    {
                        subs[i] += "=";
                    }
                    else
                    {
                        subs[i] += ",";
                    }
                }
                if (isTheSame)
                {
                    chat.LastVisitedBy = String.Concat(subs);
                    await db.SaveChangesAsync();
                    return new EmptyResult();
                }
            }
            chat.LastVisitedBy += User.Identity.Name + "=" + DateTime.Now.ToString() + ",";
            await db.SaveChangesAsync();
            return new EmptyResult();
        }
            [HttpPost]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            Chat chatWithMessages = await db.Chats.Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id.ToString() == chatId);
            Chat chatWithUsers = await db.Chats.Include(c => c.Users).ThenInclude(u => u.BlackList).FirstOrDefaultAsync(c => c.Id.ToString() == chatId);

            User currentUser = chatWithUsers.Users.Find(u => u.UserName == User.Identity.Name);
            User companionUser = chatWithUsers.Users.Find(u => u.UserName != User.Identity.Name);

            MessagesViewModel mvm = new MessagesViewModel();
            mvm.CompanionUserIdentityName = companionUser.UserName;
            mvm.CompanionUserName = companionUser.FullName;
            mvm.CompanionUserProfilePicture = companionUser.ProfilePicture;
            mvm.CompanionUserStatus = companionUser.Status;

            if (currentUser.BlackList.Exists(u => u.UserName == companionUser.UserName)) mvm.DidCurrentUserAddedCompanionUserToBlackList = true;
            if (companionUser.BlackList.Exists(u => u.UserName == currentUser.UserName)) mvm.DidCompanionUserAddedCurrentUserToBlackList = true;

            if (chatWithMessages.Messages.Count != 0)
            {
                foreach(Message mes in chatWithMessages.Messages)
                {
                    mvm.Messages.Add(mes);
                }
                mvm.Messages.Reverse();
            }
            return PartialView("MessagesViewPartial",mvm);
        }
    }
}
