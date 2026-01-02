using Microsoft.AspNetCore.Mvc;
using RoomAI_Project.Models;
using RoomAI_Project.Services;

namespace RoomAI_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAIService _aiService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IAIService aiService, IConfiguration configuration, ILogger<HomeController> logger)
        {
            _aiService = aiService;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SelectMode(string mode)
        {
            if (string.IsNullOrEmpty(mode) || !new[] { "walls", "floors", "both" }.Contains(mode))
            {
                return BadRequest("Invalid mode");
            }

            HttpContext.Session.SetString("SurfaceMode", mode);
            return RedirectToAction("Upload");
        }

        public IActionResult Upload()
        {
            var mode = HttpContext.Session.GetString("SurfaceMode") ?? "both";
            ViewBag.SurfaceMode = mode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new UploadResponse { Success = false, ErrorMessage = "No file uploaded" });
                }

                var maxFileSize = _configuration.GetValue<int>("FileUpload:MaxFileSizeInMB") * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return Json(new UploadResponse { Success = false, ErrorMessage = "File too large" });
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/{fileName}";
                var imageId = Guid.NewGuid().ToString();

                return Json(new UploadResponse
                {
                    Success = true,
                    ImageId = imageId,
                    ImageUrl = imageUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Upload error: {ex.Message}");
                return Json(new UploadResponse { Success = false, ErrorMessage = ex.Message });
            }
        }

        public IActionResult Edit()
        {
            var mode = HttpContext.Session.GetString("SurfaceMode") ?? "both";
            ViewBag.SurfaceMode = mode;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] RoomEditRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageId))
                {
                    return Json(new GenerateResponse { Success = false, ErrorMessage = "No image selected" });
                }

                var tempImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"{request.ImageId}.jpg");
                if (!System.IO.File.Exists(tempImagePath))
                {
                    return Json(new GenerateResponse { Success = false, ErrorMessage = "Image not found" });
                }

                using var imageFile = System.IO.File.OpenRead(tempImagePath);
                var formFile = new FormFile(imageFile, 0, imageFile.Length, "file", Path.GetFileName(tempImagePath));

                var result = await _aiService.GenerateDesignAsync(request, formFile);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Generate error: {ex.Message}");
                return Json(new GenerateResponse { Success = false, ErrorMessage = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClassifyRoom([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "No file uploaded" });
                }

                var classification = await _aiService.ClassifyRoomAsync(file);
                return Json(new { success = true, data = classification });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetColorPalettes()
        {
            try
            {
                var palettes = await _aiService.GetColorPalettesAsync();
                return Json(new { success = true, data = palettes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterials(string type = "flooring", string? roomType = null)
        {
            try
            {
                var materials = await _aiService.GetMaterialsAsync(type, roomType);
                return Json(new { success = true, data = materials });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Result()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
