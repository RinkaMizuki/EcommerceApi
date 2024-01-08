using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Models.Product;
using EcommerceApi.Services;
using EcommerceApi.Services.ProductService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin/")]
    [Authorize(IdentityData.AdminPolicyName)]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICloudflareClientService _cloudFlareService;
        public ProductController(IProductService productService, ICloudflareClientService cloudflareClientService)
        {
            _productService = productService;
            _cloudFlareService = cloudflareClientService;
        }
        [HttpGet]
        [Route("products")]
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
        [HttpGet]
        [Route("products/{productId:Guid}")]
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
        [HttpPost]
        [Route("products/post")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto productDto, CancellationToken userCancellationToken)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);
                var newProduct = await _productService.PostProductAsync(productDto, userName, Request, userCancellationToken);
                return new JsonResult(newProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Route("products/delete/{productId:Guid}")]
        public async Task<IActionResult> DeleteProduct(Guid productId, CancellationToken userCancellationToken)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(productId, userCancellationToken);
                if (!result) return BadRequest();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Route("products/update/{productId:Guid}")]
        public async Task<IActionResult> UpdateProduct([FromForm] ProductDto productDto, Guid productId, CancellationToken userCancellationToken)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);
                var result = await _productService.UpdateProductAsync(productDto, productId, userName, Request, userCancellationToken);
                if (result == null) return BadRequest();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("products/category/{categoryId:int}")]
        public async Task<IActionResult> GetProductByCategory(int categoryId, CancellationToken userCancellationToken)
        {
            try
            {
                var listProductByCate = await _productService.GetProductByCategoryAsync(categoryId, userCancellationToken);
                return new JsonResult(listProductByCate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("products/preview")]
        public async Task<IActionResult> GetImage(string productImage)
        {
            try
            {
                var response = await _cloudFlareService.GetObjectAsync(productImage);
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

        [AllowAnonymous]
        [HttpGet]
        [Route("products/images")]
        public async Task<IActionResult> GetListImage(string prefix)
        {
            try
            {
                var response = await _cloudFlareService.GetListObjectAsync(prefix);
                var files = response.Select(x => x.Key);
                var arquivos = files.Select(x => $"{Request.Scheme}://{Request.Host}/api/v1/Admin/product/preview?productImage={Path.GetFileName(x)}").ToList();
                return Ok(arquivos);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }
    }
}
