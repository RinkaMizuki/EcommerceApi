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

namespace EcommerceApi.Controllers.V1
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

        [HttpGet("categories")]
        public async Task<IActionResult> GetListCategory()
        {
            try
            {
                var listCate = await _categoryService.GetListCategoryAsync();
                listCate = listCate.Where(pc => pc?.ParentProductCategory == null).ToList();
                return new JsonResult(listCate);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("category/{categoryId:int}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(categoryId);
                if (result) return NoContent();
                return BadRequest();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("category")]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);

                if (string.IsNullOrEmpty(userName)) return BadRequest();
                var result = await _categoryService.PostCategoryAsync(categoryDto, userName);
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPut("category/{categoryId:int}")]
        public async Task<IActionResult> UpdateCategory(CategoryDto categoryDto,int categoryId)
        {
            try
            {
                var userName = Helpers.GetUserNameLogin(HttpContext);
                var result = await _categoryService.UpdateCategoryAsync(categoryDto, categoryId, userName);
                if(result is null) return BadRequest("Can't update category");
                return new JsonResult(result);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}