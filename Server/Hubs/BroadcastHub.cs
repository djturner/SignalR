using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public class PingHub : Hub
    {
        public async Task Ping(string pinger)
        {
            await Clients.Others.SendAsync("Pinged", pinger);
        }

        public async Task Pong(string pinger, string ponger)
        {
            await Clients.Client(pinger).SendAsync("Ponged", ponger);
        }
    }
}