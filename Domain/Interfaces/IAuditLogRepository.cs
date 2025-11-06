using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> CreateAsync(AuditLog auditLog);
        Task<List<AuditLog>> GetByUserIdAsync(int userId, int skip = 0, int take = 50);
        Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId);
        Task<List<AuditLog>> GetByActionAsync(string action, int skip = 0, int take = 50);
    }
}
