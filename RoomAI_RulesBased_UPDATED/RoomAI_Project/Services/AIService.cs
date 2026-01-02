using RoomAI_Project.Models;
using System.Text.Json;

namespace RoomAI_Project.Services
{
    public interface IAIService
    {
        Task<GenerateResponse> GenerateDesignAsync(RoomEditRequest request, IFormFile imageFile);
        Task<RoomClassification> ClassifyRoomAsync(IFormFile imageFile);
        Task<List<ColorPalette>> GetColorPalettesAsync();
        // type: "walls" | "flooring"
        // roomType: "living_room" | "bedroom" | "kitchen" | "bathroom" | "office" (or any string)
        Task<List<Material>> GetMaterialsAsync(string type, string? roomType = null);
    }

    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIService> _logger;

        public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GenerateResponse> GenerateDesignAsync(RoomEditRequest request, IFormFile imageFile)
        {
            try
            {
                var useLocalMock = _configuration.GetValue<bool>("AIServices:UseLocalMock");

                if (useLocalMock)
                {
                    return GenerateMockResponse(request);
                }

                using var content = new MultipartFormDataContent();
                using var stream = imageFile.OpenReadStream();
                content.Add(new StreamContent(stream), "image", imageFile.FileName);
                content.Add(new StringContent(request.SurfaceMode), "surfaceMode");
                content.Add(new StringContent(request.RoomType), "roomType");
                content.Add(new StringContent(request.SelectedMaterial), "material");
                content.Add(new StringContent(request.SelectedColor), "color");

                var segmentationUrl = _configuration["AIServices:SegmentationApiUrl"];
                var response = await _httpClient.PostAsync(segmentationUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GenerateResponse>(jsonContent);
                    return result ?? new GenerateResponse { Success = false, ErrorMessage = "Invalid response format" };
                }

                return new GenerateResponse { Success = false, ErrorMessage = "API call failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GenerateDesignAsync: {ex.Message}");
                return new GenerateResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<RoomClassification> ClassifyRoomAsync(IFormFile imageFile)
        {
            try
            {
                var useLocalMock = _configuration.GetValue<bool>("AIServices:UseLocalMock");

                if (useLocalMock)
                {
                    return new RoomClassification { RoomType = "living_room", Confidence = 0.95 };
                }

                using var content = new MultipartFormDataContent();
                using var stream = imageFile.OpenReadStream();
                content.Add(new StreamContent(stream), "image", imageFile.FileName);

                var classifierUrl = _configuration["AIServices:RoomClassifierApiUrl"];
                var response = await _httpClient.PostAsync(classifierUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RoomClassification>(jsonContent);
                    return result ?? new RoomClassification { RoomType = "living_room", Confidence = 0.5 };
                }

                return new RoomClassification { RoomType = "living_room", Confidence = 0.5 };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ClassifyRoomAsync: {ex.Message}");
                return new RoomClassification { RoomType = "living_room", Confidence = 0.5 };
            }
        }

        public async Task<List<ColorPalette>> GetColorPalettesAsync()
        {
            return await Task.FromResult(new List<ColorPalette>
            {
                new ColorPalette
                {
                    Name = "Earthy Tones",
                    Category = "earthy",
                    Colors = new List<string> { "#8B6F47", "#A0826D", "#B89968", "#D4A574" }
                },
                new ColorPalette
                {
                    Name = "Neutral",
                    Category = "neutral",
                    Colors = new List<string> { "#9CA3AF", "#D1D5DB", "#E5E7EB", "#F3F4F6" }
                },
                new ColorPalette
                {
                    Name = "Cool Gray",
                    Category = "cool",
                    Colors = new List<string> { "#475569", "#64748B", "#94A3B8", "#CBD5E1" }
                },
                new ColorPalette
                {
                    Name = "Nature Inspired",
                    Category = "nature",
                    Colors = new List<string> { "#15803D", "#22C55E", "#86EFAC", "#DCFCE7" }
                },
                new ColorPalette
                {
                    Name = "Luxury",
                    Category = "luxury",
                    Colors = new List<string> { "#6B21A8", "#9333EA", "#D8B4FE", "#F3E8FF" }
                }
            });
        }

        public async Task<List<Material>> GetMaterialsAsync(string type, string? roomType = null)
{
    // UI sends: type = "walls" | "flooring"
    // Rules use: surface = "wall" | "floor"
    var surface = (type ?? "").ToLowerInvariant() switch
    {
        "walls" => "wall",
        "flooring" => "floor",
        _ => type?.ToLowerInvariant() ?? "floor"
    };

    // Normalize roomType between UI and rules
    var rt = (roomType ?? "").ToLowerInvariant();
    rt = rt switch
    {
        "office" => "home_office",
        "entrance" => "entrance_hall",
        "entrance_hall" => "entrance_hall",
        _ => rt
    };

    // 1) Material catalog (IDs must match your rules.allowed_materials)
    var materials = new Dictionary<string, Material>
    {
        // ---------- FLOOR ----------
        ["porcelain_tile_matte_R10"] = new Material { Id="porcelain_tile_matte_R10", Name="Porcelain Tile (Matte, R10)", TextureUrl="/images/materials/porcelain_tile.jpg" },
        ["safety_vinyl_EN13845"]     = new Material { Id="safety_vinyl_EN13845",     Name="Safety Vinyl (EN 13845)",       TextureUrl="/images/materials/vinyl_safety.jpg" },
        ["ceramic_floor_matte"]      = new Material { Id="ceramic_floor_matte",      Name="Ceramic Floor (Matte)",         TextureUrl="/images/materials/ceramic_floor.jpg" },
        ["granite_floor_textured"]   = new Material { Id="granite_floor_textured",   Name="Granite (Textured)",            TextureUrl="/images/materials/granite_textured.jpg" },
        ["natural_stone_floor_textured"] = new Material { Id="natural_stone_floor_textured", Name="Natural Stone (Textured)", TextureUrl="/images/materials/natural_stone.jpg" },
        ["engineered_wood_matte"]    = new Material { Id="engineered_wood_matte",    Name="Engineered Wood (Matte)",       TextureUrl="/images/materials/engineered_wood.jpg" },
        ["laminate_AC4_matte"]       = new Material { Id="laminate_AC4_matte",       Name="Laminate (AC4, Matte)",         TextureUrl="/images/materials/laminate_ac4.jpg" },
        ["carpet_soft"]              = new Material { Id="carpet_soft",              Name="Soft Carpet",                    TextureUrl="/images/materials/carpet_soft.jpg" },

        // ---------- WALL ----------
        ["ceramic_wall_tile"]            = new Material { Id="ceramic_wall_tile",            Name="Ceramic Wall Tile",             TextureUrl="/images/materials/ceramic_wall.jpg" },
        ["porcelain_wall_tile"]          = new Material { Id="porcelain_wall_tile",          Name="Porcelain Wall Tile",           TextureUrl="/images/materials/porcelain_wall.jpg" },
        ["bathroom_mold_resistant_paint"]= new Material { Id="bathroom_mold_resistant_paint",Name="Mold-Resistant Bathroom Paint",  TextureUrl="/images/materials/paint_mold_resistant.jpg" },
        ["satin_washable_paint"]         = new Material { Id="satin_washable_paint",         Name="Satin Washable Paint",          TextureUrl="/images/materials/paint_satin.jpg" },
        ["matte_washable_paint"]         = new Material { Id="matte_washable_paint",         Name="Matte Washable Paint",          TextureUrl="/images/materials/paint_matte.jpg" },
        ["decorative_stone_wall"]        = new Material { Id="decorative_stone_wall",        Name="Decorative Stone Wall",         TextureUrl="/images/materials/decorative_stone.jpg" },
        ["wallpaper"]                    = new Material { Id="wallpaper",                    Name="Wallpaper",                      TextureUrl="/images/materials/wallpaper.jpg" },
        ["wood_wall_panels"]             = new Material { Id="wood_wall_panels",             Name="Wood Wall Panels",              TextureUrl="/images/materials/wood_panels.jpg" },
    };

    // 2) Rule-based allowed materials per room + surface
    //    (exactly as your rules dict — simplified to only what UI needs)
    var rules = new Dictionary<string, Dictionary<string, List<string>>>
    {
        ["bathroom"] = new()
        {
            ["floor"] = new() { "porcelain_tile_matte_R10", "safety_vinyl_EN13845", "ceramic_floor_matte", "granite_floor_textured" },
            ["wall"]  = new() { "ceramic_wall_tile", "porcelain_wall_tile", "bathroom_mold_resistant_paint" }
        },
        ["kitchen"] = new()
        {
            ["floor"] = new() { "porcelain_tile_matte_R10", "granite_floor_textured", "safety_vinyl_EN13845", "ceramic_floor_matte" },
            ["wall"]  = new() { "ceramic_wall_tile", "porcelain_wall_tile", "satin_washable_paint" }
        },
        ["entrance_hall"] = new()
        {
            ["floor"] = new() { "granite_floor_textured", "porcelain_tile_matte_R10", "safety_vinyl_EN13845", "natural_stone_floor_textured" },
            ["wall"]  = new() { "satin_washable_paint", "matte_washable_paint", "decorative_stone_wall" }
        },
        ["living_room"] = new()
        {
            ["floor"] = new() { "engineered_wood_matte", "laminate_AC4_matte", "porcelain_tile_matte_R10", "natural_stone_floor_textured" },
            ["wall"]  = new() { "matte_washable_paint", "wallpaper", "decorative_stone_wall", "wood_wall_panels" }
        },
        ["dining_room"] = new()
        {
            ["floor"] = new() { "porcelain_tile_matte_R10", "laminate_AC4_matte", "engineered_wood_matte", "granite_floor_textured" },
            ["wall"]  = new() { "matte_washable_paint", "satin_washable_paint", "wallpaper" }
        },
        ["bedroom"] = new()
        {
            ["floor"] = new() { "carpet_soft", "engineered_wood_matte", "laminate_AC4_matte", "porcelain_tile_matte_R10" },
            ["wall"]  = new() { "matte_washable_paint", "wallpaper", "wood_wall_panels" }
        },
        ["home_office"] = new()
        {
            ["floor"] = new() { "laminate_AC4_matte", "engineered_wood_matte", "porcelain_tile_matte_R10", "safety_vinyl_EN13845" },
            ["wall"]  = new() { "matte_washable_paint", "satin_washable_paint", "wood_wall_panels" }
        },
    };

    List<string>? allowed = null;
    if (!string.IsNullOrWhiteSpace(rt) && rules.TryGetValue(rt, out var bySurface) && bySurface.TryGetValue(surface, out var ids))
        allowed = ids;

    // 3) Build UI list (keep rule order)
    List<Material> result;
    if (allowed != null && allowed.Count > 0)
    {
        result = allowed
            .Where(id => materials.ContainsKey(id))
            .Select(id => materials[id])
            .ToList();
    }
    else
    {
        // fallback: return all materials of this surface (basic heuristic by id list)
        var fallbackIds = surface == "wall"
            ? new[] { "ceramic_wall_tile","porcelain_wall_tile","bathroom_mold_resistant_paint","satin_washable_paint","matte_washable_paint","decorative_stone_wall","wallpaper","wood_wall_panels" }
            : new[] { "porcelain_tile_matte_R10","safety_vinyl_EN13845","ceramic_floor_matte","granite_floor_textured","natural_stone_floor_textured","engineered_wood_matte","laminate_AC4_matte","carpet_soft" };

        result = fallbackIds.Select(id => materials[id]).ToList();
    }

    return await Task.FromResult(result);
}

            return await Task.FromResult(filtered);
        }

        private GenerateResponse GenerateMockResponse(RoomEditRequest request)
        {
            return new GenerateResponse
            {
                Success = true,
                ResultImageUrl = "/images/mock-result.jpg",
                MaskUrl = "/images/mock-mask.png",
                Metadata = new Dictionary<string, object>
                {
                    { "surfaceMode", request.SurfaceMode },
                    { "roomType", request.RoomType },
                    { "material", request.SelectedMaterial },
                    { "color", request.SelectedColor },
                    { "processingTime", "2.5s" }
                }
            };
        }
    }
}
