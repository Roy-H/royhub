using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoyHub.Chat
{
    public class ChatGroup
    {
        Dictionary<string, WebSocket> mClients;
        public ChatGroup()
        {
            mClients = new Dictionary<string, WebSocket>();
        }

        public async Task Join(string id, WebSocket webSocket)
        {
            mClients.Remove(id);
            mClients[id] = webSocket;

            //BroadCast(id + " has joined this chat group");
            await StartChat(id,mClients[id]);
            //await Echo(webSocket);
        }

        private async Task BroadCast(string info)
        {
            byte[] re = Encoding.Default.GetBytes(info);
            int count = mClients.Keys.Count;
            foreach (var key in mClients.Keys)
            {
                await mClients[key].SendAsync(new ArraySegment<byte>(re, 0, re.Length),WebSocketMessageType.Text,true, CancellationToken.None);
            }
            
        }

        private async Task StartChat(string user,WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var text = Encoding.Default.GetString(buffer);
                

                text = user+" say:" + text.Substring(0, result.Count);
                

                await BroadCast(text);
                //await webSocket.SendAsync(new ArraySegment<byte>(re, 0, re.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

        }

        private async Task Echo( WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var a = Encoding.Default.GetString(buffer);
                Console.WriteLine("get info from client " + a);

                a = a.Substring(0, result.Count) + "_from server";
                byte[] re = Encoding.Default.GetBytes(a);

                await webSocket.SendAsync(new ArraySegment<byte>(re, 0, re.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public WebSocket Add(string id,WebSocket webSocket)
        {
            if (mClients.ContainsKey(id))
            {
                return mClients[id];
            }
            else
            {
                mClients[id] = webSocket;
                return mClients[id];
            }
        }

        public void Remove(string id, WebSocket webSocket)
        {
            if (mClients.ContainsKey(id))
            {
                mClients.Remove(id);
            }
        }


    }
}
