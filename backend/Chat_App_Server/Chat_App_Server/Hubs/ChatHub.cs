using Chat_App_Server.Data;
using Chat_App_Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat_App_Server.Hubs
{
    public class ChatHub : Hub
    {
        //Set nickname our clients with their connectionId
        public async Task GetNickName(string nickName)
        {
            //static data
            Client client = new()
            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName
            };
            ClientSource.Clients.Add(client);
            //send to others except from me my nickname with 'clientJoined' method
            await Clients.Others.SendAsync("clientJoined", nickName);
            //send to all clients to client list
            await Clients.All.SendAsync("clients", ClientSource.Clients);
        }

        public async Task SendMessageAsync(string message, string clientName)
        {
            //send specific message with his/her connectionId
            Client senderClient = ClientSource.Clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            //if selection is All send message to everyone
            if (clientName.Trim() == "All")
            {
                //send all other clients message except from you
                await Clients.Others.SendAsync("receiveMessage", message, senderClient.NickName);
            }
            else
            {
                //find the nickname you sent the message to
                Client client = ClientSource.Clients.FirstOrDefault(x => x.NickName == clientName);
                //send message to specific client
                await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message, senderClient.NickName);
            }
        }

        public async Task AddGroup(string groupName)
        {
            Group group = new();
            //select client
            Client createdClient = ClientSource.Clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            //create group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            GroupSource.Groups.Add(new Group { GroupName = groupName });
            //add client who created new group to group's client list
            group.Clients.Add(createdClient);
            //send all users to 'I am created new group' automatically
            await Clients.All.SendAsync("receiveGroup", groupName, createdClient.NickName);
        }

        public async Task AddClientToGroup(IEnumerable<string> groupNames)
        {
            //select client
            Client client = ClientSource.Clients.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            foreach (var group in groupNames)
            {
                //identify which group do you wanna join
                Group _group = GroupSource.Groups.FirstOrDefault(x => x.GroupName == group);

                //check if user already exist current group
                var result = _group.Clients.Any(x => x.ConnectionId == Context.ConnectionId);
                if (!result)
                {
                    //join group
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
            //send to group your message and your nickname
            await Clients.Group(groupName).SendAsync("receiveMessage",message,client.NickName);
        }
    }
}
