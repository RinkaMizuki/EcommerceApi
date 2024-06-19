using System.Collections.Concurrent;

namespace EcommerceApi.Shared
{
    public class UserConnection
    {
        private readonly ConcurrentDictionary<string, string> _onlineUsers = new();

        public UserConnection()
        {
            _onlineUsers = new();
        }
        public void AddUser(string userId, string connectionId)
        {
            _onlineUsers.TryAdd(userId, connectionId);
        }
        public void RemoveUser(string userId)
        {
            _onlineUsers.TryRemove(userId, out _);
        }
        public List<string> GetAllUser()
        {
            return _onlineUsers.Keys.ToList();
        }
        public string GetOneUser(string userId)
        {
            return _onlineUsers[userId];
        }
    }
}
