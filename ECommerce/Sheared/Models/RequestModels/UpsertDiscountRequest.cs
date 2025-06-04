using Sheared.Enums;

namespace Sheared.Models.RequestModels;

public sealed record UpsertDiscountRequest(
    string Code, DiscountType Type, decimal Value,
    bool IsActive, int? MaxUses, decimal? MinOrderTotal,
    DateTime? StartsAtUtc, DateTime? ExpiresAtUtc,
    string? Description);
