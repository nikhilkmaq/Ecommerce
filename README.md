# ECommerce API Documentation Nikhil

## Authentication Endpoints

### Register

```http
POST /api/auth/register
```

Register a new user account.

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (200 OK):**

```json
{
  "message": "User created successfully"
}
```

### Login

```http
POST /identity/login
```

Login to get an access token.

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiI...",
  "tokenType": "Bearer",
  "expiresIn": 86400
}
```

### Logout

```http
POST /identity/logout
```

Logout the current user.

**Headers:**

- Authorization: Bearer {token}

**Response (200 OK)**

### Password Management

#### Change Password

```http
POST /identity/password
```

Change the password for the current user.

**Headers:**

- Authorization: Bearer {token}

**Request Body:**

```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!"
}
```

#### Reset Password

```http
POST /identity/reset-password
```

Request a password reset.

**Request Body:**

```json
{
  "email": "user@example.com"
}
```

### User Management

#### Get Current User Info

```http
GET /identity/info
```

Get information about the currently logged-in user.

**Headers:**

- Authorization: Bearer {token}

**Response (200 OK):**

```json
{
  "id": "user-guid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe"
}
```

## Product Endpoints

### Get All Products

```http
GET /api/products
```

Returns a list of all products with their categories.

**Response (200 OK):**

```json
[
  {
    "id": 1,
    "name": "Product Name",
    "description": "Product Description",
    "price": 99.99,
    "category": {
      "id": 1,
      "name": "Category Name"
    }
  }
]
```

### Get Product by ID

```http
GET /api/products/{id}
```

Returns a specific product by ID.

### Create Product

```http
POST /api/products
```

Create a new product (Admin only).

**Headers:**

- Authorization: Bearer {token}

**Request Body:**

```json
{
  "name": "New Product",
  "description": "Product Description",
  "price": 99.99,
  "categoryId": 1
}
```

### Update Product

```http
PUT /api/products/{id}
```

Update an existing product (Admin only).

### Delete Product

```http
DELETE /api/products/{id}
```

Delete a product (Admin only).

## Order Endpoints

### Get Orders

```http
GET /api/orders
```

Get all orders for the current user (or all orders for admin).

**Headers:**

- Authorization: Bearer {token}

**Response (200 OK):**

```json
[
  {
    "id": 1,
    "orderDate": "2023-01-01T00:00:00Z",
    "status": "Pending",
    "totalAmount": 299.97,
    "orderItems": [
      {
        "productId": 1,
        "quantity": 3,
        "unitPrice": 99.99,
        "price": 299.97
      }
    ]
  }
]
```

### Get Order by ID

```http
GET /api/orders/{id}
```

Get a specific order.

### Create Order

```http
POST /api/orders
```

Create a new order from cart items.

### Update Order Status

```http
PUT /api/orders/{id}/status
```

Update order status (Admin only).

**Request Body:**

```json
{
  "status": "Processing"
}
```

## Cart Endpoints

### Get Cart

```http
GET /api/cart
```

Get the current user's cart.

**Headers:**

- Authorization: Bearer {token}

**Response (200 OK):**

```json
{
  "id": 1,
  "userId": "user-guid",
  "cartItems": [
    {
      "productId": 1,
      "quantity": 2,
      "product": {
        "id": 1,
        "name": "Product Name",
        "price": 99.99
      }
    }
  ]
}
```

### Add to Cart

```http
POST /api/cart/items
```

Add an item to the cart.

**Request Body:**

```json
{
  "productId": 1,
  "quantity": 1
}
```

### Remove from Cart

```http
DELETE /api/cart/items/{productId}
```

Remove an item from the cart.

## Protected Routes

All protected routes require the following header:

```http
Authorization: Bearer {your-jwt-token}
```

### Error Responses

#### 400 Bad Request

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "field": ["Error message"]
  }
}
```

#### 401 Unauthorized

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid authentication token"
}
```

#### 403 Forbidden

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Insufficient permissions"
}
```

## Notes

1. All requests and responses use JSON format
2. All dates are in ISO 8601 format
3. All protected endpoints require a valid JWT token
4. Rate limiting may be applied to endpoints
5. All endpoints return appropriate HTTP status codes:
   - 200: Success
   - 201: Created
   - 400: Bad Request
   - 401: Unauthorized
   - 403: Forbidden
   - 404: Not Found
   - 500: Internal Server Error

## Security Recommendations

1. Always use HTTPS
2. Store tokens securely
3. Implement token refresh mechanism
4. Use strong passwords
5. Implement proper CORS policies
