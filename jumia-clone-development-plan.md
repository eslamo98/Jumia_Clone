# Jumia Clone Development Plan & API Specifications

## Project Overview
This development plan outlines the implementation of the Jumia clone e-commerce platform, including backend API endpoints, frontend components, and testing strategies. The plan is organized by feature areas and includes detailed specifications for each API endpoint.

## Development Timeline

### Phase 1: Core Infrastructure (Week 1)
- Setup project structure and environment
- Implement authentication system
- Create basic user management
- Set up database connections and repositories

### Phase 2: Product Catalog (Week 1-2)
- Implement category and subcategory management
- Create product listing and detail endpoints
- Develop search and filtering functionality
- Add product attribute management

### Phase 3: Shopping Experience (Week 2)
- Implement cart functionality
- Create wishlist management
- Develop product variant handling
- Add user profile and address management

### Phase 4: Order Processing (Week 3)
- Implement checkout process
- Create order management system
- Develop order splitting by seller
- Add payment integration (mock)

### Phase 5: Seller Dashboard (Week 3-4)
- Create seller product management
- Implement order fulfillment
- Add inventory management
- Develop seller analytics

### Phase 6: Reviews & Ratings (Week 4)
- Implement product reviews and ratings
- Add helpful vote functionality
- Create review management

### Phase 7: Advanced Features (Week 4-5)
- Implement affiliate system
- Add recommendation engine
- Develop coupons and promotions
- Create analytics dashboards

## API Endpoints Specification

### Authentication APIs

#### 1. Register User
- **Endpoint:** `POST /api/auth/register`
- **Description:** Register a new user
- **Request Body:**
  ```json
  {
    "email": "string",
    "password": "string",
    "first_name": "string",
    "last_name": "string",
    "phone_number": "string",
    "user_type": "customer|seller"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Registration successful",
    "data": {
      "user_id": "integer",
      "email": "string",
      "first_name": "string",
      "last_name": "string",
      "token": "string"
    }
  }
  ```
- **Notes:** Creates a new user and returns a JWT token for immediate authentication. If registering as a seller, additional business information will be required.

#### 2. Login User
- **Endpoint:** `POST /api/auth/login`
- **Description:** Authenticate user and generate token
- **Request Body:**
  ```json
  {
    "email": "string",
    "password": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Login successful",
    "data": {
      "user_id": "integer",
      "email": "string",
      "first_name": "string",
      "last_name": "string",
      "user_type": "string",
      "token": "string",
      "refresh_token": "string"
    }
  }
  ```
- **Notes:** Validates credentials and returns JWT token and refresh token for authentication.

#### 3. Refresh Token
- **Endpoint:** `POST /api/auth/refresh-token`
- **Description:** Get a new access token using refresh token
- **Request Body:**
  ```json
  {
    "refresh_token": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "token": "string",
      "refresh_token": "string"
    }
  }
  ```
- **Notes:** Uses the refresh token to generate a new access token without requiring re-login.

#### 4. Change Password
- **Endpoint:** `PUT /api/auth/change-password`
- **Description:** Change user password
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "current_password": "string",
    "new_password": "string",
    "confirm_password": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Password changed successfully"
  }
  ```
- **Notes:** Validates current password before allowing password change.

### User Management APIs

#### 5. Get User Profile
- **Endpoint:** `GET /api/users/profile`
- **Description:** Get current user's profile information
- **Authorization:** Required
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "user_id": "integer",
      "email": "string",
      "first_name": "string",
      "last_name": "string",
      "phone_number": "string",
      "user_type": "string",
      "created_at": "datetime",
      "customer": { /* Customer details if applicable */ },
      "seller": { /* Seller details if applicable */ }
    }
  }
  ```
- **Notes:** Returns profile information based on authenticated user's token.

#### 6. Update User Profile
- **Endpoint:** `PUT /api/users/profile`
- **Description:** Update user profile information
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "first_name": "string",
    "last_name": "string",
    "phone_number": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Profile updated successfully",
    "data": {
      "user_id": "integer",
      "first_name": "string",
      "last_name": "string",
      "phone_number": "string",
      "updated_at": "datetime"
    }
  }
  ```
- **Notes:** Updates basic profile information. For user type specific information, use specialized endpoints.

#### 7. Get User Addresses
- **Endpoint:** `GET /api/users/addresses`
- **Description:** Get all addresses for the current user
- **Authorization:** Required
- **Response:**
  ```json
  {
    "success": true,
    "data": [
      {
        "address_id": "integer",
        "street_address": "string",
        "city": "string",
        "state": "string",
        "postal_code": "string",
        "country": "string",
        "phone_number": "string",
        "is_default": "boolean",
        "address_name": "string"
      }
    ]
  }
  ```
- **Notes:** Returns all saved addresses for the authenticated user.

#### 8. Add New Address
- **Endpoint:** `POST /api/users/addresses`
- **Description:** Add a new address for the user
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "street_address": "string",
    "city": "string",
    "state": "string",
    "postal_code": "string",
    "country": "string",
    "phone_number": "string",
    "is_default": "boolean",
    "address_name": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Address added successfully",
    "data": {
      "address_id": "integer",
      "street_address": "string",
      "city": "string",
      "state": "string",
      "postal_code": "string",
      "country": "string",
      "phone_number": "string",
      "is_default": "boolean",
      "address_name": "string"
    }
  }
  ```
- **Notes:** If is_default is true, all other addresses for this user will have is_default set to false.

#### 9. Update Address
- **Endpoint:** `PUT /api/users/addresses/{id}`
- **Description:** Update an existing address
- **Authorization:** Required
- **Path Parameters:** `id` - Address ID
- **Request Body:**
  ```json
  {
    "street_address": "string",
    "city": "string",
    "state": "string",
    "postal_code": "string",
    "country": "string",
    "phone_number": "string",
    "is_default": "boolean",
    "address_name": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Address updated successfully",
    "data": {
      "address_id": "integer",
      "street_address": "string",
      "city": "string",
      "state": "string",
      "postal_code": "string",
      "country": "string",
      "phone_number": "string",
      "is_default": "boolean",
      "address_name": "string"
    }
  }
  ```
- **Notes:** User can only update their own addresses. If is_default is set to true, all other addresses will have is_default set to false.

#### 10. Delete Address
- **Endpoint:** `DELETE /api/users/addresses/{id}`
- **Description:** Delete an address
- **Authorization:** Required
- **Path Parameters:** `id` - Address ID
- **Response:**
  ```json
  {
    "success": true,
    "message": "Address deleted successfully"
  }
  ```
- **Notes:** Cannot delete an address that is currently being used in an active order.

### Category and Subcategory APIs

#### 11. Get All Categories
- **Endpoint:** `GET /api/categories`
- **Description:** Get all product categories
- **Query Parameters:**
  - `include_inactive` (optional): Include inactive categories (admin only)
- **Response:**
  ```json
  {
    "success": true,
    "data": [
      {
        "category_id": "integer",
        "name": "string",
        "description": "string",
        "image_url": "string",
        "is_active": "boolean",
        "subcategory_count": "integer"
      }
    ]
  }
  ```
- **Notes:** Returns all active categories by default. Admins can view inactive categories by setting include_inactive=true.

