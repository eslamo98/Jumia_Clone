﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

namespace Jumia_Clone.Models.Entities;

public class AffiliateSellerRelationship
{
    public int RelationshipId { get; set; }

    public int AffiliateId { get; set; }

    public int SellerId { get; set; }

    public decimal? CommissionRate { get; set; }

    public string Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Affiliate Affiliate { get; set; }

    public virtual Seller Seller { get; set; }
}