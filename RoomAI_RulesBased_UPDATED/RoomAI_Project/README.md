# Room AI - Interior Design with AI

مشروع تخرج متكامل لتطبيق ويب لتعديل الغرف باستخدام الذكاء الاصطناعي.

## المتطلبات

- .NET 8.0 SDK
- Visual Studio Code أو Visual Studio
- متصفح ويب حديث

## البنية المعمارية

```
RoomAI_Project/
├── Controllers/          # وحدات التحكم
├── Models/              # نماذج البيانات
├── Views/               # صفحات Razor
├── Services/            # خدمات الذكاء الاصطناعي
├── wwwroot/             # الملفات الثابتة (CSS, JS, Images)
├── Program.cs           # نقطة البداية
├── appsettings.json     # التكوينات
└── RoomAI_Project.csproj # ملف المشروع
```

## الميزات الرئيسية

### 1. اختيار وضع التعديل
- جدران فقط (Walls Only)
- أرضيات فقط (Floors Only)
- جدران + أرضيات (Both)

### 2. رفع الصور
- دعم Drag & Drop
- معاينة الصور
- التحقق من حجم الملف

### 3. تحرير التصميم
- واجهة خطوات (Stepper)
- إدخال معلومات الغرفة
- اختيار الألوان والمواد
- قياس AR اختياري

### 4. توليد التصاميم
- تكامل مع APIs الذكاء الاصطناعي على Colab
- معالجة الصور تلقائية
- عرض النتائج

### 5. عرض النتائج
- Before/After Slider
- تحميل الصور
- معلومات التصميم

## التثبيت والتشغيل

### 1. استنساخ المشروع
```bash
cd RoomAI_Project
```

### 2. استعادة المكتبات
```bash
dotnet restore
```

### 3. تشغيل التطبيق
```bash
dotnet run
```

### 4. فتح المتصفح
```
http://localhost:5000
```

## التكوينات

### appsettings.json

```json
{
  "AIServices": {
    "SegmentationApiUrl": "https://your-colab-api.com/segment",
    "StyleTransferApiUrl": "https://your-colab-api.com/transform",
    "RoomClassifierApiUrl": "https://your-colab-api.com/classify",
    "UseLocalMock": true
  },
  "FileUpload": {
    "MaxFileSizeInMB": 50,
    "AllowedExtensions": ".jpg,.jpeg,.png,.gif",
    "UploadDirectory": "wwwroot/uploads"
  }
}
```

## مسارات الـ API

### Upload
- **POST** `/Home/UploadImage`
- Body: multipart/form-data (file)
- Response: `{ success, imageId, imageUrl }`

### Generate Design
- **POST** `/Home/Generate`
- Body: JSON (RoomEditRequest)
- Response: `{ success, resultImageUrl, errorMessage }`

### Get Color Palettes
- **GET** `/Home/GetColorPalettes`
- Response: `{ success, data: ColorPalette[] }`

### Get Materials
- **GET** `/Home/GetMaterials?type=flooring`
- Response: `{ success, data: Material[] }`

### Classify Room
- **POST** `/Home/ClassifyRoom`
- Body: multipart/form-data (file)
- Response: `{ success, data: RoomClassification }`

## هيكل البيانات

### RoomEditRequest
```csharp
{
    "imageId": "string",
    "surfaceMode": "walls|floors|both",
    "roomAreaM2": 25.5,
    "roomType": "living_room",
    "selectedMaterial": "wood_oak",
    "selectedColor": "#8B6F47",
    "colorPalette": "Earthy Tones"
}
```

### GenerateResponse
```csharp
{
    "success": true,
    "resultImageUrl": "/uploads/result.jpg",
    "maskUrl": "/uploads/mask.png",
    "metadata": { ... }
}
```

## localStorage Keys

- `surfaceMode` - وضع التعديل
- `imageId` - معرف الصورة
- `imageUrl` - رابط الصورة
- `resultImage` - الصورة النهائية
- `originalImage` - الصورة الأصلية

## خدمات الذكاء الاصطناعي

### SegFormer (Segmentation)
- تقسيم الصورة إلى أجزاء
- تحديد الجدران والأرضيات

### SAM (Segment Anything Model)
- تحسين الأقنعة
- تحديد الحدود بدقة

### Room Classifier
- تصنيف نوع الغرفة
- اقتراح الألوان المناسبة

## مسارات Google Drive

```
/content/drive/MyDrive/AI_Interior_Project/
├── models/
│   ├── room_classifier/model.pth
│   ├── segformer_struct/final/
│   └── sam/sam_vit_h.pth
├── data/
│   └── NYU_Depth_Dataset/
└── artifacts/
    ├── masks/
    └── results/
```

## الملفات المرفوعة

- الصور: `wwwroot/uploads/`
- النتائج: `wwwroot/uploads/results/`
- الأقنعة: `wwwroot/uploads/masks/`

## معالجة الأخطاء

- التحقق من حجم الملف
- التحقق من صيغة الملف
- معالجة أخطاء الشبكة
- رسائل خطأ واضحة

## الأمان

- CORS مفعل
- التحقق من نوع الملف
- حد أقصى لحجم الملف
- معرفات فريدة للصور

## الأداء

- تخزين مؤقت للصور
- معالجة غير متزامنة
- تحميل كسول للصور
- ضغط الصور

## الدعم والمساعدة

للمساعدة في تشغيل المشروع:

1. تأكد من تثبيت .NET 8.0
2. تحقق من اتصال الإنترنت
3. تحقق من أذونات المجلدات
4. راجع السجلات للأخطاء

## الترخيص

جميع الحقوق محفوظة © 2024

## المساهمون

- فريق التطوير

---

**آخر تحديث:** ديسمبر 2024
**الإصدار:** 1.0.0