#### 12. Get Category by ID
- **Endpoint:** `GET /api/categories/{id}`
- **Description:** Get a specific category by ID
- **Path Parameters:** `id` - Category ID
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "category_id": "integer",
      "name": "string",
      "description": "string",
      "image_url": "string",
      "is_active": "boolean"
    }
  }
  ```
- **Notes:** Returns a single category with detailed information.

#### 13. Get Subcategories by Category
- **Endpoint:** `GET /api/categories/{id}/subcategories`
- **Description:** Get all subcategories for a specific category
- **Path Parameters:** `id` - Category ID
- **Query Parameters:**
  - `include_inactive` (optional): Include inactive subcategories (admin only)
- **Response:**
  ```json
  {
    "success": true,
    "data": [
      {
        "subcategory_id": "integer",
        "name": "string",
        "description": "string",
        "image_url": "string",
        "is_active": "boolean",
        "category_id": "integer",
        "product_count": "integer"
      }
    ]
  }
  ```
- **Notes:** Returns all active subcategories for the specified category by default.

#### 14. Create Category (Admin)
- **Endpoint:** `POST /api/admin/categories`
- **Description:** Create a new category
- **Authorization:** Admin required
- **Request Body:**
  ```json
  {
    "name": "string",
    "description": "string",
    "image_url": "string",
    "is_active": "boolean"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Category created successfully",
    "data": {
      "category_id": "integer",
      "name": "string",
      "description": "string",
      "image_url": "string",
      "is_active": "boolean"
    }
  }
  ```
- **Notes:** Accessible only to admin users.

#### 15. Update Category (Admin)
- **Endpoint:** `PUT /api/admin/categories/{id}`
- **Description:** Update an existing category
- **Authorization:** Admin required
- **Path Parameters:** `id` - Category ID
- **Request Body:**
  ```json
  {
    "name": "string",
    "description": "string",
    "image_url": "string",
    "is_active": "boolean"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Category updated successfully",
    "data": {
      "category_id": "integer",
      "name": "string",
      "description": "string",
      "image_url": "string",
      "is_active": "boolean"
    }
  }
  ```
- **Notes:** Accessible only to admin users.

#### 16. Create Subcategory (Admin)
- **Endpoint:** `POST /api/admin/subcategories`
- **Description:** Create a new subcategory
- **Authorization:** Admin required
- **Request Body:**
  ```json
  {
    "category_id": "integer",
    "name": "string",
    "description": "string",
    "image_url": "string",
    "is_active": "boolean"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Subcategory created successfully",
    "data": {
      "subcategory_id": "integer",
      "category_id": "integer",
      "name": "string",
      "description": "string",
      "image_url": "string",
      "is_active": "boolean"
    }
  }
  ```
- **Notes:** Accessible only to admin users.

### Product APIs

#### 17. Get All Products
- **Endpoint:** `GET /api/products`
- **Description:** Get products with filtering and pagination
- **Query Parameters:**
  - `category_id` (optional): Filter by category
  - `subcategory_id` (optional): Filter by subcategory
  - `seller_id` (optional): Filter by seller
  - `search` (optional): Search term for product name/description
  - `min_price` (optional): Minimum price filter
  - `max_price` (optional): Maximum price filter
  - `sort_by` (optional): Sort field (price_asc, price_desc, rating, newest)
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "product_id": "integer",
          "name": "string",
          "base_price": "decimal",
          "discount_percentage": "decimal",
          "final_price": "decimal",
          "average_rating": "float",
          "main_image_url": "string",
          "seller": {
            "seller_id": "integer",
            "business_name": "string"
          },
          "subcategory": {
            "subcategory_id": "integer",
            "name": "string"
          }
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer",
        "has_next_page": "boolean",
        "has_previous_page": "boolean"
      }
    }
  }
  ```
- **Notes:** Returns paginated product listings with basic information. For detailed product information, use the Get Product by ID endpoint.

#### 18. Get Product by ID
- **Endpoint:** `GET /api/products/{id}`
- **Description:** Get detailed information for a specific product
- **Path Parameters:** `id` - Product ID
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "product_id": "integer",
      "name": "string",
      "description": "string",
      "base_price": "decimal",
      "discount_percentage": "decimal",
      "final_price": "decimal",
      "is_available": "boolean",
      "stock_quantity": "integer",
      "main_image_url": "string",
      "average_rating": "float",
      "approval_status": "string",
      "created_at": "datetime",
      "updated_at": "datetime",
      "seller": {
        "seller_id": "integer",
        "business_name": "string",
        "rating": "float"
      },
      "subcategory": {
        "subcategory_id": "integer",
        "name": "string",
        "category": {
          "category_id": "integer",
          "name": "string"
        }
      },
      "variants": [
        {
          "variant_id": "integer",
          "variant_name": "string",
          "price": "decimal",
          "discount_percentage": "decimal",
          "final_price": "decimal",
          "stock_quantity": "integer",
          "is_available": "boolean",
          "is_default": "boolean",
          "variant_image_url": "string",
          "attributes": [
            {
              "attribute_name": "string",
              "attribute_value": "string"
            }
          ]
        }
      ],
      "images": [
        {
          "image_id": "integer",
          "image_url": "string",
          "display_order": "integer"
        }
      ],
      "attributes": [
        {
          "attribute_id": "integer",
          "name": "string",
          "value": "string"
        }
      ],
      "rating_summary": {
        "average": "float",
        "count": "integer",
        "distribution": {
          "5": "integer",
          "4": "integer",
          "3": "integer",
          "2": "integer",
          "1": "integer"
        }
      }
    }
  }
  ```
- **Notes:** Returns comprehensive product information including variants, images, attributes, and rating summary.

#### 19. Get Featured Products
- **Endpoint:** `GET /api/products/featured`
- **Description:** Get a list of featured products
- **Query Parameters:**
  - `limit` (optional): Number of products to return (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": [
      {
        "product_id": "integer",
        "name": "string",
        "base_price": "decimal",
        "discount_percentage": "decimal",
        "final_price": "decimal",
        "average_rating": "float",
        "main_image_url": "string",
        "seller": {
          "seller_id": "integer",
          "business_name": "string"
        }
      }
    ]
  }
  ```
- **Notes:** Returns a curated list of featured products. Implementation can be based on trending products, high ratings, etc.

