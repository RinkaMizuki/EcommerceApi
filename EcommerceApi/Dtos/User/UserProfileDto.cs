﻿namespace EcommerceApi.Dtos.User
{
    public class UserProfileDto
    {
        public string UserName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IFormFile Avatar { get; set; }
    }
}
