using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Contact;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.ContactService
{
    public class ContactService : IContactService
    {
        private readonly EcommerceDbContext _context;

        public ContactService(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Contact> PostContactAsync(ContactDto contactDto, CancellationToken userCancellation)
        {
            try
            {
                var userContact = await _context
                                                .Users
                                                .Where(c => c.UserId == contactDto.UserId)
                                                .FirstOrDefaultAsync(userCancellation)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");

                var newContact = new Contact() { 
                    Content = contactDto.Content,
                    Email = contactDto.Email,
                    Name = contactDto.Name,
                    Phone = contactDto.Phone,
                    SentDate = DateTime.Now,
                    UserId = contactDto.UserId,
                    User = userContact,
                };
                
                await _context.Contacts.AddAsync(newContact, userCancellation);
                await _context.SaveChangesAsync(userCancellation);
                return newContact;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<bool> DeleteContactAsync(int contactId, CancellationToken userCancellation)
        {
            try
            {
                var deleteContact = await _context
                                            .Contacts
                                            .Where(c => c.ContactId == contactId)
                                            .FirstOrDefaultAsync(userCancellation)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Contact not found.");
                _context.Remove(deleteContact);
                await _context.SaveChangesAsync(userCancellation);
                return true;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Contact> GetContactByIdAsync(int contactId, CancellationToken userCancellation)
        {
            try
            {
                var contactById = await _context
                                                .Contacts
                                                .Where(c => c.ContactId == contactId)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(userCancellation)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Contact not found.");
                return contactById;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<Contact>> GetListContactASync(CancellationToken userCancellation)
        {
            try
            {
                var listContact = await _context
                                                .Contacts
                                                .AsNoTracking()
                                                .ToListAsync(userCancellation);
                return listContact;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }
    }
}