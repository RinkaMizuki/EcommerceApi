using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Payment;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.DestinateService
{
    public class DestinateService : IDestinateService
    {
        private readonly EcommerceDbContext _context;
        public DestinateService(EcommerceDbContext ecommerceDbContext)
        {
            _context = ecommerceDbContext;
        }
        public async Task<bool> DeleteDestinationAsync(Guid destinationId, CancellationToken cancellationToken)
        {
            var destinationDelete = await _context
                                                  .PaymentDestinations
                                                  .Where(pd => pd.DestinationId == destinationId)
                                                  .FirstOrDefaultAsync(cancellationToken)
                                                  ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Destination not found.");
            try
            {
                _context.PaymentDestinations.Remove(destinationDelete);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<PaymentDestination>> GetListDestinationAsync(CancellationToken cancellationToken)
        {
            try
            {
                var listDes = await _context
                    .PaymentDestinations
                    .AsNoTracking()
                    .Where(dp => dp.ParentDestinationId == null)
                    .Include(dp => dp.PaymentDestinationsChild)
                    .ToListAsync(cancellationToken);
                return listDes;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PaymentDestination> PostDestinationAsync(DestinationDto destinationDto, CancellationToken cancellationToken)
        {

            try
            {
                var newDestinate = new PaymentDestination()
                {
                    DestinationId = Guid.NewGuid(),
                    DesLogo = destinationDto.DesLogo,
                    DesName = destinationDto.DesName,
                    DesShortName = destinationDto.DesShortName,
                    IsActive = destinationDto.IsActive,
                };
                PaymentDestination? parentDestination = null;
                if (destinationDto.ParentDestinationId != null)
                {
                    parentDestination = await _context
                                                      .PaymentDestinations
                                                      .Where(pd => pd.DestinationId == destinationDto.ParentDestinationId)
                                                      .FirstOrDefaultAsync(cancellationToken);
                    newDestinate.ParentDestinationId = parentDestination?.DestinationId;
                    newDestinate.ParentPaymentDestination = parentDestination;
                }
                else
                {
                    newDestinate.ParentDestinationId = null;
                }
                await _context
                              .PaymentDestinations
                              .AddAsync(newDestinate, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return newDestinate;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PaymentDestination> UpdateDestinationAsync(DestinationDto destinationDto, Guid destinationId,CancellationToken cancellationToken)
        {
             var desUpdate = await _context
                                            .PaymentDestinations
                                            .Where(pd => pd.DestinationId == destinationId)
                                            .FirstOrDefaultAsync(cancellationToken) 
                                            ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Destination not found.");
            try
            {
                desUpdate.IsActive = destinationDto.IsActive;

                await _context.SaveChangesAsync(cancellationToken);
                return desUpdate;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
