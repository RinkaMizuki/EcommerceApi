using System.Collections.Concurrent;

namespace EcommerceApi.Shared
{
    public class AdminConnection
    {
        private readonly ConcurrentDictionary<string, string> _onlineAdmins = new();

        public AdminConnection()
        {
            _onlineAdmins = new();
        }
        public void AddAdmin(string adminId, string connectionId)
        {
            _onlineAdmins.TryAdd(adminId, connectionId);    
        }
        public void RemoveAdmin(string adminId)
        {
            _onlineAdmins.TryRemove(adminId, out _);
        }
        public bool IsOnlineAdmin(string adminId)
        {
            return _onlineAdmins.TryGetValue(adminId, out _);
        }
        public KeyValuePair<string, string>? GetRandomAdmin()
        {
            if(_onlineAdmins.IsEmpty)
            {
                return null;
            }
            Random rd = new ();
            int index = rd.Next(_onlineAdmins.Count);
            var admin = _onlineAdmins.ElementAt(index);
            return admin;
        }
    }
}
