using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyHub.Hubs
{
    public class ChatHub2 : Hub<IClient>
    {
        private static ConcurrentDictionary<string, User> ChatClients = new ConcurrentDictionary<string, User>();


        public override Task OnDisconnectedAsync(Exception exception)
        {
            Debug.WriteLine(exception.Message);

            var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                Clients.Others.ParticipantDisconnection(userName);
                Console.WriteLine($"<> {userName} disconnected");
            }
            return base.OnDisconnectedAsync(exception);
        }

        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
        //    if (userName != null)
        //    {
        //        Clients.Others.ParticipantDisconnection(userName);
        //        Console.WriteLine($"<> {userName} disconnected");
        //    }
        //    return base.OnDisconnected(stopCalled);
        //}

        public override Task OnConnectedAsync()
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                Clients.Others.ParticipantReconnection(userName);
                Console.WriteLine($"== {userName} reconnected");
            }
            return base.OnConnectedAsync();
        }
        //public override Task OnReconnected()
        //{
        //    var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
        //    if (userName != null)
        //    {
        //        Clients.Others.ParticipantReconnection(userName);
        //        Console.WriteLine($"== {userName} reconnected");
        //    }
        //    return base.OnReconnected();
        //}

        public async Task SendMessage(string user, string message)
        {
            //await Clients.Others.SendAsync("ReceiveMessage", user, message);
        }

        public List<User> Login(string name, byte[] photo)
        {
            if (!ChatClients.ContainsKey(name))
            {
                Console.WriteLine($"++ {name} logged in");
                List<User> users = new List<User>(ChatClients.Values);
                User newUser = new User { Name = name, ID = Context.ConnectionId, Photo = photo };
                var added = ChatClients.TryAdd(name, newUser);
                if (!added) return null;
                //Clients.CallerState.UserName = name;
                
                Clients.Others.ParticipantLogin(newUser);
                return users;
            }
            return null;
        }

        public void Logout()
        {
            //var name = Clients.CallerState.UserName;
            var name = "no name";
            if (!string.IsNullOrEmpty(name))
            {
                User client = new User();
                ChatClients.TryRemove(name, out client);
                Clients.Others.ParticipantLogout(name);
                Console.WriteLine($"-- {name} logged out");
            }
        }

        public void BroadcastTextMessage(string message)
        {
            //var name = Clients.CallerState.UserName;
            var name = "no name";
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(message))
            {
                Clients.Others.BroadcastTextMessage(name, message);
            }
        }

        public void BroadcastImageMessage(byte[] img)
        {
            //var name = Clients.CallerState.UserName;
            var name = "no name";
            if (img != null)
            {
                Clients.Others.BroadcastPictureMessage(name, img);
            }
        }

        public void UnicastTextMessage(string recepient, string message)
        {
            //var sender = Clients.CallerState.UserName;
            var sender = "dd name";
            if (!string.IsNullOrEmpty(sender) && recepient != sender &&
                !string.IsNullOrEmpty(message) && ChatClients.ContainsKey(recepient))
            {
                User client = new User();
                ChatClients.TryGetValue(recepient, out client);
                Clients.Client(client.ID).UnicastTextMessage(sender, message);
            }
        }

        public void UnicastImageMessage(string recepient, byte[] img)
        {
            //var sender = Clients.CallerState.UserName;
            var sender = "dd name";
            if (!string.IsNullOrEmpty(sender) && recepient != sender &&
                img != null && ChatClients.ContainsKey(recepient))
            {
                User client = new User();
                ChatClients.TryGetValue(recepient, out client);
                Clients.Client(client.ID).UnicastPictureMessage(sender, img);
            }
        }

        public void Typing(string recepient)
        {
            if (string.IsNullOrEmpty(recepient)) return;
            //var sender = Clients.CallerState.UserName;
            var sender = "dd name";
            User client = new User();
            ChatClients.TryGetValue(recepient, out client);
            Clients.Client(client.ID).ParticipantTyping(sender);
        }
    }

    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, User> ChatClients = new ConcurrentDictionary<string, User>();

        public async Task<List<User>> Login(string name)
        {
            if (!ChatClients.ContainsKey(name))
            {
                Console.WriteLine($"++ {name} logged in");
                List<User> users = new List<User>(ChatClients.Values);
                User newUser = new User { Name = name, ID = Context.ConnectionId };
                var added = ChatClients.TryAdd(name, newUser);
                if (!added) return null;
                //Clients.CallerState.UserName = name;

                //Clients.Others.ParticipantLogin(newUser);
                await Clients.Others.SendAsync("ParticipantLogin",newUser);
                return users;
            }
            return null;
        }

        public async Task Logout(string callerName)
        {
            //var name = Clients.CallerState.UserName;
            var name = callerName;
            if (!string.IsNullOrEmpty(name))
            {
                User client = new User();
                ChatClients.TryRemove(name, out client);
                
                //Clients.Others.ParticipantLogout(name);
                await Clients.Others.SendAsync("ParticipantLogout", name);
                Console.WriteLine($"-- {name} logged out");
            }
        }

        public async Task Typing(string callerName,string recepient)
        {
            if (string.IsNullOrEmpty(recepient)) return;
            //var sender = Clients.CallerState.UserName;
            var sender = callerName;
            User client = new User();
            ChatClients.TryGetValue(recepient, out client);
            
            //Clients.Client(client.ID).ParticipantTyping(sender);
            await Clients.Client(client.ID).SendAsync("ParticipantTyping", sender);
        }



        public async Task UnicastTextMessage(string callerName, string recepient, string message)
        {
            //var sender = Clients.CallerState.UserName;
            var sender = callerName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender &&
                !string.IsNullOrEmpty(message) && ChatClients.ContainsKey(recepient))
            {
                User client = new User();
                ChatClients.TryGetValue(recepient, out client);
                
                //Clients.Client(client.ID).UnicastTextMessage(sender, message);
                await Clients.Client(client.ID).SendAsync("UnicastTextMessage", sender, message);
            }
        }

        public async Task BroadcastImageMessage(string callerName,byte[] img)
        {            
            var name = callerName;
            if (img != null)
            {
                //Clients.Others.BroadcastPictureMessage(name, img);
                await Clients.Others.SendAsync("BroadcastPictureMessage", name, img);
            }
        }

        public async Task UnicastImageMessage(string callerName,string recepient, byte[] img)
        {
            //var sender = Clients.CallerState.UserName;
            var sender = callerName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender &&
                img != null && ChatClients.ContainsKey(recepient))
            {
                User client = new User();
                ChatClients.TryGetValue(recepient, out client);
                
                //Clients.Client(client.ID).UnicastPictureMessage(sender, img);
                await Clients.Client(client.ID).SendAsync("UnicastPictureMessage", sender, img);
            }
        }

        public async Task UnicastVideoFrameMessage2(string img)
        {

            await Clients.Others.SendAsync("UnicastVideoFrameMessage2", img);
            //await Task.Factory.StartNew(() =>
            //{
            //    byte[] data = Encoding.UTF8.GetBytes(img);

            //});
        }

        public async Task UnicastVideoFrameMessage(string callerName, string recepient, byte[] img)
        {
            //var sender = Clients.CallerState.UserName;
            var sender = callerName;
            if (!string.IsNullOrEmpty(sender) && recepient != sender &&
                img != null && ChatClients.ContainsKey(recepient))
            {
                User client = new User();
                ChatClients.TryGetValue(recepient, out client);

                //Clients.Client(client.ID).UnicastPictureMessage(sender, img);
                await Clients.Client(client.ID).SendAsync("UnicastVideoFrameMessage", sender, img);
            }
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override Task OnConnectedAsync()
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                Clients.Others.SendAsync("ParticipantReconnection",userName);
                Console.WriteLine($"== {userName} reconnected");
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userName = ChatClients.SingleOrDefault((c) => c.Value.ID == Context.ConnectionId).Key;
            if (userName != null)
            {
                Clients.Others.SendAsync("ParticipantDisconnection",userName);
                Console.WriteLine($"<> {userName} disconnected");
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
