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
        public async Task<IActionResult> GetListProduct(
            [FromQuery(Name = "sort")] string sort,
            [FromQuery(Name = "range")] string range,
            [FromQuery(Name = "filter")] string filter, 
            CancellationToken userCancellationToken
            )
        {
            var listProduct = await _productService.GetListProductAsync(sort, range, filter, HttpContext.Response, userCancellationToken);
            return new JsonResult(listProduct);
        }
        [HttpGet]
        [Route("products/{productId:Guid}")]
        public async Task<IActionResult> GetListProduct(Guid productId, CancellationToken userCancellationToken)
        {
            var productById = await _productService.GetProductByIdAsync(productId, userCancellationToken);
            return new JsonResult(productById);
        }
        [HttpPost]
        [Route("products/post")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto productDto, CancellationToken userCancellationToken)
        {
            var userName = Helpers.GetUserNameLogin(HttpContext);
            var newProduct = await _productService.PostProductAsync(productDto, userName, Request, userCancellationToken);
            return new JsonResult(newProduct);
        }
        [HttpDelete]
        [Route("products/delete/{productId:Guid}")]
        public async Task<IActionResult> DeleteProduct(Guid productId, CancellationToken userCancellationToken)
        {
            await _productService.DeleteProductAsync(productId, userCancellationToken);
            return NoContent();
        }
        [HttpPut]
        [Route("products/update/{productId:Guid}")]
        public async Task<IActionResult> UpdateProduct([FromForm] ProductDto productDto, Guid productId, CancellationToken userCancellationToken)
        {
            var userName = Helpers.GetUserNameLogin(HttpContext);
            var result = await _productService.UpdateProductAsync(productDto, productId, userName, Request, userCancellationToken);
            return Ok(result);
        }
        [HttpGet]
        [Route("products/category/{categoryId:int}")]
        public async Task<IActionResult> GetProductByCategory(int categoryId, CancellationToken userCancellationToken)
        {
            var listProductByCate = await _productService.GetProductByCategoryAsync(categoryId, userCancellationToken);
            return new JsonResult(listProductByCate);
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("product/preview")]
        public async Task<IActionResult> GetProductImage(string productImage, CancellationToken userCancellationToken)
        {
            var userAvatar = await _productService.GetImageAsync(productImage, userCancellationToken);
            return File(userAvatar.FileStream, userAvatar.ContentType);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("products/images")]
        public async Task<IActionResult> GetListProductImage(string prefix, CancellationToken userCancellationToken)
        {
            var response = await _cloudFlareService.GetListObjectAsync(prefix, userCancellationToken);
            var files = response.Select(x => x.Key);
            var arquivos = files.Select(x => $"{Request.Scheme}://{Request.Host}/api/v1/Admin/product/preview?productImage={Path.GetFileName(x)}").ToList();
            return Ok(arquivos);
        }
    }
}
