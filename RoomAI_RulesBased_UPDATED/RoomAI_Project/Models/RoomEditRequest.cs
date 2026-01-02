namespace RoomAI_Project.Models
{
    public class RoomEditRequest
    {
        public string ImageId { get; set; } = string.Empty;
        public string SurfaceMode { get; set; } = "both";
        public double RoomAreaM2 { get; set; }
        public string RoomType { get; set; } = "living_room";
        public string SelectedMaterial { get; set; } = string.Empty;
        public string SelectedColor { get; set; } = string.Empty;
        public string ColorPalette { get; set; } = string.Empty;
    }

    public class GenerateResponse
    {
        public bool Success { get; set; }
        public string? ResultImageUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public string? MaskUrl { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class UploadResponse
    {
        public bool Success { get; set; }
        public string? ImageId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RoomClassification
    {
        public string RoomType { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class ColorPalette
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Colors { get; set; } = new();
        public string Category { get; set; } = string.Empty;
    }

    public class Material
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TextureUrl { get; set; } = string.Empty;

        // Optional: if provided, material is shown only for these room types.
        // Example: ["kitchen","bathroom"]
        public List<string> Rooms { get; set; } = new();
    }
}
