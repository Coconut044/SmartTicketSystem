// Services/SlaService.cs
using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Data;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Services
{
    public interface ISlaService
    {
        Task<List<SlaConfigurationDto>> GetAllSlaConfigurationsAsync();
        Task<SlaConfigurationDto?> GetSlaByPriorityAsync(string priority);
        Task<SlaConfigurationDto> CreateSlaConfigurationAsync(CreateSlaConfigurationDto dto);
        Task<SlaConfigurationDto> UpdateSlaConfigurationAsync(int id, CreateSlaConfigurationDto dto);
        Task<bool> DeleteSlaConfigurationAsync(int id);
        Task<DateTime?> CalculateDueDateAsync(string priority, DateTime createdAt);
    }

    public class SlaService : ISlaService
    {
        private readonly ApplicationDbContext _context;

        public SlaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SlaConfigurationDto>> GetAllSlaConfigurationsAsync()
        {
            return await _context.SlaConfigurations
                .OrderBy(s => s.Priority)
                .Select(s => MapToDto(s))
                .ToListAsync();
        }

        public async Task<SlaConfigurationDto?> GetSlaByPriorityAsync(string priority)
        {
            var sla = await _context.SlaConfigurations
                .FirstOrDefaultAsync(s => s.Priority == priority && s.IsActive);

            return sla != null ? MapToDto(sla) : null;
        }

        public async Task<SlaConfigurationDto> CreateSlaConfigurationAsync(CreateSlaConfigurationDto dto)
        {
            var exists = await _context.SlaConfigurations
                .AnyAsync(s => s.Priority == dto.Priority);

            if (exists)
                throw new InvalidOperationException($"SLA configuration for priority '{dto.Priority}' already exists");

            var sla = new SlaConfiguration
            {
                Priority = dto.Priority,
                ResponseTimeHours = dto.ResponseTimeHours,
                ResolutionTimeHours = dto.ResolutionTimeHours,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SlaConfigurations.Add(sla);
            await _context.SaveChangesAsync();

            return MapToDto(sla);
        }

        public async Task<SlaConfigurationDto> UpdateSlaConfigurationAsync(int id, CreateSlaConfigurationDto dto)
        {
            var sla = await _context.SlaConfigurations.FindAsync(id);

            if (sla == null)
                throw new InvalidOperationException("SLA configuration not found");

            sla.Priority = dto.Priority;
            sla.ResponseTimeHours = dto.ResponseTimeHours;
            sla.ResolutionTimeHours = dto.ResolutionTimeHours;

            await _context.SaveChangesAsync();

            return MapToDto(sla);
        }

        public async Task<bool> DeleteSlaConfigurationAsync(int id)
        {
            var sla = await _context.SlaConfigurations.FindAsync(id);

            if (sla == null)
                return false;

            sla.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<DateTime?> CalculateDueDateAsync(string priority, DateTime createdAt)
        {
            var sla = await _context.SlaConfigurations
                .FirstOrDefaultAsync(s => s.Priority == priority && s.IsActive);

            if (sla == null)
                return null;

            return createdAt.AddHours(sla.ResolutionTimeHours);
        }

        private static SlaConfigurationDto MapToDto(SlaConfiguration sla)
        {
            return new SlaConfigurationDto
            {
                Id = sla.Id,
                Priority = sla.Priority,
                ResponseTimeHours = sla.ResponseTimeHours,
                ResolutionTimeHours = sla.ResolutionTimeHours,
                IsActive = sla.IsActive,
                CreatedAt = sla.CreatedAt
            };
        }
    }
}