#### 20. Search Products
- **Endpoint:** `GET /api/products/search`
- **Description:** Search products by keyword
- **Query Parameters:**
  - `query` (required): Search keyword
  - `category_id` (optional): Filter by category
  - `subcategory_id` (optional): Filter by subcategory
  - `min_price` (optional): Minimum price filter
  - `max_price` (optional): Maximum price filter
  - `sort_by` (optional): Sort field (price_asc, price_desc, rating, newest)
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "product_id": "integer",
          "name": "string",
          "base_price": "decimal",
          "discount_percentage": "decimal",
          "final_price": "decimal",
          "average_rating": "float",
          "main_image_url": "string",
          "seller": {
            "seller_id": "integer",
            "business_name": "string"
          },
          "subcategory": {
            "subcategory_id": "integer",
            "name": "string"
          }
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer",
        "has_next_page": "boolean",
        "has_previous_page": "boolean"
      }
    }
  }
  ```
- **Notes:** Searches product name, description, and relevant attributes. Also logs search queries to the SearchHistory table for analytics.

#### 21. Get Product Attributes by Subcategory
- **Endpoint:** `GET /api/subcategories/{id}/attributes`
- **Description:** Get attributes defined for a specific subcategory
- **Path Parameters:** `id` - Subcategory ID
- **Response:**
  ```json
  {
    "success": true,
    "data": [
      {
        "attribute_id": "integer",
        "name": "string",
        "type": "string",
        "possible_values": ["string"],
        "is_required": "boolean",
        "is_filterable": "boolean"
      }
    ]
  }
  ```
- **Notes:** Used for building product creation forms and for filtering products within a subcategory.

#### 22. Create Product (Seller)
- **Endpoint:** `POST /api/seller/products`
- **Description:** Create a new product
- **Authorization:** Seller required
- **Request Body:**
  ```json
  {
    "subcategory_id": "integer",
    "name": "string",
    "description": "string",
    "base_price": "decimal",
    "discount_percentage": "decimal",
    "stock_quantity": "integer",
    "main_image_url": "string",
    "attributes": [
      {
        "attribute_id": "integer",
        "value": "string"
      }
    ],
    "variants": [
      {
        "variant_name": "string",
        "price": "decimal",
        "discount_percentage": "decimal",
        "stock_quantity": "integer",
        "sku": "string",
        "variant_image_url": "string",
        "is_default": "boolean",
        "attributes": [
          {
            "attribute_name": "string",
            "attribute_value": "string"
          }
        ]
      }
    ],
    "additional_images": [
      {
        "image_url": "string",
        "display_order": "integer"
      }
    ]
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Product created successfully. Pending approval.",
    "data": {
      "product_id": "integer",
      "name": "string",
      "approval_status": "pending"
    }
  }
  ```
- **Notes:** Creates a product with pending approval status. Admin must approve before it becomes visible to customers.

#### 23. Update Product (Seller)
- **Endpoint:** `PUT /api/seller/products/{id}`
- **Description:** Update an existing product
- **Authorization:** Seller required
- **Path Parameters:** `id` - Product ID
- **Request Body:**
  ```json
  {
    "name": "string",
    "description": "string",
    "base_price": "decimal",
    "discount_percentage": "decimal",
    "stock_quantity": "integer",
    "main_image_url": "string",
    "attributes": [
      {
        "attribute_id": "integer",
        "value": "string"
      }
    ]
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Product updated successfully",
    "data": {
      "product_id": "integer",
      "name": "string",
      "updated_at": "datetime"
    }
  }
  ```
- **Notes:** Seller can only update their own products. Substantial changes may require re-approval by an admin.

#### 24. Add/Update Product Variant (Seller)
- **Endpoint:** `POST /api/seller/products/{id}/variants`
- **Description:** Add or update a product variant
- **Authorization:** Seller required
- **Path Parameters:** `id` - Product ID
- **Request Body:**
  ```json
  {
    "variant_id": "integer",  // Include for update, omit for create
    "variant_name": "string",
    "price": "decimal",
    "discount_percentage": "decimal",
    "stock_quantity": "integer",
    "sku": "string",
    "variant_image_url": "string",
    "is_default": "boolean",
    "attributes": [
      {
        "attribute_name": "string",
        "attribute_value": "string"
      }
    ]
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Variant added/updated successfully",
    "data": {
      "variant_id": "integer",
      "variant_name": "string",
      "price": "decimal"
    }
  }
  ```
- **Notes:** If a variant ID is provided, it updates that variant; otherwise, it creates a new one.

#### 25. Delete Product Variant (Seller)
- **Endpoint:** `DELETE /api/seller/products/{productId}/variants/{variantId}`
- **Description:** Delete a product variant
- **Authorization:** Seller required
- **Path Parameters:** 
  - `productId` - Product ID
  - `variantId` - Variant ID
- **Response:**
  ```json
  {
    "success": true,
    "message": "Variant deleted successfully"
  }
  ```
- **Notes:** Cannot delete the last variant of a product. If the variant is the default, another variant will be set as default.

#### 26. Approve/Reject Product (Admin)
- **Endpoint:** `PUT /api/admin/products/{id}/approval`
- **Description:** Approve or reject a product
- **Authorization:** Admin required
- **Path Parameters:** `id` - Product ID
- **Request Body:**
  ```json
  {
    "approval_status": "approved|rejected",
    "rejection_reason": "string"  // Required if rejecting
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Product approval status updated",
    "data": {
      "product_id": "integer",
      "name": "string",
      "approval_status": "string"
    }
  }
  ```
- **Notes:** When rejected, the seller receives notification with the rejection reason.

### Cart APIs

#### 27. Get User's Cart
- **Endpoint:** `GET /api/cart`
- **Description:** Get the current user's shopping cart
- **Authorization:** Required
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "cart_id": "integer",
      "items": [
        {
          "cart_item_id": "integer",
          "product": {
            "product_id": "integer",
            "name": "string",
            "main_image_url": "string",
            "seller": {
              "seller_id": "integer",
              "business_name": "string"
            }
          },
          "variant": {
            "variant_id": "integer",
            "variant_name": "string",
            "attributes": [
              {
                "attribute_name": "string",
                "attribute_value": "string"
              }
            ]
          },
          "quantity": "integer",
          "price": "decimal",
          "total": "decimal"
        }
      ],
      "summary": {
        "subtotal": "decimal",
        "total_items": "integer",
        "seller_count": "integer"
      }
    }
  }
  ```
- **Notes:** If the user has no cart, an empty cart will be created and returned.

#### 28. Add Item to Cart
- **Endpoint:** `POST /api/cart/items`
- **Description:** Add an item to the shopping cart
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "product_id": "integer",
    "variant_id": "integer",  // Optional - if not provided, main product is added
    "quantity": "integer"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Item added to cart",
    "data": {
      "cart_item_id": "integer",
      "product_id": "integer",
      "variant_id": "integer",
      "quantity": "integer",
      "price": "decimal",
      "cart_summary": {
        "subtotal": "decimal",
        "total_items": "integer"
      }
    }
  }
  ```
- **Notes:** If the product is already in the cart, the quantity will be updated.

#### 29. Update Cart Item
- **Endpoint:** `PUT /api/cart/items/{id}`
- **Description:** Update the quantity of an item in the cart
- **Authorization:** Required
- **Path Parameters:** `id` - Cart Item ID
- **Request Body:**
  ```json
  {
    "quantity": "integer"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Cart item updated",
    "data": {
      "cart_item_id": "integer",
      "quantity": "integer",
      "total": "decimal",
      "cart_summary": {
        "subtotal": "decimal",
        "total_items": "integer"
      }
    }
  }
  ```
- **Notes:** If quantity is set to 0, the item will be removed from the cart.

#### 30. Remove Item from Cart
- **Endpoint:** `DELETE /api/cart/items/{id}`
- **Description:** Remove an item from the cart
- **Authorization:** Required
- **Path Parameters:** `id` - Cart Item ID
- **Response:**
  ```json
  {
    "success": true,
    "message": "Item removed from cart",
    "data": {
      "cart_summary": {
        "subtotal": "decimal",
        "total_items": "integer"
      }
    }
  }
  ```
- **Notes:** Completely removes the item from the cart regardless of quantity.

#### 31. Clear Cart
- **Endpoint:** `DELETE /api/cart`
- **Description:** Remove all items from the cart
- **Authorization:** Required
- **Response:**
  ```json
  {
    "success": true,
    "message": "Cart cleared successfully"
  }
  ```
- **Notes:** Removes all items from the user's cart.

### Wishlist APIs

#### 32. Get User's Wishlist
- **Endpoint:** `GET /api/wishlist`
- **Description:** Get the current user's wishlist
- **Authorization:** Required
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "wishlist_id": "integer",
      "items": [
        {
          "wishlist_item_id": "integer",
          "product": {
            "product_id": "integer",
            "name": "string",
            "base_price": "decimal",
            "discount_percentage": "decimal",
            "final_price": "decimal",
            "main_image_url": "string",
            "average_rating": "float",
            "is_available": "boolean"
          },
          "added_at": "datetime"
        }
      ],
      "total_items": "integer"
    }


#### 33. Add to Wishlist
- **Endpoint:** `POST /api/wishlist/items`
- **Description:** Add a product to the wishlist
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "product_id": "integer"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Product added to wishlist",
    "data": {
      "wishlist_item_id": "integer",
      "product_id": "integer"
    }
  }
  ```
- **Notes:** If the product is already in the wishlist, returns success without creating a duplicate entry.

#### 34. Remove from Wishlist
- **Endpoint:** `DELETE /api/wishlist/items/{id}`
- **Description:** Remove a product from the wishlist
- **Authorization:** Required
- **Path Parameters:** `id` - Wishlist Item ID
- **Response:**
  ```json
  {
    "success": true,
    "message": "Product removed from wishlist"
  }
  ```
- **Notes:** Completely removes the item from the wishlist.

