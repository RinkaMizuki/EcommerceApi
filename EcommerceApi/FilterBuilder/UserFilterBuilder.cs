using EcommerceApi.Models.Segment;
using EcommerceApi.Responses;

namespace EcommerceApi.FilterBuilder
{
    public class UserFilterBuilder
    {
        private readonly List<Func<UserResponse, bool>> _filterOptions = new();

        public UserFilterBuilder AddBanFilter(string isBan)
        {
            if(!string.IsNullOrEmpty(isBan))
            {
                _filterOptions.Add(u => u.IsActive.Equals(Convert.ToBoolean(isBan)));
            }
            return this;
        }
        public UserFilterBuilder AddSegmentsFilter(List<string> segmentsValue)
        {
            //return elmCount >= filters.Count;
            if (segmentsValue.Count > 0 && !string.IsNullOrEmpty(segmentsValue[0]))
            {
                _filterOptions.Add(u => IsExistSegment(segmentsValue, u.Segments));
            }
            return this;
        }
        public UserFilterBuilder AddVerifyFilter(string isVerify)
        {
            if(!string.IsNullOrEmpty(isVerify))
            {
                _filterOptions.Add(u => u.EmailConfirm.Equals(Convert.ToBoolean(isVerify)));
            }
            return this;
        }
        private bool IsExistSegment(List<string> segmentsValue, List<Segment> userSegments)
        {
            int elmCount = 0;
            foreach (var us in userSegments) // review, order
            {
                foreach (var f in segmentsValue) // review
                {
                    if (f.ToLower() == us.Title.ToLower())
                    {
                        elmCount++;
                        break;
                    }
                }
            }

            return elmCount >= segmentsValue.Count;
        }

        public Func<UserResponse, bool> Build() => (user) => _filterOptions.All(filter => filter(user));
    }
}
