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
        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User Sender { get; set; }
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