#### 35. Move from Wishlist to Cart
- **Endpoint:** `POST /api/wishlist/items/{id}/move-to-cart`
- **Description:** Move a product from wishlist to cart
- **Authorization:** Required
- **Path Parameters:** `id` - Wishlist Item ID
- **Request Body:**
  ```json
  {
    "quantity": "integer",  // default: 1
    "variant_id": "integer"  // optional: specific variant to add
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Product moved from wishlist to cart",
    "data": {
      "cart_item_id": "integer",
      "removed_from_wishlist": true
    }
  }
  ```
- **Notes:** Adds the item to the cart and removes it from the wishlist.

### Order APIs

#### 36. Create Order (Checkout)
- **Endpoint:** `POST /api/orders`
- **Description:** Create a new order from the cart items
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "address_id": "integer",
    "payment_method": "string",
    "coupon_code": "string",  // optional
    "notes": "string"  // optional
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Order created successfully",
    "data": {
      "order_id": "integer",
      "final_amount": "decimal",
      "payment_status": "string",
      "created_at": "datetime",
      "suborders": [
        {
          "suborder_id": "integer",
          "seller": {
            "seller_id": "integer",
            "business_name": "string"
          },
          "status": "string",
          "item_count": "integer",
          "subtotal": "decimal"
        }
      ],
      "payment_details": {
        "payment_method": "string",
        "payment_link": "string"  // if applicable
      }
    }
  }
  ```
- **Notes:** Creates an order using the create_order stored procedure, which automatically splits the order by seller. Empties the cart after successful order creation.

#### 37. Get Order History
- **Endpoint:** `GET /api/orders`
- **Description:** Get the order history for the current user
- **Authorization:** Required
- **Query Parameters:**
  - `status` (optional): Filter by order status
  - `start_date` (optional): Start date for filtering
  - `end_date` (optional): End date for filtering
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "order_id": "integer",
          "created_at": "datetime",
          "final_amount": "decimal",
          "payment_status": "string",
          "suborders": [
            {
              "suborder_id": "integer",
              "seller": {
                "seller_id": "integer",
                "business_name": "string"
              },
              "status": "string",
              "item_count": "integer"
            }
          ]
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns paginated order history with summary information. For detailed order information, use the Get Order by ID endpoint.

#### 38. Get Order Details
- **Endpoint:** `GET /api/orders/{id}`
- **Description:** Get detailed information for a specific order
- **Authorization:** Required
- **Path Parameters:** `id` - Order ID
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "order_id": "integer",
      "created_at": "datetime",
      "updated_at": "datetime",
      "total_amount": "decimal",
      "discount_amount": "decimal",
      "shipping_fee": "decimal",
      "tax_amount": "decimal",
      "final_amount": "decimal",
      "payment_method": "string",
      "payment_status": "string",
      "address": {
        "address_id": "integer",
        "street_address": "string",
        "city": "string",
        "state": "string",
        "postal_code": "string",
        "country": "string",
        "phone_number": "string"
      },
      "coupon": {
        "coupon_id": "integer",
        "code": "string",
        "discount_amount": "decimal"
      },
      "suborders": [
        {
          "suborder_id": "integer",
          "seller": {
            "seller_id": "integer",
            "business_name": "string"
          },
          "status": "string",
          "status_updated_at": "datetime",
          "subtotal": "decimal",
          "tracking_number": "string",
          "shipping_provider": "string",
          "items": [
            {
              "order_item_id": "integer",
              "product": {
                "product_id": "integer",
                "name": "string",
                "main_image_url": "string"
              },
              "variant": {
                "variant_id": "integer",
                "variant_name": "string",
                "attributes": [
                  {
                    "attribute_name": "string",
                    "attribute_value": "string"
                  }
                ]
              },
              "quantity": "integer",
              "price_at_purchase": "decimal",
              "total_price": "decimal"
            }
          ]
        }
      ]
    }
  }
  ```
- **Notes:** Returns comprehensive order information including suborders, items, and delivery details.

#### 39. Cancel Order (Customer)
- **Endpoint:** `POST /api/orders/{id}/cancel`
- **Description:** Cancel an order as a customer
- **Authorization:** Required
- **Path Parameters:** `id` - Order ID
- **Request Body:**
  ```json
  {
    "cancellation_reason": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Order cancellation requested",
    "data": {
      "order_id": "integer",
      "status": "cancellation_requested"
    }
  }
  ```
- **Notes:** Customer can only cancel orders that haven't been shipped yet. Some implementations may allow immediate cancellation of orders with certain statuses.

#### 40. Track Order
- **Endpoint:** `GET /api/orders/{id}/tracking`
- **Description:** Get tracking information for an order
- **Authorization:** Required
- **Path Parameters:** `id` - Order ID
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "order_id": "integer",
      "suborders": [
        {
          "suborder_id": "integer",
          "seller": {
            "business_name": "string"
          },
          "status": "string",
          "status_updated_at": "datetime",
          "tracking_number": "string",
          "shipping_provider": "string",
          "tracking_url": "string",
          "estimated_delivery": "datetime",
          "tracking_history": [
            {
              "status": "string",
              "description": "string",
              "location": "string",
              "timestamp": "datetime"
            }
          ]
        }
      ]
    }
  }
  ```
- **Notes:** Provides detailed tracking information for each suborder including status history and tracking links.

#### 41. Request Return
- **Endpoint:** `POST /api/orders/{orderID}/suborders/{suborderID}/return`
- **Description:** Request to return items from a suborder
- **Authorization:** Required
- **Path Parameters:** 
  - `orderID` - Order ID
  - `suborderID` - Suborder ID
- **Request Body:**
  ```json
  {
    "return_reason": "string",
    "items": [
      {
        "order_item_id": "integer",
        "quantity": "integer",
        "return_reason": "string"  // Optional item-specific reason
      }
    ],
    "comments": "string"  // Optional additional details
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Return request submitted successfully",
    "data": {
      "return_id": "integer",
      "return_status": "requested",
      "requested_at": "datetime"
    }
  }
  ```
- **Notes:** Creates a return request for specified items. Customer can only request returns within a certain time period after delivery (e.g., 30 days).

#### 42. Get Return Requests
- **Endpoint:** `GET /api/returns`
- **Description:** Get return requests for the current user
- **Authorization:** Required
- **Query Parameters:**
  - `status` (optional): Filter by return status
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "return_id": "integer",
          "suborder": {
            "suborder_id": "integer",
            "order_id": "integer"
          },
          "return_reason": "string",
          "return_status": "string",
          "requested_at": "datetime",
          "item_count": "integer",
          "total_refund_amount": "decimal"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns paginated return request history with summary information.

#### 43. Get Return Request Details
- **Endpoint:** `GET /api/returns/{id}`
- **Description:** Get detailed information for a specific return request
- **Authorization:** Required
- **Path Parameters:** `id` - Return Request ID
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "return_id": "integer",
      "suborder": {
        "suborder_id": "integer",
        "order_id": "integer",
        "seller": {
          "seller_id": "integer",
          "business_name": "string"
        }
      },
      "return_reason": "string",
      "return_status": "string",
      "requested_at": "datetime",
      "approved_at": "datetime",
      "received_at": "datetime",
      "refunded_at": "datetime",
      "tracking_number": "string",
      "items": [
        {
          "return_item_id": "integer",
          "order_item": {
            "order_item_id": "integer",
            "product": {
              "product_id": "integer",
              "name": "string",
              "main_image_url": "string"
            },
            "variant": {
              "variant_id": "integer",
              "variant_name": "string"
            },
            "price_at_purchase": "decimal"
          },
          "quantity": "integer",
          "return_reason": "string",
          "condition": "string",
          "refund_amount": "decimal"
        }
      ],
      "total_refund_amount": "decimal",
      "comments": "string",
      "return_instructions": "string"
    }
  }
  ```
- **Notes:** Provides comprehensive information about a return request including items, status history, and refund details.

