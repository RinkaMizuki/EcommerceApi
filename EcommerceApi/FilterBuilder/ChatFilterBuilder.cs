using EcommerceApi.Models.Chat;

namespace EcommerceApi.FilterBuilder
{
    public class ChatFilterBuilder
    {
        private readonly List<Func<Participation, bool>> _filterOptions = new();
        public ChatFilterBuilder AddSearchFilter(string searchValue)
        {
            if(!string.IsNullOrEmpty(searchValue))
            {
                _filterOptions.Add(pp => pp.User.UserName.ToLower().Contains(searchValue) || (!string.IsNullOrEmpty(pp.Conversation.Title) && pp.Conversation.Title.Equals(searchValue)));
            }
            return this;
        }
        public ChatFilterBuilder AddIdFilter(string adminId)
        {
            if(!string.IsNullOrEmpty(adminId))
            {
                _filterOptions.Add(pp => pp.AdminId.Equals(Guid.Parse(adminId)));
            }
            return this;
        }
        public Func<Participation, bool> Build() => pp => _filterOptions.All(filter => filter(pp));
    }
}
