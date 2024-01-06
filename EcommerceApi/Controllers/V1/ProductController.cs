using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services;
using EcommerceApi.Services.ProductService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceApi.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [Authorize(IdentityData.AdminPolicyName)]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICloudflareClient _cloudflareClient;
        public ProductController(IProductService productService,ICloudflareClient cloudflareClient) {
            _productService = productService;
            _cloudflareClient = cloudflareClient;
        }
        [HttpGet("products")]
        public async Task<IActionResult> GetListProduct()
        {
            try
            {
                var listProduct = await _productService.GetListProductAsync();
                return new JsonResult(listProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("product/{productId:Guid}")]
        public async Task<IActionResult> GetListProduct(Guid productId)
        {
            try
            {
                var productById = await _productService.GetProductByIdAsync(productId);
                return new JsonResult(productById);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("product")]
        public async Task<IActionResult> CreateProduct([FromForm]ProductDto productDto)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);
                var newProduct = await _productService.PostProductAsync(productDto, userName, Request);
                return new JsonResult(newProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet("product/preview")]
        public async Task<IActionResult> GetImage(string productImage)
        {
            try
            {
                var response = await _cloudflareClient.GetObjectAsync(productImage);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    return File(response.ResponseStream, response.Headers.ContentType);
                }

                throw new Exception("Can't not get object");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