### Seller Management APIs

#### 44. Get Seller Products
- **Endpoint:** `GET /api/seller/products`
- **Description:** Get products for the current seller
- **Authorization:** Seller required
- **Query Parameters:**
  - `status` (optional): Filter by approval status
  - `subcategory_id` (optional): Filter by subcategory
  - `search` (optional): Search term for product name
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "product_id": "integer",
          "name": "string",
          "base_price": "decimal",
          "discount_percentage": "decimal",
          "stock_quantity": "integer",
          "main_image_url": "string",
          "approval_status": "string",
          "created_at": "datetime",
          "average_rating": "float",
          "subcategory": {
            "subcategory_id": "integer",
            "name": "string"
          },
          "variant_count": "integer",
          "total_sales": "integer"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns paginated list of products for the seller with basic metrics.

#### 45. Manage Inventory
- **Endpoint:** `PUT /api/seller/inventory/update`
- **Description:** Update inventory for multiple products or variants
- **Authorization:** Seller required
- **Request Body:**
  ```json
  {
    "products": [
      {
        "product_id": "integer",
        "stock_quantity": "integer"
      }
    ],
    "variants": [
      {
        "variant_id": "integer",
        "stock_quantity": "integer"
      }
    ]
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Inventory updated successfully",
    "data": {
      "updated_product_count": "integer",
      "updated_variant_count": "integer"
    }
  }
  ```
- **Notes:** Allows bulk updating of inventory levels for multiple products or variants.

#### 46. Get Seller Orders
- **Endpoint:** `GET /api/seller/orders`
- **Description:** Get orders for the current seller
- **Authorization:** Seller required
- **Query Parameters:**
  - `status` (optional): Filter by order status
  - `start_date` (optional): Start date for filtering
  - `end_date` (optional): End date for filtering
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "suborder_id": "integer",
          "order_id": "integer",
          "created_at": "datetime",
          "status": "string",
          "subtotal": "decimal",
          "item_count": "integer",
          "customer": {
            "customer_id": "integer",
            "name": "string"
          }
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns paginated list of orders for the seller to process.

#### 47. Get Seller Order Details
- **Endpoint:** `GET /api/seller/orders/{id}`
- **Description:** Get detailed information for a specific seller order
- **Authorization:** Seller required
- **Path Parameters:** `id` - Suborder ID
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "suborder_id": "integer",
      "order_id": "integer",
      "created_at": "datetime",
      "status": "string",
      "status_updated_at": "datetime",
      "subtotal": "decimal",
      "customer": {
        "customer_id": "integer",
        "name": "string",
        "email": "string"
      },
      "shipping_address": {
        "street_address": "string",
        "city": "string",
        "state": "string",
        "postal_code": "string",
        "country": "string",
        "phone_number": "string"
      },
      "items": [
        {
          "order_item_id": "integer",
          "product": {
            "product_id": "integer",
            "name": "string",
            "main_image_url": "string"
          },
          "variant": {
            "variant_id": "integer",
            "variant_name": "string",
            "sku": "string"
          },
          "quantity": "integer",
          "price_at_purchase": "decimal",
          "total_price": "decimal"
        }
      ],
      "tracking_number": "string",
      "shipping_provider": "string"
    }
  }
  ```
- **Notes:** Provides comprehensive information about a seller order including items and shipping details.

#### 48. Update Order Status (Seller)
- **Endpoint:** `PUT /api/seller/orders/{id}/status`
- **Description:** Update the status of a seller order
- **Authorization:** Seller required
- **Path Parameters:** `id` - Suborder ID
- **Request Body:**
  ```json
  {
    "status": "approved|processing|shipped|delivered|rejected",
    "tracking_number": "string",  // Required for "shipped" status
    "shipping_provider": "string",  // Required for "shipped" status
    "rejection_reason": "string"  // Required for "rejected" status
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Order status updated successfully",
    "data": {
      "suborder_id": "integer",
      "status": "string",
      "status_updated_at": "datetime"
    }
  }
  ```
- **Notes:** Allows the seller to update order status through the fulfillment process. When status is set to "shipped," tracking information is required.

#### 49. Get Seller Returns
- **Endpoint:** `GET /api/seller/returns`
- **Description:** Get return requests for the current seller
- **Authorization:** Seller required
- **Query Parameters:**
  - `status` (optional): Filter by return status
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "return_id": "integer",
          "suborder_id": "integer",
          "order_id": "integer",
          "customer": {
            "customer_id": "integer",
            "name": "string"
          },
          "return_reason": "string",
          "return_status": "string",
          "requested_at": "datetime",
          "item_count": "integer",
          "total_refund_amount": "decimal"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns paginated list of return requests for the seller to process.

#### 50. Process Return Request (Seller)
- **Endpoint:** `PUT /api/seller/returns/{id}/process`
- **Description:** Approve, reject, or complete a return request
- **Authorization:** Seller required
- **Path Parameters:** `id` - Return Request ID
- **Request Body:**
  ```json
  {
    "action": "approve|reject|receive",
    "refund_amount": "decimal",  // For approve/receive actions
    "rejection_reason": "string",  // For reject action
    "return_instructions": "string",  // For approve action
    "items": [  // Optional, for partial approvals
      {
        "return_item_id": "integer",
        "approved": "boolean",
        "refund_amount": "decimal"
      }
    ]
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Return request processed successfully",
    "data": {
      "return_id": "integer",
      "return_status": "string",
      "processed_at": "datetime"
    }
  }
  ```
- **Notes:** Allows sellers to process return requests through approval, rejection, or receipt of returned items.

### Review and Rating APIs

#### 51. Get Product Reviews
- **Endpoint:** `GET /api/products/{id}/reviews`
- **Description:** Get reviews for a specific product
- **Path Parameters:** `id` - Product ID
- **Query Parameters:**
  - `sort_by` (optional): Sort field (newest, highest_rating, lowest_rating, most_helpful)
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "rating_id": "integer",
          "customer": {
            "name": "string"
          },
          "stars": "integer",
          "comment": "string",
          "created_at": "datetime",
          "is_verified_purchase": "boolean",
          "helpful_count": "integer",
          "images": [
            {
              "image_url": "string"
            }
          ],
          "current_user_found_helpful": "boolean"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      },
      "summary": {
        "average_rating": "float",
        "total_reviews": "integer",
        "distribution": {
          "5": "integer",
          "4": "integer",
          "3": "integer",
          "2": "integer",
          "1": "integer"
        }
      }
    }
  }
  ```
- **Notes:** Returns paginated reviews with rating summary and distribution. Includes whether the current user found each review helpful.

#### 52. Add Product Review
- **Endpoint:** `POST /api/products/{id}/reviews`
- **Description:** Add a review for a product
- **Authorization:** Required
- **Path Parameters:** `id` - Product ID
- **Request Body:**
  ```json
  {
    "stars": "integer",  // 1-5
    "comment": "string",
    "images": [  // Optional
      {
        "image_url": "string"
      }
    ]
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Review added successfully",
    "data": {
      "rating_id": "integer",
      "stars": "integer",
      "created_at": "datetime",
      "is_verified_purchase": "boolean"
    }
  }
  ```
- **Notes:** Automatically checks if the user has purchased the product and marks the review accordingly. Users can only review a product once.

#### 53. Update Product Review
- **Endpoint:** `PUT /api/products/reviews/{id}`
- **Description:** Update an existing review
- **Authorization:** Required
- **Path Parameters:** `id` - Rating ID
- **Request Body:**
  ```json
  {
    "stars": "integer",  // 1-5
    "comment": "string",
    "images": [  // Optional, overwrites existing images
      {
        "image_url": "string"
      }
    ]
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Review updated successfully",
    "data": {
      "rating_id": "integer",
      "stars": "integer",
      "updated_at": "datetime"
    }
  }
  ```
- **Notes:** Users can only update their own reviews.

