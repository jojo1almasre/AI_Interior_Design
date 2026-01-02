# Room AI - API Documentation

## Base URL
```
http://localhost:5000
```

---

## Endpoints

### 1. Upload Image
**Upload a room image for processing**

- **URL:** `/Home/UploadImage`
- **Method:** `POST`
- **Content-Type:** `multipart/form-data`

#### Request
```bash
curl -X POST http://localhost:5000/Home/UploadImage \
  -F "file=@room.jpg"
```

#### Response (Success)
```json
{
    "success": true,
    "imageId": "550e8400-e29b-41d4-a716-446655440000",
    "imageUrl": "/uploads/550e8400-e29b-41d4-a716-446655440000_room.jpg"
}
```

#### Response (Error)
```json
{
    "success": false,
    "errorMessage": "File too large"
}
```

#### Constraints
- Max file size: 50 MB
- Allowed formats: JPG, JPEG, PNG, GIF

---

### 2. Generate Design
**Generate AI-powered room design**

- **URL:** `/Home/Generate`
- **Method:** `POST`
- **Content-Type:** `application/json`

#### Request
```json
{
    "imageId": "550e8400-e29b-41d4-a716-446655440000",
    "surfaceMode": "both",
    "roomAreaM2": 25.5,
    "roomType": "living_room",
    "selectedMaterial": "wood_oak",
    "selectedColor": "#8B6F47",
    "colorPalette": "Earthy Tones"
}
```

#### Response (Success)
```json
{
    "success": true,
    "resultImageUrl": "/uploads/results/result_550e8400.jpg",
    "maskUrl": "/uploads/masks/mask_550e8400.png",
    "metadata": {
        "surfaceMode": "both",
        "roomType": "living_room",
        "material": "wood_oak",
        "color": "#8B6F47",
        "processingTime": "2.5s"
    }
}
```

#### Response (Error)
```json
{
    "success": false,
    "errorMessage": "Image not found"
}
```

#### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| imageId | string | Yes | Image ID from upload |
| surfaceMode | string | Yes | `walls`, `floors`, or `both` |
| roomAreaM2 | number | Yes | Room area in square meters |
| roomType | string | Yes | Type of room |
| selectedMaterial | string | Yes | Material ID |
| selectedColor | string | Yes | Hex color code |
| colorPalette | string | Yes | Palette name |

---

### 3. Get Color Palettes
**Retrieve available color palettes**

- **URL:** `/Home/GetColorPalettes`
- **Method:** `GET`

#### Response
```json
{
    "success": true,
    "data": [
        {
            "name": "Earthy Tones",
            "category": "earthy",
            "colors": ["#8B6F47", "#A0826D", "#B89968", "#D4A574"]
        },
        {
            "name": "Neutral",
            "category": "neutral",
            "colors": ["#9CA3AF", "#D1D5DB", "#E5E7EB", "#F3F4F6"]
        },
        {
            "name": "Cool Gray",
            "category": "cool",
            "colors": ["#475569", "#64748B", "#94A3B8", "#CBD5E1"]
        },
        {
            "name": "Nature Inspired",
            "category": "nature",
            "colors": ["#15803D", "#22C55E", "#86EFAC", "#DCFCE7"]
        },
        {
            "name": "Luxury",
            "category": "luxury",
            "colors": ["#6B21A8", "#9333EA", "#D8B4FE", "#F3E8FF"]
        }
    ]
}
```

---

### 4. Get Materials
**Retrieve available materials**

- **URL:** `/Home/GetMaterials?type=flooring`
- **Method:** `GET`

#### Query Parameters

| Parameter | Type | Required | Values |
|-----------|------|----------|--------|
| type | string | No | `flooring`, `walls`, or both |

#### Response
```json
{
    "success": true,
    "data": [
        {
            "id": "wood_oak",
            "name": "Oak Wood",
            "type": "flooring",
            "textureUrl": "/images/materials/oak.jpg"
        },
        {
            "id": "marble_white",
            "name": "White Marble",
            "type": "flooring",
            "textureUrl": "/images/materials/marble.jpg"
        },
        {
            "id": "ceramic_light",
            "name": "Light Ceramic",
            "type": "flooring",
            "textureUrl": "/images/materials/ceramic.jpg"
        },
        {
            "id": "paint_matte",
            "name": "Matte Paint",
            "type": "walls",
            "textureUrl": "/images/materials/paint.jpg"
        },
        {
            "id": "wallpaper_modern",
            "name": "Modern Wallpaper",
            "type": "walls",
            "textureUrl": "/images/materials/wallpaper.jpg"
        }
    ]
}
```

