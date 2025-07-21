# Segment API Improvements

## Overview
The segment add and update functionality has been improved with better validation, error handling, and API contract management.

## Key Improvements

### 1. Enhanced Validation
- **Model Validation**: Added comprehensive validation attributes to the Segment model
- **DTO Pattern**: Created separate DTOs for Create and Update operations
- **JSON Validation**: Criteria field is validated as proper JSON
- **Business Rules**: Prevents duplicate segment names
- **Input Sanitization**: Trims whitespace and normalizes status values

### 2. Better Error Handling
- **Try-Catch Blocks**: All operations are wrapped in exception handling
- **Detailed Error Messages**: Clear, actionable error responses
- **HTTP Status Codes**: Proper status codes for different scenarios
- **Validation Errors**: Returns specific validation error messages

### 3. API Contract Improvements
- **SegmentDto**: For creating new segments
- **SegmentUpdateDto**: For updating existing segments
- **Consistent Responses**: Standardized response format
- **CreatedAtAction**: Returns proper 201 status for creation

### 4. Data Integrity
- **Required Fields**: Name is required for all operations
- **Status Validation**: Only accepts 'active', 'inactive', or 'draft'
- **Color Validation**: Hex color format validation
- **Member Count**: Non-negative integer validation

## API Endpoints

### POST /api/segments
Creates a new segment.

**Request Body:**
```json
{
  "name": "Segment Name",
  "description": "Segment description",
  "criteria": "[{\"id\":\"1\",\"field\":\"totalSpent\",\"operator\":\">=\",\"value\":\"500\"}]",
  "memberCount": 0,
  "status": "active",
  "color": "#10B981"
}
```

**Response:**
- `201 Created`: Segment created successfully
- `400 Bad Request`: Validation errors
- `500 Internal Server Error`: Server error

### PUT /api/segments/{id}
Updates an existing segment.

**Request Body:** Same as POST
**Response:**
- `200 OK`: Segment updated successfully
- `404 Not Found`: Segment not found
- `400 Bad Request`: Validation errors
- `500 Internal Server Error`: Server error

## Validation Rules

1. **Name**: Required, max 100 characters, must be unique
2. **Description**: Optional, max 500 characters
3. **Criteria**: Must be valid JSON, max 2000 characters
4. **MemberCount**: Non-negative integer
5. **Status**: Must be 'active', 'inactive', or 'draft'
6. **Color**: Optional, must be valid hex color (e.g., #FF0000)

## Testing
Use the `test_segments.http` file to test all endpoints and validation scenarios. 