﻿namespace EcommerceApi.Entities;

public class WishlistItem : IBaseEntity
{
    public Guid WishlistItemId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public required Guid TenantId { get; set; }
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
