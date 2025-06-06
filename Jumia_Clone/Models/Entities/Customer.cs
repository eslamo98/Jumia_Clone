﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable

namespace Jumia_Clone.Models.Entities;

public class Customer
{
    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<HelpfulRating> HelpfulRatings { get; set; } = new List<HelpfulRating>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductView> ProductViews { get; set; } = new List<ProductView>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();

    public virtual User User { get; set; }

    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();

    public virtual ICollection<UserProductInteraction> UserProductInteractions { get; set; } = new List<UserProductInteraction>();

    public virtual ICollection<UserRecommendation> UserRecommendations { get; set; } = new List<UserRecommendation>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}