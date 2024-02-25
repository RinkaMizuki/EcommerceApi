using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services.CategoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1.Admin
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Admin")]
    [Authorize(Policy = IdentityData.AdminPolicyName)]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Route("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListCategory([FromQuery] string filter,
            CancellationToken userCancellationToken)
        {
            var listCate = await _categoryService.GetListCategoryAsync(filter, userCancellationToken);
            return new JsonResult(listCate);
        }

        [HttpGet]
        [Route("categories/{categoryId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListCategoryById(int categoryId, CancellationToken userCancellationToken)
        {
            var cateById = await _categoryService.GetCategoryByIdAsync(categoryId, userCancellationToken);
            return new JsonResult(cateById);
        }

        [HttpDelete]
        [Route("categories/delete/{categoryId:int}")]
        public async Task<IActionResult> DeleteCategory(int categoryId, CancellationToken userCancellationToken)
        {
            var result = await _categoryService.DeleteCategoryAsync(categoryId, userCancellationToken);
            if (result) return NoContent();
            return BadRequest();
        }

        [HttpPost]
        [Route("categories/post")]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto,
            CancellationToken userCancellationToken)
        {
            var userName = Helpers.GetUserNameLogin(HttpContext);

            if (string.IsNullOrEmpty(userName)) return BadRequest();
            var result = await _categoryService.PostCategoryAsync(categoryDto, userName, userCancellationToken);
            return new JsonResult(result);
        }

        [HttpPut]
        [Route("categories/update/{categoryId:int}")]
        public async Task<IActionResult> UpdateCategory(CategoryDto categoryDto, int categoryId,
            CancellationToken userCancellationToken)
        {
            var userName = Helpers.GetUserNameLogin(HttpContext);
            var result =
                await _categoryService.UpdateCategoryAsync(categoryDto, categoryId, userName,
                    userCancellationToken);
            if (result is null) return BadRequest("Can't update category");
            return new JsonResult(result);
        }
    }
}