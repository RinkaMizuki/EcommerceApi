using EcommerceApi.Models.UserAddress;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EcommerceApi.Models.Chat
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        public Guid SeenderId { get; set; }
        [JsonIgnore]
        [ForeignKey("SeenderId")]
        public User Seender { get; set; }
        public Guid ConversationId { get; set; }
        [JsonIgnore]
        [ForeignKey("ConversationId")]
        public Conversation Conversation { get; set; }
        public string MessageContent {  get; set; } = string.Empty;
        public DateTime SendAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public Guid? OriginalMessageId { get; set; } = null;

        [ForeignKey("OriginalMessageId")]
        [JsonIgnore]
        public Message? OriginalMessage { get; set; } = null;
        public List<Message> ListMessageChild { get; set; } = new();
    }
}
