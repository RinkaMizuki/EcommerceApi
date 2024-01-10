﻿using EcommerceApi.Dtos.Admin;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Rate;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.FeedbackService
{
    public class RateService : IRateService
    {
        private readonly EcommerceDbContext _context;
        public RateService(EcommerceDbContext context) {
            _context = context;
        }
        public async Task<bool> DeleteFeedbackAsync(int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var rateByID = await _context
                                        .Rates
                                        .Where(r => r.RateId == rateId)
                                        .FirstOrDefaultAsync(userCancellationToken);
                if (rateByID is null)
                {
                    throw new HttpStatusException(HttpStatusCode.NotFound, "Rate not found.");
                }
                _context.Rates.Remove(rateByID);
                await _context.SaveChangesAsync(userCancellationToken);
                return true;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<List<Rate>> GetListFeedbackAsync(CancellationToken userCancellationToken)
        {
            try
            {
                var listRates = await _context
                                        .Rates
                                        .Include(r => r.FeedbackRate)
                                        .AsNoTracking()
                                        .ToListAsync(userCancellationToken);
                return listRates;
            }
            catch(SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Rate> PostFeedbackAsync(RateDto rateDto, CancellationToken userCancellationToken)
        {
            try
            {
                var productRate = await _context
                                            .Products
                                            .Where(p => p.ProductId == rateDto.ProductId)
                                            .Include(p => p.ProductRates)
                                            .FirstOrDefaultAsync(userCancellationToken);
                var userRate = await _context
                                            .Users.Where(u => u.UserId == rateDto.UserId)
                                            .FirstOrDefaultAsync(userCancellationToken);

                if (productRate is null)
                {
                    throw new HttpStatusException(HttpStatusCode.NotFound, "Product not found.");
                }
                if (userRate is null)
                {
                    throw new HttpStatusException(HttpStatusCode.NotFound, "User not found.");
                }

                if (productRate.ProductRates.Where(pr => pr.UserId == userRate.UserId).Any())
                {
                    throw new HttpStatusException(HttpStatusCode.BadRequest, "This product has been rated.");
                }
                var newRate = new Rate()
                {
                    Content = rateDto.Content,
                    Star = rateDto.Star,
                    Status = "pending",
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    Product = productRate,
                    ProductId = productRate.ProductId,
                    User = userRate,
                    UserId = userRate.UserId,
                };

                await _context
                              .Rates
                              .AddAsync(newRate, userCancellationToken);

                await _context.SaveChangesAsync(userCancellationToken);

                return newRate;
            }
            catch (SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }

        public async Task<Rate> UpdateFeedbackAsync(RateDto rateDto, int rateId, CancellationToken userCancellationToken)
        {
            try
            {
                var updateRate = await _context
                                      .Rates
                                      .Where(r => r.RateId == rateId)
                                      .FirstOrDefaultAsync(userCancellationToken)
                                      ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Rate not found.");

                updateRate.Content = rateDto.Content;
                updateRate.Star = rateDto.Star;
                updateRate.ModifiedAt = DateTime.Now;
                updateRate.Status = rateDto.Status;

                await _context.SaveChangesAsync(userCancellationToken);

                return updateRate;
            }
            catch(SqlException ex)
            {
                throw new HttpStatusException((HttpStatusCode)ex.ErrorCode, ex.Message);
            }
        }
    }
}
