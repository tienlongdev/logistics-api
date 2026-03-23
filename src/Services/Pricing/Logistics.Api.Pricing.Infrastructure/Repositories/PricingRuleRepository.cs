using Logistics.Api.Pricing.Domain.Entities;
using Logistics.Api.Pricing.Domain.Enums;
using Logistics.Api.Pricing.Domain.Repositories;
using Logistics.Api.Pricing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Api.Pricing.Infrastructure.Repositories;

internal sealed class PricingRuleRepository : IPricingRuleRepository
{
    private readonly PricingDbContext _context;

    public PricingRuleRepository(PricingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PricingRule>> GetActiveRulesAsync(
        ServiceType serviceType,
        CancellationToken ct = default)
    {
        return await _context.PricingRules
            .Where(r => r.IsActive && r.ServiceType == serviceType)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(ct);
    }

    public void Add(PricingRule rule) => _context.PricingRules.Add(rule);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
