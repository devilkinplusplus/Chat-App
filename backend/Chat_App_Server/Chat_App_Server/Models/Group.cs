namespace Chat_App_Server.Models
{
    public class Group
    {
        public string GroupName { get; set; }
        public List<Client> Clients { get; set; } = new();
    }
}
