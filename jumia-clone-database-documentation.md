# Jumia Clone Database Documentation

## Table of Contents
1. [Introduction](#introduction)
2. [Core User Management System](#core-user-management-system)
3. [Product Management System](#product-management-system)
4. [Product Variant System](#product-variant-system)
5. [Shopping Experience](#shopping-experience)
6. [Order Management System](#order-management-system)
7. [Review and Rating System](#review-and-rating-system)
8. [Promotional System](#promotional-system)
9. [Affiliate System](#affiliate-system)
10. [Recommendation System](#recommendation-system)
11. [Analytics System](#analytics-system)
12. [Database Indexes](#database-indexes)
13. [Stored Procedures](#stored-procedures)

## Introduction

This document provides a comprehensive description of the Jumia clone database schema. The database is designed to support a multi-vendor e-commerce platform with features such as product management, user management, order processing, reviews, affiliate marketing, and recommendation systems.

## Core User Management System

### User Table
Stores common user information shared across all user types.

**Table Name:** `User`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| user_id | INT IDENTITY(1,1) | Primary key | PK |
| email | NVARCHAR(255) | User's email address | NOT NULL, UNIQUE |
| password_hash | NVARCHAR(255) | Hashed password | NOT NULL |
| first_name | NVARCHAR(100) | User's first name | NOT NULL |
| last_name | NVARCHAR(100) | User's last name | NOT NULL |
| phone_number | NVARCHAR(20) | User's contact number | NULL |
| created_at | DATETIME2 | When the user account was created | DEFAULT(GETDATE()) |
| updated_at | DATETIME2 | When the user account was last updated | DEFAULT(GETDATE()) |
| user_type | NVARCHAR(10) | Type of user | NOT NULL, CHECK(user_type IN ('customer', 'seller', 'admin')) |
| is_active | BIT | Whether the user account is active | DEFAULT(1) |

### Customer Table
Stores specific information for users who are shoppers on the platform.

**Table Name:** `Customer`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| customer_id | INT IDENTITY(1,1) | Primary key | PK |
| user_id | INT | Foreign key to the User table | FK, UNIQUE, NOT NULL |
| last_login | DATETIME2 | When the customer last logged in | NULL |

### Seller Table
Stores information specific to sellers/vendors on the platform.

**Table Name:** `Seller`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| seller_id | INT IDENTITY(1,1) | Primary key | PK |
| user_id | INT | Foreign key to the User table | FK, UNIQUE, NOT NULL |
| business_name | NVARCHAR(255) | Name of the seller's business | NOT NULL |
| business_description | NVARCHAR(MAX) | Description of the seller's business | NULL |
| business_logo | NVARCHAR(255) | URL to the seller's business logo | NULL |
| is_verified | BIT | Whether the seller is verified | DEFAULT(0) |
| verified_at | DATETIME2 | When the seller was verified | NULL |
| rating | FLOAT | Average rating of the seller | DEFAULT(0) |

### Admin Table
Stores information specific to administrators of the platform.

**Table Name:** `Admin`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| admin_id | INT IDENTITY(1,1) | Primary key | PK |
| user_id | INT | Foreign key to the User table | FK, UNIQUE, NOT NULL |
| role | NVARCHAR(50) | Administrative role | NOT NULL |
| permissions | NVARCHAR(MAX) | JSON-formatted permission structure | NULL |

### Address Table
Stores address information for users.

**Table Name:** `Address`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| address_id | INT IDENTITY(1,1) | Primary key | PK |
| user_id | INT | Foreign key to the User table | FK, NOT NULL |
| street_address | NVARCHAR(255) | Street address | NOT NULL |
| city | NVARCHAR(100) | City name | NOT NULL |
| state | NVARCHAR(100) | State or province | NULL |
| postal_code | NVARCHAR(20) | Postal or ZIP code | NOT NULL |
| country | NVARCHAR(100) | Country name | NOT NULL |
| phone_number | NVARCHAR(20) | Contact number for this address | NOT NULL |
| is_default | BIT | Whether this is the default address | DEFAULT(0) |
| address_name | NVARCHAR(50) | Custom name for the address (e.g., Home, Work) | DEFAULT('Home') |

## Product Management System

### Category Table
Stores top-level product categories.

**Table Name:** `Category`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| category_id | INT IDENTITY(1,1) | Primary key | PK |
| name | NVARCHAR(100) | Category name | NOT NULL, UNIQUE |
| description | NVARCHAR(MAX) | Description of the category | NULL |
| image_url | NVARCHAR(255) | URL to the category image | NULL |
| is_active | BIT | Whether the category is active | DEFAULT(1) |

### SubCategory Table
Stores second-level product categories.

**Table Name:** `SubCategory`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| subcategory_id | INT IDENTITY(1,1) | Primary key | PK |
| category_id | INT | Foreign key to the Category table | FK, NOT NULL |
| name | NVARCHAR(100) | Subcategory name | NOT NULL |
| description | NVARCHAR(MAX) | Description of the subcategory | NULL |
| image_url | NVARCHAR(255) | URL to the subcategory image | NULL |
| is_active | BIT | Whether the subcategory is active | DEFAULT(1) |
|  |  |  | UNIQUE(category_id, name) |

### Product Table
Stores the core product information.

**Table Name:** `Product`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| product_id | INT IDENTITY(1,1) | Primary key | PK |
| seller_id | INT | Foreign key to the Seller table | FK, NOT NULL |
| subcategory_id | INT | Foreign key to the SubCategory table | FK, NOT NULL |
| name | NVARCHAR(255) | Product name | NOT NULL |
| description | NVARCHAR(MAX) | Product description | NOT NULL |
| base_price | DECIMAL(10, 2) | Base price of the product | NOT NULL |
| discount_percentage | DECIMAL(5, 2) | Current discount percentage | DEFAULT(0) |
| is_available | BIT | Whether the product is in stock | DEFAULT(1) |
| approval_status | NVARCHAR(10) | Status of product approval | DEFAULT('pending'), CHECK(approval_status IN ('pending', 'approved', 'rejected')) |
| created_at | DATETIME2 | When the product was created | DEFAULT(GETDATE()) |
| updated_at | DATETIME2 | When the product was last updated | DEFAULT(GETDATE()) |
| stock_quantity | INT | Current inventory level | DEFAULT(0), NOT NULL |
| main_image_url | NVARCHAR(255) | URL to the main product image | NOT NULL |
| average_rating | FLOAT | Average rating from customer reviews | DEFAULT(0) |

### ProductAttribute Table
Defines attributes for products in a specific subcategory.

**Table Name:** `ProductAttribute`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| attribute_id | INT IDENTITY(1,1) | Primary key | PK |
| subcategory_id | INT | Foreign key to the SubCategory table | FK, NOT NULL |
| name | NVARCHAR(100) | Attribute name | NOT NULL |
| type | NVARCHAR(10) | Data type of the attribute | NOT NULL, CHECK(type IN ('text', 'number', 'boolean', 'select')) |
| possible_values | NVARCHAR(MAX) | JSON-formatted list of possible values | NULL |
| is_required | BIT | Whether the attribute is required | DEFAULT(0) |
| is_filterable | BIT | Whether the attribute can be used for filtering | DEFAULT(0) |
|  |  |  | UNIQUE(subcategory_id, name) |

### ProductAttributeValue Table
Stores the values of attributes for specific products.

**Table Name:** `ProductAttributeValue`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| value_id | INT IDENTITY(1,1) | Primary key | PK |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| attribute_id | INT | Foreign key to the ProductAttribute table | FK, NOT NULL |
| value | NVARCHAR(MAX) | Value of the attribute for this product | NOT NULL |
|  |  |  | UNIQUE(product_id, attribute_id) |

### ProductImage Table
Stores additional images for a product.

**Table Name:** `ProductImage`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| image_id | INT IDENTITY(1,1) | Primary key | PK |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| image_url | NVARCHAR(255) | URL to the image | NOT NULL |
| display_order | INT | Order in which to display the image | DEFAULT(0) |

## Product Variant System

### ProductVariant Table
Stores product variants with their own prices and inventory.

**Table Name:** `ProductVariant`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| variant_id | INT IDENTITY(1,1) | Primary key | PK |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| variant_name | NVARCHAR(100) | Name of the variant | NOT NULL |
| price | DECIMAL(10, 2) | Price of this variant | NOT NULL |
| discount_percentage | DECIMAL(5, 2) | Discount percentage for this variant | DEFAULT(0) |
| stock_quantity | INT | Inventory level for this variant | DEFAULT(0), NOT NULL |
| sku | NVARCHAR(50) | Stock Keeping Unit code | NULL |
| variant_image_url | NVARCHAR(255) | URL to variant-specific image | NULL |
| is_default | BIT | Whether this is the default variant | DEFAULT(0) |
| is_available | BIT | Whether this variant is available | DEFAULT(1) |
|  |  |  | UNIQUE(product_id, variant_name) |

### VariantAttribute Table
Stores specific attribute values for each variant.

**Table Name:** `VariantAttribute`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| variant_attribute_id | INT IDENTITY(1,1) | Primary key | PK |
| variant_id | INT | Foreign key to the ProductVariant table | FK, NOT NULL |
| attribute_name | NVARCHAR(50) | Name of the attribute (e.g., "Color") | NOT NULL |
| attribute_value | NVARCHAR(100) | Value of the attribute (e.g., "Red") | NOT NULL |
|  |  |  | UNIQUE(variant_id, attribute_name) |

## Shopping Experience

### Cart Table
Stores shopping cart information for customers.

**Table Name:** `Cart`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| cart_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NOT NULL |
| created_at | DATETIME2 | When the cart was created | DEFAULT(GETDATE()) |
| updated_at | DATETIME2 | When the cart was last updated | DEFAULT(GETDATE()) |

### CartItem Table
Stores items in a customer's shopping cart.

**Table Name:** `CartItem`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| cart_item_id | INT IDENTITY(1,1) | Primary key | PK |
| cart_id | INT | Foreign key to the Cart table | FK, NOT NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| quantity | INT | Quantity of the product | DEFAULT(1), NOT NULL |
| price_at_addition | DECIMAL(10, 2) | Price when added to cart | NOT NULL |
|  |  |  | UNIQUE(cart_id, product_id) |

### Wishlist Table
Stores wishlist information for customers.

**Table Name:** `Wishlist`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| wishlist_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NOT NULL |
| created_at | DATETIME2 | When the wishlist was created | DEFAULT(GETDATE()) |

### WishlistItem Table
Stores items in a customer's wishlist.

**Table Name:** `WishlistItem`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| wishlist_item_id | INT IDENTITY(1,1) | Primary key | PK |
| wishlist_id | INT | Foreign key to the Wishlist table | FK, NOT NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| added_at | DATETIME2 | When the item was added to the wishlist | DEFAULT(GETDATE()) |
|  |  |  | UNIQUE(wishlist_id, product_id) |

## Order Management System

### Order Table
Stores main order information.

**Table Name:** `Order`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| order_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NOT NULL |
| address_id | INT | Foreign key to the Address table | FK, NOT NULL |
| coupon_id | INT | Foreign key to the Coupon table | FK, NULL |
| total_amount | DECIMAL(10, 2) | Total price before discounts/taxes | NOT NULL |
| discount_amount | DECIMAL(10, 2) | Total discount applied | DEFAULT(0) |
| shipping_fee | DECIMAL(10, 2) | Shipping cost | DEFAULT(0) |
| tax_amount | DECIMAL(10, 2) | Tax amount | DEFAULT(0) |
| final_amount | DECIMAL(10, 2) | Final amount charged | NOT NULL |
| payment_method | NVARCHAR(20) | Method of payment | NOT NULL, CHECK(payment_method IN ('credit_card', 'debit_card', 'paypal', 'bank_transfer', 'cash_on_delivery')) |
| payment_status | NVARCHAR(10) | Status of payment | DEFAULT('pending'), CHECK(payment_status IN ('pending', 'paid', 'failed')) |
| created_at | DATETIME2 | When the order was created | DEFAULT(GETDATE()) |
| updated_at | DATETIME2 | When the order was last updated | DEFAULT(GETDATE()) |
| affiliate_id | INT | Foreign key to the Affiliate table | FK, NULL |
| affiliate_code | NVARCHAR(20) | Affiliate code used (if any) | NULL |

### SubOrder Table
Stores seller-specific portions of an order.

**Table Name:** `SubOrder`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| suborder_id | INT IDENTITY(1,1) | Primary key | PK |
| order_id | INT | Foreign key to the Order table | FK, NOT NULL |
| seller_id | INT | Foreign key to the Seller table | FK, NOT NULL |
| subtotal | DECIMAL(10, 2) | Subtotal for this seller's items | NOT NULL |
| status | NVARCHAR(20) | Processing status of the sub-order | DEFAULT('pending'), CHECK(status IN ('pending', 'approved', 'rejected', 'processing', 'shipped', 'delivered', 'cancelled')) |
| status_updated_at | DATETIME2 | When the status was last updated | DEFAULT(GETDATE()) |
| tracking_number | NVARCHAR(100) | Shipping tracking number | NULL |
| shipping_provider | NVARCHAR(100) | Shipping carrier or service | NULL |
| return_reason | NVARCHAR(MAX) | Reason for return (if applicable) | NULL |
| return_requested_at | DATETIME2 | When return was requested | NULL |
| return_approved_at | DATETIME2 | When return was approved | NULL |
| return_received_at | DATETIME2 | When returned items were received | NULL |
| refund_amount | DECIMAL(10, 2) | Amount refunded | NULL |
| refunded_at | DATETIME2 | When refund was processed | NULL |

### OrderItem Table
Stores individual items within a sub-order.

**Table Name:** `OrderItem`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| order_item_id | INT IDENTITY(1,1) | Primary key | PK |
| suborder_id | INT | Foreign key to the SubOrder table | FK, NOT NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| quantity | INT | Quantity ordered | NOT NULL |
| price_at_purchase | DECIMAL(10, 2) | Price at time of purchase | NOT NULL |
| total_price | DECIMAL(10, 2) | Total price (quantity × price) | NOT NULL |

## Review and Rating System

### Rating Table
Stores customer reviews and ratings for products.

**Table Name:** `Rating`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| rating_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NOT NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| stars | INT | Rating from 1-5 stars | NOT NULL, CHECK(stars BETWEEN 1 AND 5) |
| comment | NVARCHAR(MAX) | Review text | NULL |
| created_at | DATETIME2 | When the review was created | DEFAULT(GETDATE()) |
| is_verified_purchase | BIT | Whether from a verified purchaser | DEFAULT(0) |
| helpful_count | INT | Number of helpful votes | DEFAULT(0) |
|  |  |  | UNIQUE(customer_id, product_id) |

### ReviewImage Table
Stores images attached to customer reviews.

**Table Name:** `ReviewImage`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| review_image_id | INT IDENTITY(1,1) | Primary key | PK |
| rating_id | INT | Foreign key to the Rating table | FK, NOT NULL |
| image_url | NVARCHAR(255) | URL to the review image | NOT NULL |

### HelpfulRating Table
Tracks customer votes on the helpfulness of reviews.

**Table Name:** `HelpfulRating`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| helpful_id | INT IDENTITY(1,1) | Primary key | PK |
| rating_id | INT | Foreign key to the Rating table | FK, NOT NULL |
| customer_id | INT | Foreign key to the Customer table | FK, NOT NULL |
| is_helpful | BIT | Whether the customer found the review helpful | NOT NULL |
|  |  |  | UNIQUE(rating_id, customer_id) |

## Promotional System

### Coupon Table
Stores discount coupons.

**Table Name:** `Coupon`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| coupon_id | INT IDENTITY(1,1) | Primary key | PK |
| code | NVARCHAR(50) | Unique coupon code | NOT NULL, UNIQUE |
| description | NVARCHAR(MAX) | Description of the coupon | NULL |
| discount_amount | DECIMAL(10, 2) | Discount amount or percentage | NOT NULL |
| minimum_purchase | DECIMAL(10, 2) | Minimum order value required | DEFAULT(0) |
| discount_type | NVARCHAR(10) | Type of discount | NOT NULL, CHECK(discount_type IN ('percentage', 'fixed')) |
| start_date | DATETIME2 | When the coupon becomes valid | NOT NULL |
| end_date | DATETIME2 | When the coupon expires | NOT NULL |
| is_active | BIT | Whether the coupon is active | DEFAULT(1) |
| usage_limit | INT | Maximum number of uses allowed | NULL |
| usage_count | INT | Current number of uses | DEFAULT(0) |

### UserCoupon Table
Tracks coupon assignments to specific users.

**Table Name:** `UserCoupon`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| user_coupon_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NOT NULL |
| coupon_id | INT | Foreign key to the Coupon table | FK, NOT NULL |
| is_used | BIT | Whether the coupon has been used | DEFAULT(0) |
| assigned_at | DATETIME2 | When the coupon was assigned | DEFAULT(GETDATE()) |
| used_at | DATETIME2 | When the coupon was used | NULL |
|  |  |  | UNIQUE(customer_id, coupon_id) |

## Affiliate System

### Affiliate Table
Stores information about affiliates who promote products.

**Table Name:** `Affiliate`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| affiliate_id | INT IDENTITY(1,1) | Primary key | PK |
| user_id | INT | Foreign key to the User table | FK, NOT NULL |
| affiliate_code | NVARCHAR(20) | Unique affiliate tracking code | NOT NULL, UNIQUE |
| commission_rate | DECIMAL(5, 2) | Default commission percentage | DEFAULT(5.00) |
| total_earnings | DECIMAL(10, 2) | Lifetime earnings | DEFAULT(0) |
| available_balance | DECIMAL(10, 2) | Current available balance | DEFAULT(0) |
| withdrawn_amount | DECIMAL(10, 2) | Total amount withdrawn | DEFAULT(0) |
| created_at | DATETIME2 | When the affiliate account was created | DEFAULT(GETDATE()) |
| is_active | BIT | Whether the affiliate account is active | DEFAULT(1) |

### AffiliateSellerRelationship Table
Tracks relationships between affiliates and sellers.

**Table Name:** `AffiliateSellerRelationship`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| relationship_id | INT IDENTITY(1,1) | Primary key | PK |
| affiliate_id | INT | Foreign key to the Affiliate table | FK, NOT NULL |
| seller_id | INT | Foreign key to the Seller table | FK, NOT NULL |
| commission_rate | DECIMAL(5, 2) | Custom commission rate for this relationship | NULL |
| status | NVARCHAR(20) | Status of the relationship | DEFAULT('pending'), CHECK(status IN ('pending', 'approved', 'rejected', 'terminated')) |
| created_at | DATETIME2 | When the relationship was created | DEFAULT(GETDATE()) |
| updated_at | DATETIME2 | When the relationship was last updated | DEFAULT(GETDATE()) |
|  |  |  | UNIQUE(affiliate_id, seller_id) |

### AffiliateCommission Table
Tracks commissions earned by affiliates.

**Table Name:** `AffiliateCommission`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| commission_id | INT IDENTITY(1,1) | Primary key | PK |
| affiliate_id | INT | Foreign key to the Affiliate table | FK, NOT NULL |
| order_id | INT | Foreign key to the Order table | FK, NOT NULL |
| suborder_id | INT | Foreign key to the SubOrder table | FK, NOT NULL |
| seller_id | INT | Foreign key to the Seller table | FK, NOT NULL |
| order_item_id | INT | Foreign key to the OrderItem table | FK, NOT NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| commission_amount | DECIMAL(10, 2) | Commission amount earned | NOT NULL |
| commission_rate | DECIMAL(5, 2) | Commission rate applied | NOT NULL |
| order_item_total | DECIMAL(10, 2) | Total order item amount | NOT NULL |
| status | NVARCHAR(20) | Status of the commission | DEFAULT('pending'), CHECK(status IN ('pending', 'approved', 'rejected', 'paid')) |
| created_at | DATETIME2 | When the commission record was created | DEFAULT(GETDATE()) |
| updated_at | DATETIME2 | When the commission record was last updated | DEFAULT(GETDATE()) |
| paid_at | DATETIME2 | When the commission was paid | NULL |

### AffiliateWithdrawal Table
Tracks withdrawal requests from affiliates.

**Table Name:** `AffiliateWithdrawal`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| withdrawal_id | INT IDENTITY(1,1) | Primary key | PK |
| affiliate_id | INT | Foreign key to the Affiliate table | FK, NOT NULL |
| amount | DECIMAL(10, 2) | Amount to withdraw | NOT NULL |
| payment_method | NVARCHAR(50) | Method of payment | NOT NULL |
| payment_details | NVARCHAR(MAX) | Payment details (e.g., account number) | NOT NULL |
| status | NVARCHAR(20) | Status of the withdrawal | DEFAULT('pending'), CHECK(status IN ('pending', 'processing', 'completed', 'rejected')) |
| requested_at | DATETIME2 | When the withdrawal was requested | DEFAULT(GETDATE()) |
| processed_at | DATETIME2 | When the withdrawal was processed | NULL |
| notes | NVARCHAR(MAX) | Additional notes about the withdrawal | NULL |

## Recommendation System

### SearchHistory Table
Tracks customer search queries.

**Table Name:** `SearchHistory`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| search_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NULL |
| session_id | NVARCHAR(255) | Session identifier for non-logged-in users | NULL |
| search_query | NVARCHAR(500) | Search query text | NOT NULL |
| search_time | DATETIME2 | When the search was performed | DEFAULT(GETDATE()) |
| result_count | INT | Number of results returned | NULL |
| category_id | INT | Category filter applied | FK, NULL |
| subcategory_id | INT | Subcategory filter applied | FK, NULL |
| filters | NVARCHAR(MAX) | JSON-formatted applied filters | NULL |

### SearchResultClick Table
Tracks which search results customers click on.

**Table Name:** `SearchResultClick`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| click_id | INT IDENTITY(1,1) | Primary key | PK |
| search_id | INT | Foreign key to the SearchHistory table | FK, NOT NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| click_time | DATETIME2 | When the click occurred | DEFAULT(GETDATE()) |
| position_in_results | INT | Position of the product in search results | NULL |

### UserProductInteraction Table
Tracks all forms of user engagement with products.

**Table Name:** `UserProductInteraction`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| interaction_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NULL |
| session_id | NVARCHAR(255) | Session identifier for non-logged-in users | NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| interaction_type | NVARCHAR(50) | Type of interaction | NOT NULL |
| interaction_time | DATETIME2 | When the interaction occurred | DEFAULT(GETDATE()) |
| duration_seconds | INT | Duration of interaction (e.g., for views) | NULL |
| interaction_data | NVARCHAR(MAX) | Additional JSON-formatted details | NULL |

### ProductRecommendation Table
Stores pre-computed product-to-product recommendations.

**Table Name:** `ProductRecommendation`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| recommendation_id | INT IDENTITY(1,1) | Primary key | PK |
| source_product_id | INT | Foreign key to the source Product | FK, NOT NULL |
| recommended_product_id | INT | Foreign key to the recommended Product | FK, NOT NULL |
| recommendation_type | NVARCHAR(50) | Type of recommendation | NOT NULL |
| score | DECIMAL(10, 4) | Strength of the recommendation | NOT NULL |
| last_updated | DATETIME2 | When the recommendation was last calculated | DEFAULT(GETDATE()) |
|  |  |  | UNIQUE(source_product_id, recommended_product_id, recommendation_type) |

### UserRecommendation Table
Stores personalized product recommendations for users.

**Table Name:** `UserRecommendation`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| user_recommendation_id | INT IDENTITY(1,1) | Primary key | PK |
| customer_id | INT | Foreign key to the Customer table | FK, NOT NULL |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| recommendation_type | NVARCHAR(50) | Type of recommendation | NOT NULL |
| score | DECIMAL(10, 4) | Relevance score | NOT NULL |
| last_updated | DATETIME2 | When the recommendation was last calculated | DEFAULT(GETDATE()) |
|  |  |  | UNIQUE(customer_id, product_id, recommendation_type) |

### TrendingProduct Table
Caches trending products by category or overall.

**Table Name:** `TrendingProduct`

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| trending_id | INT IDENTITY(1,1) | Primary key | PK |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| category_id | INT | Foreign key to Category | FK, NULL |
| subcategory_id | INT | Foreign key to SubCategory | FK, NULL |
| trend_score | DECIMAL(10, 4) | Trending score | NOT NULL |
| time_period | NVARCHAR(20) | Period of trend calculation | NOT NULL |
| last_updated | DATETIME2 | When the trending calculation was last updated | DEFAULT(GETDATE()) |
|  |  |  | UNIQUE(product_id, category_id, subcategory_id, time_period) |

## Analytics System

### ProductView Table

| Column | Type | Description | Constraints |
|--------|------|-------------|------------|
| view_id | INT IDENTITY(1,1) | Primary key | PK |
| product_id | INT | Foreign key to the Product table | FK, NOT NULL |
| customer_id | INT | Foreign key to the Customer table | FK, NULL |
| session_id | NVARCHAR(255) | Session identifier for non-logged-in users | NULL |
| viewed_at | DATETIME2 | When the view occurred | DEFAULT(GETDATE()) |

## Database Indexes

The following indexes have been created to optimize query performance:

### Order Related Indexes
- `idx_order_customer` on `Order(customer_id)` - Improves performance when looking up a customer's orders

### Product Related Indexes
- `idx_product_category` on `Product(subcategory_id)` - Speeds up filtering products by subcategory
- `idx_product_search` on `Product(name, is_available, approval_status)` - Optimizes product search queries
- `idx_product_seller` on `Product(seller_id)` - Improves performance when looking up a seller's products
- `idx_product_view_analytics` on `ProductView(product_id, viewed_at)` - Optimizes product view analytics

### Recommendation Related Indexes
- `idx_product_recommendation_source` on `ProductRecommendation(source_product_id, score DESC)` - Optimizes retrieval of recommendations sorted by score
- `idx_user_recommendation` on `UserRecommendation(customer_id, score DESC)` - Optimizes retrieval of user-specific recommendations sorted by score
- `idx_trending_product` on `TrendingProduct(category_id, subcategory_id, trend_score DESC)` - Optimizes retrieval of trending products

### Rating Related Indexes
- `idx_rating_product` on `Rating(product_id)` - Speeds up retrieval of ratings for a specific product

### Search and User Interaction Indexes
- `idx_search_history_customer` on `SearchHistory(customer_id, search_time)` - Optimizes retrieval of a customer's search history
- `idx_search_history_session` on `SearchHistory(session_id, search_time)` - Optimizes retrieval of search history for a specific session
- `idx_search_result_click` on `SearchResultClick(search_id, click_time)` - Optimizes analysis of search result clicks
- `idx_user_product_interaction` on `UserProductInteraction(customer_id, product_id, interaction_type)` - Speeds up retrieval of specific interactions
- `idx_user_product_interaction_session` on `UserProductInteraction(session_id, interaction_time)` - Optimizes session-based interaction analysis

### Order Processing Indexes
- `idx_suborder_status` on `SubOrder(status)` - Optimizes filtering of sub-orders by status

## Stored Procedures

### create_order
This stored procedure handles the creation of a new order, including order splitting by seller and coupon application.

**Parameters:**
- `@p_customer_id` - Customer ID placing the order
- `@p_address_id` - Address ID for delivery
- `@p_coupon_id` - Optional coupon ID to apply
- `@p_payment_method` - Selected payment method

**Functionality:**
1. Calculates the total order amount from the customer's cart
2. Applies any coupon discount if applicable
3. Calculates tax and final amount
4. Creates the main order record
5. Splits the order into sub-orders by seller
6. Creates order items for each product in the cart
7. Clears the customer's cart
8. Returns the newly created order ID

### update_trending_products
This stored procedure calculates and updates trending products based on user interactions.

**Parameters:**
- `@time_period` - The time period to calculate trends for (daily, weekly, monthly)

**Functionality:**
1. Sets a date threshold based on the specified time period
2. Clears existing trending products for the time period
3. Calculates trend scores based on product interactions (views, cart adds, purchases)
4. Inserts overall trending products (top 100)
5. Inserts category-specific trending products (top 20 per category)
6. Inserts subcategory-specific trending products (top 20 per subcategory)

## Recommended Database Extensions

The following tables should be added to handle order returns properly:

### ReturnRequest Table (Recommended Addition)
```sql
CREATE TABLE ReturnRequest (
    return_id INT IDENTITY(1,1) PRIMARY KEY,
    suborder_id INT NOT NULL,
    customer_id INT NOT NULL,
    return_reason NVARCHAR(MAX) NOT NULL,
    return_status NVARCHAR(20) DEFAULT 'requested' CHECK (return_status IN ('requested', 'approved', 'rejected', 'shipped_back', 'received', 'refunded')),
    requested_at DATETIME2 DEFAULT GETDATE(),
    approved_at DATETIME2 NULL,
    received_at DATETIME2 NULL,
    refund_amount DECIMAL(10, 2) NULL,
    refunded_at DATETIME2 NULL,
    tracking_number NVARCHAR(100) NULL,
    comments NVARCHAR(MAX) NULL,
    FOREIGN KEY (suborder_id) REFERENCES SubOrder(suborder_id),
    FOREIGN KEY (customer_id) REFERENCES Customer(customer_id)
);
```

### ReturnItem Table (Recommended Addition)
```sql
CREATE TABLE ReturnItem (
    return_item_id INT IDENTITY(1,1) PRIMARY KEY,
    return_id INT NOT NULL,
    order_item_id INT NOT NULL,
    quantity INT NOT NULL,
    return_reason NVARCHAR(MAX) NULL,
    condition NVARCHAR(50) NULL,
    refund_amount DECIMAL(10, 2) NULL,
    FOREIGN KEY (return_id) REFERENCES ReturnRequest(return_id) ON DELETE CASCADE,
    FOREIGN KEY (order_item_id) REFERENCES OrderItem(order_item_id)
);
```

These tables would provide a more comprehensive return management system, allowing for partial returns and detailed tracking of the return process.

## Cart and Order Item Variant Support (Recommended Addition)

Currently, CartItem and OrderItem do not have support for tracking specific product variants. It's recommended to add variant support with these ALTER TABLE statements:

```sql
ALTER TABLE CartItem
ADD variant_id INT NULL,
FOREIGN KEY (variant_id) REFERENCES ProductVariant(variant_id);

ALTER TABLE OrderItem
ADD variant_id INT NULL,
FOREIGN KEY (variant_id) REFERENCES ProductVariant(variant_id);
```

## Entity Relationship Overview

The database schema follows these key relationships:

1. **User Management**:
   - One User can be one Customer, Seller, or Admin (one-to-one)
   - One User can have many Addresses (one-to-many)

2. **Product Organization**:
   - One Category can have many SubCategories (one-to-many)
   - One SubCategory can have many Products (one-to-many)
   - One SubCategory can define many ProductAttributes (one-to-many)
   - One Product can have many ProductVariants (one-to-many)
   - One Product can have many ProductImages (one-to-many)
   - One Product can have many ProductAttributeValues (one-to-many)

3. **Shopping Experience**:
   - One Customer can have one Cart (one-to-one)
   - One Cart can have many CartItems (one-to-many)
   - One Customer can have one Wishlist (one-to-one)
   - One Wishlist can have many WishlistItems (one-to-many)

4. **Order Processing**:
   - One Customer can place many Orders (one-to-many)
   - One Order can have many SubOrders (one-to-many)
   - One SubOrder belongs to one Seller (many-to-one)
   - One SubOrder can have many OrderItems (one-to-many)

5. **Reviews and Ratings**:
   - One Customer can leave many Ratings (one-to-many)
   - One Product can receive many Ratings (one-to-many)
   - One Rating can have many ReviewImages (one-to-many)
   - One Rating can receive many HelpfulRatings (one-to-many)

6. **Affiliate System**:
   - One User can be one Affiliate (one-to-one)
   - One Affiliate can have relationships with many Sellers (one-to-many)
   - One Affiliate can earn many Commissions (one-to-many)
   - One Affiliate can request many Withdrawals (one-to-many)

7. **Recommendation System**:
   - One Product can have many Recommendations (one-to-many)
   - One Customer can have many personalized Recommendations (one-to-many)
   - One Product can be trending in many Categories/SubCategories (one-to-many)

## Common Database Queries

### 1. Retrieve Product Details with Variants and Attributes

```sql
SELECT 
    p.product_id, p.name, p.description, p.base_price, p.discount_percentage,
    p.average_rating, p.stock_quantity, p.main_image_url,
    s.seller_id, s.business_name,
    sc.subcategory_id, sc.name AS subcategory_name,
    c.category_id, c.name AS category_name,
    pv.variant_id, pv.variant_name, pv.price, pv.stock_quantity AS variant_stock
FROM Product p
JOIN Seller s ON p.seller_id = s.seller_id
JOIN SubCategory sc ON p.subcategory_id = sc.subcategory_id
JOIN Category c ON sc.category_id = c.category_id
LEFT JOIN ProductVariant pv ON p.product_id = pv.product_id
WHERE p.product_id = @product_id
```

### 2. Filter Products by Category with Pagination

```sql
SELECT 
    p.product_id, p.name, p.base_price, p.discount_percentage,
    p.average_rating, p.main_image_url
FROM Product p
JOIN SubCategory sc ON p.subcategory_id = sc.subcategory_id
WHERE sc.category_id = @category_id
AND p.is_available = 1
AND p.approval_status = 'approved'
ORDER BY p.created_at DESC
OFFSET (@page_number - 1) * @page_size ROWS
FETCH NEXT @page_size ROWS ONLY
```

### 3. Search Products with Filtering

```sql
SELECT 
    p.product_id, p.name, p.base_price, p.discount_percentage,
    p.average_rating, p.main_image_url
FROM Product p
JOIN SubCategory sc ON p.subcategory_id = sc.subcategory_id
JOIN Category c ON sc.category_id = c.category_id
WHERE p.is_available = 1
AND p.approval_status = 'approved'
AND (
    p.name LIKE '%' + @search_term + '%'
    OR p.description LIKE '%' + @search_term + '%'
    OR c.name LIKE '%' + @search_term + '%'
    OR sc.name LIKE '%' + @search_term + '%'
)
AND (@min_price IS NULL OR p.base_price * (1 - p.discount_percentage/100) >= @min_price)
AND (@max_price IS NULL OR p.base_price * (1 - p.discount_percentage/100) <= @max_price)
AND (@category_id IS NULL OR c.category_id = @category_id)
ORDER BY 
    CASE WHEN @sort_by = 'price_asc' THEN p.base_price * (1 - p.discount_percentage/100) END ASC,
    CASE WHEN @sort_by = 'price_desc' THEN p.base_price * (1 - p.discount_percentage/100) END DESC,
    CASE WHEN @sort_by = 'rating' THEN p.average_rating END DESC,
    CASE WHEN @sort_by = 'newest' THEN p.created_at END DESC,
    CASE WHEN @sort_by IS NULL THEN p.average_rating END DESC
OFFSET (@page_number - 1) * @page_size ROWS
FETCH NEXT @page_size ROWS ONLY
```

### 4. Get Customer Order History

```sql
SELECT 
    o.order_id, o.created_at, o.final_amount, o.payment_status,
    so.suborder_id, so.status AS suborder_status, so.tracking_number,
    s.business_name AS seller_name,
    COUNT(oi.order_item_id) AS item_count
FROM [Order] o
JOIN SubOrder so ON o.order_id = so.order_id
JOIN Seller s ON so.seller_id = s.seller_id
JOIN OrderItem oi ON so.suborder_id = oi.suborder_id
WHERE o.customer_id = @customer_id
GROUP BY o.order_id, o.created_at, o.final_amount, o.payment_status,
    so.suborder_id, so.status, so.tracking_number, s.business_name
ORDER BY o.created_at DESC
```

### 5. Get Cart with Items and Totals

```sql
SELECT 
    c.cart_id,
    ci.cart_item_id, ci.product_id, ci.quantity, ci.price_at_addition,
    p.name AS product_name, p.main_image_url,
    pv.variant_id, pv.variant_name,
    s.seller_id, s.business_name,
    SUM(ci.quantity * ci.price_at_addition) OVER() AS cart_total
FROM Cart c
JOIN CartItem ci ON c.cart_id = ci.cart_id
JOIN Product p ON ci.product_id = p.product_id
JOIN Seller s ON p.seller_id = s.seller_id
LEFT JOIN ProductVariant pv ON ci.variant_id = pv.variant_id
WHERE c.customer_id = @customer_id
```

### 6. Get Seller Products with Inventory

```sql
SELECT 
    p.product_id, p.name, p.base_price, p.discount_percentage,
    p.stock_quantity, p.approval_status, p.average_rating,
    sc.name AS subcategory_name,
    c.name AS category_name,
    COUNT(pv.variant_id) AS variant_count,
    SUM(pv.stock_quantity) AS total_variant_stock
FROM Product p
JOIN SubCategory sc ON p.subcategory_id = sc.subcategory_id
JOIN Category c ON sc.category_id = c.category_id
LEFT JOIN ProductVariant pv ON p.product_id = pv.product_id
WHERE p.seller_id = @seller_id
GROUP BY p.product_id, p.name, p.base_price, p.discount_percentage,
    p.stock_quantity, p.approval_status, p.average_rating,
    sc.name, c.name
ORDER BY p.created_at DESC
```

### 7. Get Personalized Product Recommendations

```sql
SELECT 
    p.product_id, p.name, p.base_price, p.discount_percentage,
    p.average_rating, p.main_image_url,
    ur.recommendation_type, ur.score
FROM UserRecommendation ur
JOIN Product p ON ur.product_id = p.product_id
WHERE ur.customer_id = @customer_id
AND p.is_available = 1
AND p.approval_status = 'approved'
ORDER BY ur.score DESC
OFFSET (@page_number - 1) * @page_size ROWS
FETCH NEXT @page_size ROWS ONLY
```

### 8. Get Affiliate Performance Summary

```sql
SELECT 
    ac.affiliate_id,
    COUNT(DISTINCT ac.order_id) AS total_orders,
    SUM(ac.commission_amount) AS total_commission,
    COUNT(CASE WHEN ac.status = 'paid' THEN 1 END) AS paid_commissions,
    SUM(CASE WHEN ac.status = 'paid' THEN ac.commission_amount ELSE 0 END) AS paid_amount,
    COUNT(CASE WHEN ac.status = 'pending' THEN 1 END) AS pending_commissions,
    SUM(CASE WHEN ac.status = 'pending' THEN ac.commission_amount ELSE 0 END) AS pending_amount,
    AVG(ac.commission_rate) AS average_commission_rate
FROM AffiliateCommission ac
WHERE ac.affiliate_id = @affiliate_id
AND ac.created_at BETWEEN @start_date AND @end_date
GROUP BY ac.affiliate_id
```

## Security Considerations

1. **Authentication**:
   - Password hashes are stored in the User table rather than plain text
   - The system should use strong hashing algorithms (e.g., bcrypt, Argon2)

2. **Authorization**:
   - Role-based access is defined through the user_type field
   - Seller actions should be restricted to their own products and orders
   - Admin permissions can be customized through the permissions field

3. **Data Protection**:
   - Payment details should be encrypted or tokenized
   - Personal data should be secured according to relevant regulations (GDPR, etc.)

4. **Audit Logging**:
   - Consider adding audit logging for sensitive operations
   - Track user authentication attempts and administrative actions

## Performance Optimization

1. **Indexing**:
   - All foreign keys are indexed for join performance
   - Additional indexes exist on frequently filtered or sorted columns
   - Compound indexes help optimize common query patterns

2. **Pagination**:
   - All list endpoints support pagination to handle large datasets
   - Offset/fetch or limit/offset patterns are used

3. **Denormalization**:
   - Some calculated values (e.g., average_rating) are stored redundantly for performance
   - TrendingProduct provides a cache of pre-calculated trending items

4. **Stored Procedures**:
   - Complex operations like order creation are encapsulated in stored procedures
   - This minimizes roundtrips and ensures consistent business logic

## Scalability Considerations

1. **Sharding Opportunities**:
   - The database could be sharded by seller or category for horizontal scaling
   - Historical orders could be moved to archive storage after completion

2. **Partitioning**:
   - Large tables like OrderItem and ProductView could be partitioned by date
   - This would improve performance for recent data access

3. **Caching Strategy**:
   - Product details and category hierarchies are good candidates for caching
   - Read-heavy operations should utilize application-level caching