#### 54. Delete Product Review
- **Endpoint:** `DELETE /api/products/reviews/{id}`
- **Description:** Delete a review
- **Authorization:** Required
- **Path Parameters:** `id` - Rating ID
- **Response:**
  ```json
  {
    "success": true,
    "message": "Review deleted successfully"
  }
  ```
- **Notes:** Users can only delete their own reviews.

#### 55. Mark Review as Helpful
- **Endpoint:** `POST /api/products/reviews/{id}/helpful`
- **Description:** Mark a review as helpful or not helpful
- **Authorization:** Required
- **Path Parameters:** `id` - Rating ID
- **Request Body:**
  ```json
  {
    "is_helpful": "boolean"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Review helpfulness updated",
    "data": {
      "helpful_count": "integer"
    }
  }
  ```
- **Notes:** Updates or creates a HelpfulRating record for the user and rating. If the user had previously marked the review and is changing their vote, the helpful count is updated accordingly.

### Coupon and Promotion APIs

#### 56. Apply Coupon to Cart
- **Endpoint:** `POST /api/cart/apply-coupon`
- **Description:** Apply a coupon code to the current cart
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "coupon_code": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Coupon applied successfully",
    "data": {
      "coupon": {
        "coupon_id": "integer",
        "code": "string",
        "discount_type": "string",
        "discount_amount": "decimal",
        "minimum_purchase": "decimal"
      },
      "cart_summary": {
        "subtotal": "decimal",
        "discount": "decimal",
        "total_after_discount": "decimal"
      }
    }
  }
  ```
- **Notes:** Validates the coupon code and applies it to the cart if valid. Returns error if the coupon is invalid, expired, or doesn't meet minimum purchase requirements.

#### 57. Remove Coupon from Cart
- **Endpoint:** `DELETE /api/cart/coupon`
- **Description:** Remove the applied coupon from the cart
- **Authorization:** Required
- **Response:**
  ```json
  {
    "success": true,
    "message": "Coupon removed successfully",
    "data": {
      "cart_summary": {
        "subtotal": "decimal",
        "discount": "decimal",
        "total_after_discount": "decimal"
      }
    }
  }
  ```
- **Notes:** Removes any applied coupon from the cart and recalculates totals.

#### 58. Get User's Coupons
- **Endpoint:** `GET /api/coupons/user`
- **Description:** Get all coupons available to the current user
- **Authorization:** Required
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "coupons": [
        {
          "coupon_id": "integer",
          "code": "string",
          "description": "string",
          "discount_type": "string",
          "discount_amount": "decimal",
          "minimum_purchase": "decimal",
          "start_date": "datetime",
          "end_date": "datetime",
          "is_used": "boolean"
        }
      ]
    }
  }
  ```
- **Notes:** Returns all coupons assigned to the user as well as public coupons.

### Affiliate APIs

#### 59. Apply for Affiliate Program
- **Endpoint:** `POST /api/affiliate/apply`
- **Description:** Apply to become an affiliate
- **Authorization:** Required
- **Request Body:**
  ```json
  {
    "payment_method": "string",
    "payment_details": "string",
    "additional_info": "string"  // Optional
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Affiliate application submitted successfully",
    "data": {
      "status": "pending"
    }
  }
  ```
- **Notes:** Creates a pending affiliate application that must be approved by an admin.

#### 60. Get Affiliate Dashboard
- **Endpoint:** `GET /api/affiliate/dashboard`
- **Description:** Get affiliate performance metrics
- **Authorization:** Affiliate required
- **Query Parameters:**
  - `period` (optional): Time period (today, week, month, year)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "affiliate_code": "string",
      "affiliate_link": "string",
      "stats": {
        "total_earnings": "decimal",
        "available_balance": "decimal",
        "pending_commissions": "decimal",
        "withdrawn_amount": "decimal",
        "click_count": "integer",
        "conversion_count": "integer",
        "conversion_rate": "float"
      },
      "period_performance": {
        "period": "string",
        "clicks": "integer",
        "conversions": "integer",
        "earnings": "decimal",
        "chart_data": [
          {
            "date": "date",
            "clicks": "integer",
            "conversions": "integer",
            "earnings": "decimal"
          }
        ]
      },
      "top_products": [
        {
          "product_id": "integer",
          "name": "string",
          "clicks": "integer",
          "conversions": "integer",
          "commissions": "decimal"
        }
      ]
    }
  }
  ```
- **Notes:** Provides comprehensive affiliate performance metrics and statistics.


#### 61. Get Affiliate Commissions
- **Endpoint:** `GET /api/affiliate/commissions`
- **Description:** Get commission history
- **Authorization:** Affiliate required
- **Query Parameters:**
  - `status` (optional): Filter by commission status
  - `start_date` (optional): Start date for filtering
  - `end_date` (optional): End date for filtering
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "commission_id": "integer",
          "order_id": "integer",
          "product": {
            "product_id": "integer",
            "name": "string"
          },
          "seller": {
            "seller_id": "integer",
            "business_name": "string"
          },
          "commission_amount": "decimal",
          "commission_rate": "decimal",
          "order_item_total": "decimal",
          "status": "string",
          "created_at": "datetime",
          "paid_at": "datetime"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      },
      "summary": {
        "total_commissions": "decimal",
        "pending_commissions": "decimal",
        "approved_commissions": "decimal",
        "paid_commissions": "decimal"
      }
    }
  }
  ```
- **Notes:** Returns paginated commission history with summary statistics.

#### 62. Request Affiliate Withdrawal
- **Endpoint:** `POST /api/affiliate/withdrawals`
- **Description:** Request a withdrawal of affiliate earnings
- **Authorization:** Affiliate required
- **Request Body:**
  ```json
  {
    "amount": "decimal",
    "payment_method": "string",
    "payment_details": "string"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Withdrawal request submitted successfully",
    "data": {
      "withdrawal_id": "integer",
      "amount": "decimal",
      "status": "pending",
      "requested_at": "datetime"
    }
  }
  ```
- **Notes:** Creates a withdrawal request for the specified amount. Amount must be less than or equal to available balance.

#### 63. Get Affiliate Withdrawals
- **Endpoint:** `GET /api/affiliate/withdrawals`
- **Description:** Get withdrawal history
- **Authorization:** Affiliate required
- **Query Parameters:**
  - `status` (optional): Filter by withdrawal status
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "withdrawal_id": "integer",
          "amount": "decimal",
          "payment_method": "string",
          "status": "string",
          "requested_at": "datetime",
          "processed_at": "datetime"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns paginated withdrawal history.

#### 64. Get Affiliate Sellers
- **Endpoint:** `GET /api/affiliate/sellers`
- **Description:** Get sellers available for affiliate marketing
- **Authorization:** Affiliate required
- **Query Parameters:**
  - `status` (optional): Filter by relationship status
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "seller": {
            "seller_id": "integer",
            "business_name": "string",
            "business_description": "string",
            "business_logo": "string",
            "rating": "float"
          },
          "relationship": {
            "relationship_id": "integer",
            "status": "string",
            "commission_rate": "decimal",
            "created_at": "datetime"
          },
          "stats": {
            "product_count": "integer",
            "commission_earned": "decimal"
          }
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns sellers that the affiliate can promote, along with relationship status and statistics.

#### 65. Apply to Promote Seller
- **Endpoint:** `POST /api/affiliate/sellers/{sellerId}/apply`
- **Description:** Apply to promote a specific seller's products
- **Authorization:** Affiliate required
- **Path Parameters:** `sellerId` - Seller ID
- **Response:**
  ```json
  {
    "success": true,
    "message": "Application submitted successfully",
    "data": {
      "relationship_id": "integer",
      "status": "pending",
      "created_at": "datetime"
    }
  }
  ```
- **Notes:** Creates a pending relationship that must be approved by the seller.

### Admin APIs

