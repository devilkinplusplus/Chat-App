using Chat_App_Server.Data;
using Chat_App_Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat_App_Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task GetNickName(string nickName)
        {
            Client client = new()
            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName
            };
            ClientSource.Clients.Add(client);
            await Clients.Others.SendAsync("clientJoined", nickName);
            await Clients.All.SendAsync("clients", ClientSource.Clients);
        }

        public async Task SendMessageAsync(string message, string clientName)
        {
            Client senderClient = ClientSource.Clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (clientName.Trim() == "All")
            {
                //send all clients other yourself
                await Clients.Others.SendAsync("receiveMessage", message, senderClient.NickName);
            }
            else
            {
                //send specific client
                Client client = ClientSource.Clients.FirstOrDefault(x => x.NickName == clientName);
                await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message, senderClient.NickName);
            }
        }

        public async Task AddGroup(string groupName)
        {
            Group group = new();
            Client createdClient = ClientSource.Clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            GroupSource.Groups.Add(new Group { GroupName = groupName });
            //add client who created new group to group's client list
            group.Clients.Add(createdClient);

            await Clients.All.SendAsync("receiveGroup", groupName, createdClient.NickName);
        }

        public async Task AddClientToGroup(IEnumerable<string> groupNames)
        {
            Client client = ClientSource.Clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            foreach (var group in groupNames)
            {
                Group _group = GroupSource.Groups.FirstOrDefault(x => x.GroupName == group);

                //check if user already exist current group
                var result = _group.Clients.Any(x => x.ConnectionId == Context.ConnectionId);
                if (!result)
                {
                    _group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, group);
                }
            }
        }

        public async Task GetClientToGroup(string groupName)
        {
            if (groupName.Trim() == "-1")
            {
                //send everyone if rooms option selected
                await Clients.Caller.SendAsync("clients", ClientSource.Clients);
            }
            Group group = GroupSource.Groups.FirstOrDefault(x => x.GroupName == groupName);

            await Clients.Caller.SendAsync("clients", group.Clients);
        }


        public async Task SendGroupMessageAsync(string groupName,string message)
        {
            Client client = ClientSource.Clients.FirstOrDefault(x=>x.ConnectionId == Context.ConnectionId);
            await Clients.Group(groupName).SendAsync("receiveMessage",message,client.NickName);
        }
    }
}
