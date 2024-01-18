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
        public async Task<IActionResult> GetListCategory(CancellationToken userCancellationToken)
        {
            try
            {
                var listCate = await _categoryService.GetListCategoryAsync(userCancellationToken);
                var listCateResponse = listCate
                                                .Where(pc => pc?.ParentProductCategory == null)
                                                .ToList();
                return new JsonResult(listCateResponse);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("categories/delete/{categoryId:int}")]
        public async Task<IActionResult> DeleteCategory(int categoryId, CancellationToken userCancellationToken)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(categoryId,userCancellationToken);
                if (result) return NoContent();
                return BadRequest();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("categories/post")]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto, CancellationToken userCancellationToken)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);

                if (string.IsNullOrEmpty(userName)) return BadRequest();
                var result = await _categoryService.PostCategoryAsync(categoryDto, userName, userCancellationToken);
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPut]
        [Route("categories/update/{categoryId:int}")]
        public async Task<IActionResult> UpdateCategory(CategoryDto categoryDto, int categoryId, CancellationToken userCancellationToken)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);
                var result = await _categoryService.UpdateCategoryAsync(categoryDto, categoryId, userName, userCancellationToken);
                if (result is null) return BadRequest("Can't update category");
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}