using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Contact;

namespace EcommerceApi.Services.ContactService
{
    public interface IContactService
    {
        public Task<List<Contact>> GetListContactASync(string sort, string range, string filter, HttpResponse response, CancellationToken userCancellation);
        public Task<Contact> GetContactByIdAsync(int contactId, CancellationToken userCancellation);
        public Task<bool> DeleteContactAsync(int contactId, CancellationToken userCancellation);
        public Task<Contact> PostContactAsync(ContactDto contactDto, CancellationToken userCancellation);
        public Task<bool> PostSupportContactAsync(ContactDto contactDto, CancellationToken userCancellation);
    }
}