#### 66. Get All Users (Admin)
- **Endpoint:** `GET /api/admin/users`
- **Description:** Get all users with filtering and pagination
- **Authorization:** Admin required
- **Query Parameters:**
  - `user_type` (optional): Filter by user type
  - `search` (optional): Search by name or email
  - `is_active` (optional): Filter by active status
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "user_id": "integer",
          "email": "string",
          "first_name": "string",
          "last_name": "string",
          "user_type": "string",
          "is_active": "boolean",
          "created_at": "datetime"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns paginated user list with basic information.

#### 67. Update User Status (Admin)
- **Endpoint:** `PUT /api/admin/users/{id}/status`
- **Description:** Activate or deactivate a user
- **Authorization:** Admin required
- **Path Parameters:** `id` - User ID
- **Request Body:**
  ```json
  {
    "is_active": "boolean"
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "User status updated successfully",
    "data": {
      "user_id": "integer",
      "is_active": "boolean"
    }
  }
  ```
- **Notes:** Enables or disables user access to the platform.

#### 68. Get Pending Products (Admin)
- **Endpoint:** `GET /api/admin/products/pending`
- **Description:** Get products pending approval
- **Authorization:** Admin required
- **Query Parameters:**
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "product_id": "integer",
          "name": "string",
          "base_price": "decimal",
          "created_at": "datetime",
          "seller": {
            "seller_id": "integer",
            "business_name": "string"
          },
          "subcategory": {
            "subcategory_id": "integer",
            "name": "string"
          }
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns products awaiting approval from administrators.

#### 69. Manage Affiliate Applications (Admin)
- **Endpoint:** `GET /api/admin/affiliates/applications`
- **Description:** Get pending affiliate applications
- **Authorization:** Admin required
- **Query Parameters:**
  - `status` (optional): Filter by application status
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "affiliate_id": "integer",
          "user": {
            "user_id": "integer",
            "email": "string",
            "first_name": "string",
            "last_name": "string"
          },
          "status": "string",
          "created_at": "datetime",
          "payment_method": "string"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns affiliate applications for admin review.

#### 70. Process Affiliate Application (Admin)
- **Endpoint:** `PUT /api/admin/affiliates/{id}/process`
- **Description:** Approve or reject an affiliate application
- **Authorization:** Admin required
- **Path Parameters:** `id` - Affiliate ID
- **Request Body:**
  ```json
  {
    "action": "approve|reject",
    "commission_rate": "decimal",  // For approve action
    "rejection_reason": "string"  // For reject action
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Affiliate application processed successfully",
    "data": {
      "affiliate_id": "integer",
      "status": "string",
      "processed_at": "datetime"
    }
  }
  ```
- **Notes:** Approves or rejects an affiliate application and sets the commission rate if approved.

#### 71. Manage Affiliate Withdrawals (Admin)
- **Endpoint:** `GET /api/admin/affiliates/withdrawals`
- **Description:** Get affiliate withdrawal requests
- **Authorization:** Admin required
- **Query Parameters:**
  - `status` (optional): Filter by withdrawal status
  - `page` (optional): Page number (default: 1)
  - `page_size` (optional): Items per page (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "items": [
        {
          "withdrawal_id": "integer",
          "affiliate": {
            "affiliate_id": "integer",
            "user": {
              "user_id": "integer",
              "email": "string",
              "first_name": "string",
              "last_name": "string"
            }
          },
          "amount": "decimal",
          "payment_method": "string",
          "payment_details": "string",
          "status": "string",
          "requested_at": "datetime"
        }
      ],
      "pagination": {
        "total_items": "integer",
        "total_pages": "integer",
        "current_page": "integer",
        "page_size": "integer"
      }
    }
  }
  ```
- **Notes:** Returns withdrawal requests for admin processing.

#### 72. Process Affiliate Withdrawal (Admin)
- **Endpoint:** `PUT /api/admin/affiliates/withdrawals/{id}/process`
- **Description:** Process an affiliate withdrawal request
- **Authorization:** Admin required
- **Path Parameters:** `id` - Withdrawal ID
- **Request Body:**
  ```json
  {
    "action": "approve|reject",
    "notes": "string"  // Optional
  }
  ```
- **Response:**
  ```json
  {
    "success": true,
    "message": "Withdrawal request processed successfully",
    "data": {
      "withdrawal_id": "integer",
      "status": "string",
      "processed_at": "datetime"
    }
  }
  ```
- **Notes:** Approves or rejects a withdrawal request. If approved, the system marks it as completed and updates the affiliate's balance.

### Analytics and Reporting APIs

#### 73. Get Seller Dashboard Stats
- **Endpoint:** `GET /api/seller/dashboard/stats`
- **Description:** Get key performance metrics for seller dashboard
- **Authorization:** Seller required
- **Query Parameters:**
  - `period` (optional): Time period (today, week, month, year)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "period": "string",
      "sales": {
        "total_orders": "integer",
        "total_sales": "decimal",
        "avg_order_value": "decimal",
        "growth_rate": "float"
      },
      "products": {
        "total_products": "integer",
        "active_products": "integer",
        "pending_approval": "integer",
        "low_stock": "integer",
        "out_of_stock": "integer"
      },
      "performance": {
        "conversion_rate": "float",
        "return_rate": "float",
        "cancellation_rate": "float",
        "average_rating": "float"
      },
      "chart_data": [
        {
          "date": "date",
          "orders": "integer",
          "sales": "decimal"
        }
      ],
      "top_products": [
        {
          "product_id": "integer",
          "name": "string",
          "units_sold": "integer",
          "revenue": "decimal"
        }
      ]
    }
  }
  ```
- **Notes:** Provides comprehensive performance metrics for the seller dashboard.

#### 74. Get Admin Dashboard Stats
- **Endpoint:** `GET /api/admin/dashboard/stats`
- **Description:** Get key performance metrics for admin dashboard
- **Authorization:** Admin required
- **Query Parameters:**
  - `period` (optional): Time period (today, week, month, year)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "period": "string",
      "users": {
        "total_users": "integer",
        "new_users": "integer",
        "customers": "integer",
        "sellers": "integer",
        "affiliates": "integer"
      },
      "sales": {
        "total_orders": "integer",
        "total_sales": "decimal",
        "avg_order_value": "decimal",
        "growth_rate": "float"
      },
      "products": {
        "total_products": "integer",
        "pending_approval": "integer",
        "approved_today": "integer"
      },
      "chart_data": [
        {
          "date": "date",
          "orders": "integer",
          "sales": "decimal",
          "new_users": "integer"
        }
      ],
      "top_sellers": [
        {
          "seller_id": "integer",
          "business_name": "string",
          "orders": "integer",
          "revenue": "decimal"
        }
      ],
      "top_categories": [
        {
          "category_id": "integer",
          "name": "string",
          "orders": "integer",
          "revenue": "decimal"
        }
      ]
    }
  }
  ```
- **Notes:** Provides comprehensive performance metrics for the admin dashboard.

### Recommendation APIs

#### 75. Get Recommended Products
- **Endpoint:** `GET /api/recommendations/products`
- **Description:** Get personalized product recommendations
- **Authorization:** Optional
- **Query Parameters:**
  - `type` (optional): Recommendation type (browsing_history, purchase_history, trending)
  - `limit` (optional): Number of recommendations to return (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "recommendations": [
        {
          "product_id": "integer",
          "name": "string",
          "base_price": "decimal",
          "discount_percentage": "decimal",
          "final_price": "decimal",
          "main_image_url": "string",
          "average_rating": "float",
          "recommendation_type": "string"
        }
      ]
    }
  }
  ```
- **Notes:** Returns personalized product recommendations based on user behavior, purchase history, or trending products.