---

### 5. Classify Room
**Classify room type from image**

- **URL:** `/Home/ClassifyRoom`
- **Method:** `POST`
- **Content-Type:** `multipart/form-data`

#### Request
```bash
curl -X POST http://localhost:5000/Home/ClassifyRoom \
  -F "file=@room.jpg"
```

#### Response
```json
{
    "success": true,
    "data": {
        "roomType": "living_room",
        "confidence": 0.95
    }
}
```

#### Room Types
- `living_room`
- `bedroom`
- `kitchen`
- `bathroom`
- `office`

---

## Data Models

### RoomEditRequest
```csharp
public class RoomEditRequest
{
    public string ImageId { get; set; }
    public string SurfaceMode { get; set; }
    public double RoomAreaM2 { get; set; }
    public string RoomType { get; set; }
    public string SelectedMaterial { get; set; }
    public string SelectedColor { get; set; }
    public string ColorPalette { get; set; }
}
```

### GenerateResponse
```csharp
public class GenerateResponse
{
    public bool Success { get; set; }
    public string ResultImageUrl { get; set; }
    public string ErrorMessage { get; set; }
    public string MaskUrl { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

### ColorPalette
```csharp
public class ColorPalette
{
    public string Name { get; set; }
    public List<string> Colors { get; set; }
    public string Category { get; set; }
}
```

### Material
```csharp
public class Material
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string TextureUrl { get; set; }
}
```

### RoomClassification
```csharp
public class RoomClassification
{
    public string RoomType { get; set; }
    public double Confidence { get; set; }
}
```

---

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
    "success": false,
    "errorMessage": "Invalid request parameters"
}
```

#### 404 Not Found
```json
{
    "success": false,
    "errorMessage": "Image not found"
}
```

#### 500 Internal Server Error
```json
{
    "success": false,
    "errorMessage": "Internal server error"
}
```

---

## Rate Limiting

- No rate limiting implemented (for development)
- Production deployment should implement rate limiting

---

## Authentication

- No authentication required (for development)
- Production deployment should implement JWT or OAuth

---

## CORS

CORS is enabled for all origins:
```csharp
options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

---

## Examples

### JavaScript Fetch

```javascript
async function generateDesign() {
    const request = {
        imageId: "550e8400-e29b-41d4-a716-446655440000",
        surfaceMode: "both",
        roomAreaM2: 25.5,
        roomType: "living_room",
        selectedMaterial: "wood_oak",
        selectedColor: "#8B6F47",
        colorPalette: "Earthy Tones"
    };

    const response = await fetch('/Home/Generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
    });

    const data = await response.json();
    console.log(data);
}
```

### Python Requests

```python
import requests

url = 'http://localhost:5000/Home/Generate'
data = {
    'imageId': '550e8400-e29b-41d4-a716-446655440000',
    'surfaceMode': 'both',
    'roomAreaM2': 25.5,
    'roomType': 'living_room',
    'selectedMaterial': 'wood_oak',
    'selectedColor': '#8B6F47',
    'colorPalette': 'Earthy Tones'
}

response = requests.post(url, json=data)
print(response.json())
```

### cURL

```bash
curl -X POST http://localhost:5000/Home/Generate \
  -H "Content-Type: application/json" \
  -d '{
    "imageId": "550e8400-e29b-41d4-a716-446655440000",
    "surfaceMode": "both",
    "roomAreaM2": 25.5,
    "roomType": "living_room",
    "selectedMaterial": "wood_oak",
    "selectedColor": "#8B6F47",
    "colorPalette": "Earthy Tones"
  }'
```

---

## Testing

### Upload Test
```bash
curl -X POST http://localhost:5000/Home/UploadImage \
  -F "file=@test.jpg"
```

### Get Palettes Test
```bash
curl http://localhost:5000/Home/GetColorPalettes
```

### Get Materials Test
```bash
curl "http://localhost:5000/Home/GetMaterials?type=flooring"
```

---

## Versioning

Current API Version: **1.0.0**

---

## Support

For API support and issues:
- Check the logs in Console
- Review error messages
- Verify request format
- Test with provided examples

---

**Last Updated:** December 2024
**Status:** Active Development
