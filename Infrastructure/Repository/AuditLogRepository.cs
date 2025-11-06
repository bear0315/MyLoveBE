using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DatabaseContext _context;

        public AuditLogRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            auditLog.CreatedAt = DateTime.UtcNow;
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(int userId, int skip = 0, int take = 50)
        {
            return await _context.AuditLogs
                .Where(al => al.UserId == userId && !al.IsDeleted)
                .OrderByDescending(al => al.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId)
        {
            return await _context.AuditLogs
                .Where(al => al.EntityName == entityName
                    && al.EntityId == entityId
                    && !al.IsDeleted)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByActionAsync(string action, int skip = 0, int take = 50)
        {
            return await _context.AuditLogs
                .Where(al => al.Action == action && !al.IsDeleted)
                .OrderByDescending(al => al.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