#### 76. Get Similar Products
- **Endpoint:** `GET /api/products/{id}/similar`
- **Description:** Get products similar to a specific product
- **Path Parameters:** `id` - Product ID
- **Query Parameters:**
  - `limit` (optional): Number of similar products to return (default: 10)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "similar_products": [
        {
          "product_id": "integer",
          "name": "string",
          "base_price": "decimal",
          "discount_percentage": "decimal",
          "final_price": "decimal",
          "main_image_url": "string",
          "average_rating": "float",
          "similarity_score": "float"
        }
      ]
    }
  }
  ```
- **Notes:** Returns products similar to the specified product based on category, attributes, and user behavior.

#### 77. Get Frequently Bought Together
- **Endpoint:** `GET /api/products/{id}/frequently-bought-together`
- **Description:** Get products frequently bought with a specific product
- **Path Parameters:** `id` - Product ID
- **Query Parameters:**
  - `limit` (optional): Number of products to return (default: 5)
- **Response:**
  ```json
  {
    "success": true,
    "data": {
      "products": [
        {
          "product_id": "integer",
          "name": "string",
          "base_price": "decimal",
          "discount_percentage": "decimal",
          "final_price": "decimal",
          "main_image_url": "string",
          "average_rating": "float"
        }
      ],
      "total_price": "decimal"
    }
  }
  ```
- **Notes:** Returns products that are frequently purchased together with the specified product.

## Implementation Tasks Checklist

### Project Setup
- [ ] Initialize .NET Web API project
- [ ] Set up database connection
- [ ] Configure Entity Framework Core
- [ ] Set up JWT authentication
- [ ] Create repository interfaces and implementations
- [ ] Configure dependency injection
- [ ] Set up logging
- [ ] Configure Swagger/OpenAPI

### User Management
- [ ] Implement User entity and repository
- [ ] Create authentication service
- [ ] Implement user registration
- [ ] Implement user login and JWT generation
- [ ] Create user profile management
- [ ] Implement address management
- [ ] Set up role-based authorization

### Product Management
- [ ] Create Category and SubCategory entities and repositories
- [ ] Implement Product entity and repository
- [ ] Create product attributes system
- [ ] Implement product variant functionality
- [ ] Create product image handling
- [ ] Implement product search and filtering
- [ ] Create product recommendation engine

### Shopping Experience
- [ ] Implement Cart and CartItem entities and repositories
- [ ] Create Wishlist functionality
- [ ] Implement add to cart functionality
- [ ] Create cart management operations
- [ ] Implement wishlist management
- [ ] Create coupon application system

### Order Processing
- [ ] Implement Order and SubOrder entities and repositories
- [ ] Create OrderItem entity and repository
- [ ] Implement checkout process
- [ ] Create order splitting logic
- [ ] Implement order status management
- [ ] Create order tracking functionality
- [ ] Implement return request system

### Seller Management
- [ ] Create Seller entity and repository
- [ ] Implement seller dashboard
- [ ] Create product management for sellers
- [ ] Implement inventory management
- [ ] Create order fulfillment functionality
- [ ] Implement return processing
- [ ] Create seller analytics

### Review and Rating System
- [ ] Implement Rating entity and repository
- [ ] Create review submission functionality
- [ ] Implement helpful votes system
- [ ] Create review moderation (for admins)
- [ ] Implement review analytics

### Affiliate System
- [ ] Create Affiliate entity and repository
- [ ] Implement affiliate application process
- [ ] Create affiliate dashboard
- [ ] Implement commission tracking
- [ ] Create affiliate withdrawal system
- [ ] Implement seller-affiliate relationships

### Admin Features
- [ ] Create admin dashboard
- [ ] Implement user management for admins
- [ ] Create product approval workflow
- [ ] Implement category management
- [ ] Create coupon management
- [ ] Implement affiliate management
- [ ] Create reporting and analytics

### Testing
- [ ] Write unit tests for repositories
- [ ] Create integration tests for API endpoints
- [ ] Implement end-to-end testing
- [ ] Perform load testing
- [ ] Create test data generation scripts

### Deployment
- [ ] Set up CI/CD pipeline
- [ ] Configure staging environment
- [ ] Create production deployment plan
- [ ] Implement database migration strategy
- [ ] Set up monitoring and logging
- [ ] Create backup and recovery procedures

## Frontend Components

### Authentication Components
- [ ] Login Form
- [ ] Registration Form
- [ ] Password Reset
- [ ] User Profile Page

### Product Components
- [ ] Product Card
- [ ] Product Grid
- [ ] Product Detail Page
- [ ] Product Search
- [ ] Product Filters
- [ ] Product Category Navigation

### Cart Components
- [ ] Cart Summary
- [ ] Cart Item Card
- [ ] Coupon Application
- [ ] Checkout Form
- [ ] Order Confirmation

### Seller Dashboard Components
- [ ] Seller Dashboard Overview
- [ ] Product Management
- [ ] Order Management
- [ ] Inventory Management
- [ ] Analytics Dashboard

### Customer Dashboard Components
- [ ] Order History
- [ ] Order Detail
- [ ] Wishlist Management
- [ ] Address Book
- [ ] Return Request Form

### Admin Dashboard Components
- [ ] Admin Dashboard Overview
- [ ] User Management
- [ ] Product Approval
- [ ] Category Management
- [ ] Coupon Management
- [ ] Affiliate Management

### Affiliate Dashboard Components
- [ ] Affiliate Dashboard Overview
- [ ] Commission Tracking
- [ ] Withdrawal Management
- [ ] Seller Applications

## Integration Points

1. **Payment Gateway Integration**
   - Payment processing service for handling transactions
   - Status callbacks for payment completion/failure

2. **Email Service Integration**
   - Order confirmation emails
   - Shipping notifications
   - Password reset emails
   - Marketing communications

3. **SMS Service Integration**
   - Order status notifications
   - Delivery alerts
   - Two-factor authentication

4. **File Storage Integration**
   - Product images
   - User profile pictures
   - Review images

5. **Analytics Integration**
   - User behavior tracking
   - Conversion analytics
   - Sales reporting

6. **Search Engine Integration**
   - Advanced product search
   - Auto-suggestions
   - Search analytics

## Security Considerations

1. **Authentication**
   - JWT token generation and validation
   - Refresh token mechanism
   - Password hashing (bcrypt)
   - Account lockout after failed attempts

2. **Authorization**
   - Role-based access control
   - Resource ownership validation
   - Permission checks

3. **Data Protection**
   - Input validation on all endpoints
   - XSS prevention
   - CSRF protection
   - SQL injection prevention

4. **API Security**
   - Rate limiting
   - HTTPS enforcement
   - Security headers
   - CORS configuration

## Performance Optimization

1. **Database Optimization**
   - Efficient indexing
   - Query optimization
   - Connection pooling
   - Caching strategies

2. **API Response Optimization**
   - Pagination for list endpoints
   - Data projection (select only needed fields)
   - Compression
   - ETags for caching

3. **Asynchronous Processing**
   - Background jobs for long-running tasks
   - Message queues for event handling
   - Email sending via background tasks

## Monitoring and Logging

1. **Application Logging**
   - Request/response logging
   - Error logging
   - Performance metrics
   - Audit logging for sensitive operations

2. **Monitoring**
   - Health checks
   - Performance monitoring
   - Error tracking
   - User behavior analytics

## Final Recommendations

1. **Phased Implementation**
   - Implement core features first (user, product, cart, order)
   - Add secondary features in later phases (affiliate, recommendations)
   - Continuously deploy and test

2. **Scalability Planning**
   - Design for horizontal scaling
   - Stateless API architecture
   - Consider service separation for major features
   - Plan for database scaling

3. **Testing Strategy**
   - Comprehensive unit testing
   - API integration testing
   - Stress testing for peak loads
   - Security vulnerability testing

4. **Documentation**
   - Maintain API documentation
   - Create developer guides
   - Document database schema and relationships
   - Include setup and deployment instructions
