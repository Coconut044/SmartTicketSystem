using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.Repositories;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CategoryDto>>>> GetCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                SlaHours = c.SlaHours,
                CreatedAt = c.CreatedAt
            });

            return Ok(new ApiResponseDto<IEnumerable<CategoryDto>>
            {
                Success = true,
                Message = "Categories retrieved successfully",
                Data = categoryDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> GetCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new ApiResponseDto<CategoryDto>
                {
                    Success = false,
                    Message = "Category not found"
                });
            }

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                SlaHours = category.SlaHours,
                CreatedAt = category.CreatedAt
            };

            return Ok(new ApiResponseDto<CategoryDto>
            {
                Success = true,
                Message = "Category retrieved successfully",
                Data = categoryDto
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> CreateCategory(
            [FromBody] CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                SlaHours = dto.SlaHours,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdCategory = await _categoryRepository.CreateAsync(category);

            var categoryDto = new CategoryDto
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name,
                Description = createdCategory.Description,
                IsActive = createdCategory.IsActive,
                SlaHours = createdCategory.SlaHours,
                CreatedAt = createdCategory.CreatedAt
            };

            return CreatedAtAction(nameof(GetCategory), new { id = categoryDto.Id }, new ApiResponseDto<CategoryDto>
            {
                Success = true,
                Message = "Category created successfully",
                Data = categoryDto
            });
        }

        // ✅ ADDED: Update Category
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<CategoryDto>>> UpdateCategory(
            int id,
            [FromBody] CreateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new ApiResponseDto<CategoryDto>
                {
                    Success = false,
                    Message = "Category not found"
                });
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.SlaHours = dto.SlaHours;

            await _categoryRepository.UpdateAsync(category);

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                SlaHours = category.SlaHours,
                CreatedAt = category.CreatedAt
            };

            return Ok(new ApiResponseDto<CategoryDto>
            {
                Success = true,
                Message = "Category updated successfully",
                Data = categoryDto
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteCategory(int id)
        {
            var result = await _categoryRepository.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Category not found"
                });
            }

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Category deleted successfully",
                Data = true
            });
        }
    }
}
