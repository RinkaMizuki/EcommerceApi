using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcommerceApi.Models.Segment;

namespace EcommerceApi.Models.Contact;

public class Contact
{
    [Key] public int ContactId { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]public User User { get; set; }
    public string Content { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public DateTime SentDate { get; set; }
}