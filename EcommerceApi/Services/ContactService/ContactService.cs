using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Contact;
using EcommerceApi.Models.Segment;
using EcommerceApi.Services.SegmentService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.ContactService
{
    public class ContactService : IContactService
    {
        private readonly EcommerceDbContext _context;
        private readonly ISegmentService _segmentService;

        public ContactService(EcommerceDbContext context, ISegmentService segmentService)
        {
            _context = context;
            _segmentService = segmentService;
        }

        public async Task<Contact> PostContactAsync(ContactDto contactDto, CancellationToken userCancellationToken)
        {
            try
            {
                var userContact = await _context
                                                .Users
                                                .Where(c => c.UserId == contactDto.UserId)
                                                .Include(u => u.UserSegments)
                                                .ThenInclude(us => us.Segment)
                                                .FirstOrDefaultAsync(userCancellationToken)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
                var segments = await _context
                                            .Segments
                                            .Include(s => s.Users)
                                            .AsNoTracking()
                                            .ToListAsync(userCancellationToken);

                var newContact = new Contact() { 
                    Content = contactDto.Content,
                    Email = contactDto.Email,
                    Name = contactDto.Name,
                    Phone = contactDto.Phone,
                    SentDate = DateTime.Now,
                    UserId = contactDto.UserId,
                    User = userContact,
                };

                bool flag = false;

                Segment segment = null;

                foreach (var s in segments)
                {
                    if (s.Title == "Contact")
                    {
                        segment = s;
                        foreach (var u in s.Users)
                        {
                            if (u.UserId == userContact.UserId)
                            {
                                flag = true;
                                break;
                            }
                        }
                        break;
                    }
                }

                if (!flag)
                {
                    var newUserSegment = new UserSegment()
                    {
                        SegmentId = segment!.SegmentId,
                        UserId = userContact.UserId,
                        User = userContact
                    };
                    await _context.UserSegments.AddAsync(newUserSegment, userCancellationToken);
                }

                await _context.Contacts.AddAsync(newContact, userCancellationToken);
                await _context.SaveChangesAsync(userCancellationToken);

                return newContact;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<bool> DeleteContactAsync(int contactId, CancellationToken userCancellationToken)
        {
            try
            {
                var deleteContact = await _context
                                            .Contacts
                                            .Where(c => c.ContactId == contactId)
                                            .FirstOrDefaultAsync(userCancellationToken)
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Contact not found.");
                _context.Remove(deleteContact);
                await _context.SaveChangesAsync(userCancellationToken);
                return true;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Contact> GetContactByIdAsync(int contactId, CancellationToken userCancellationToken)
        {
            try
            {
                var contactById = await _context
                                                .Contacts
                                                .Where(c => c.ContactId == contactId)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(userCancellationToken)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Contact not found.");
                return contactById;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<Contact>> GetListContactASync(CancellationToken userCancellationToken)
        {
            try
            {
                var listContact = await _context
                                                .Contacts
                                                .AsNoTracking()
                                                .ToListAsync(userCancellationToken);
                return listContact;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }
    }
}