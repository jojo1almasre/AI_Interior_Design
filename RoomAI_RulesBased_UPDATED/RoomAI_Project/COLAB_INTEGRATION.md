# Colab Integration Guide

## Overview
هذا الدليل يشرح كيفية دمج خدمات الذكاء الاصطناعي من Google Colab مع تطبيق Room AI.

---

## 1. إعداد Colab Notebooks

### أ) Segmentation Model (SegFormer)

```python
# في Colab Notebook

from transformers import SegformerImageProcessor, SegformerForSemanticSegmentation
import torch
from PIL import Image
import numpy as np
from flask import Flask, request, jsonify
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

processor = SegformerImageProcessor.from_pretrained("nvidia/segformer-b5-finetuned-ade-512-512")
model = SegformerForSemanticSegmentation.from_pretrained("nvidia/segformer-b5-finetuned-ade-512-512")

@app.route('/segment', methods=['POST'])
def segment():
    try:
        file = request.files['image']
        surface_mode = request.form.get('surfaceMode', 'both')
        
        image = Image.open(file).convert('RGB')
        inputs = processor(images=image, return_tensors="pt")
        
        with torch.no_grad():
            outputs = model(**inputs)
        
        logits = outputs.logits
        upsampled_logits = torch.nn.functional.interpolate(
            logits,
            size=image.size[::-1],
            mode="bilinear",
            align_corners=False,
        )
        
        pred_seg = upsampled_logits.argmax(dim=1)[0].numpy()
        
        mask = create_mask(pred_seg, surface_mode)
        
        return jsonify({
            'success': True,
            'maskUrl': save_mask(mask),
            'metadata': {
                'surfaceMode': surface_mode,
                'processingTime': '2.5s'
            }
        })
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)})

def create_mask(segmentation, surface_mode):
    mask = np.zeros_like(segmentation, dtype=np.uint8)
    
    if surface_mode in ['walls', 'both']:
        wall_classes = [1, 2, 3]
        for cls in wall_classes:
            mask[segmentation == cls] = 1
    
    if surface_mode in ['floors', 'both']:
        floor_classes = [4, 5, 6]
        for cls in floor_classes:
            mask[segmentation == cls] = 2
    
    return mask

def save_mask(mask):
    filename = f'mask_{uuid.uuid4()}.png'
    Image.fromarray(mask * 100).save(f'/content/drive/MyDrive/AI_Interior_Project/artifacts/masks/{filename}')
    return f'https://your-colab-url/masks/{filename}'

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=False)
```

### ب) Style Transfer Model

```python
# في Colab Notebook

from PIL import Image
import torch
import torchvision.transforms as transforms
from flask import Flask, request, jsonify
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

model = load_style_transfer_model()

@app.route('/transform', methods=['POST'])
def transform():
    try:
        image_file = request.files['image']
        color = request.form.get('color', '#8B6F47')
        material = request.form.get('material', 'wood')
        
        image = Image.open(image_file).convert('RGB')
        mask = load_mask(request.form.get('maskUrl'))
        
        transformed = apply_style(image, mask, color, material)
        
        output_path = save_image(transformed)
        
        return jsonify({
            'success': True,
            'resultImageUrl': output_path,
            'metadata': {
                'color': color,
                'material': material,
                'processingTime': '3.2s'
            }
        })
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)})

def apply_style(image, mask, color, material):
    image_array = np.array(image)
    
    rgb_color = tuple(int(color.lstrip('#')[i:i+2], 16) for i in (0, 2, 4))
    
    image_array[mask > 0] = apply_color_overlay(
        image_array[mask > 0],
        rgb_color,
        0.7
    )
    
    return Image.fromarray(image_array.astype('uint8'))

def apply_color_overlay(pixels, color, alpha):
    return (pixels * (1 - alpha) + np.array(color) * alpha).astype(np.uint8)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5001, debug=False)
```

### ج) Room Classifier

```python
# في Colab Notebook

from transformers import ViTImageProcessor, ViTForImageClassification
from PIL import Image
from flask import Flask, request, jsonify
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

processor = ViTImageProcessor.from_pretrained('google/vit-base-patch16-224')
model = ViTForImageClassification.from_pretrained('google/vit-base-patch16-224')

room_classes = {
    0: 'living_room',
    1: 'bedroom',
    2: 'kitchen',
    3: 'bathroom',
    4: 'office'
}

@app.route('/classify', methods=['POST'])
def classify():
    try:
        file = request.files['image']
        image = Image.open(file).convert('RGB')
        
        inputs = processor(images=image, return_tensors="pt")
        
        with torch.no_grad():
            outputs = model(**inputs)
        
        logits = outputs.logits
        predicted_class_idx = logits.argmax(-1).item()
        
        confidence = torch.nn.functional.softmax(logits, dim=-1)[0][predicted_class_idx].item()
        
        return jsonify({
            'success': True,
            'roomType': room_classes[predicted_class_idx],
            'confidence': confidence
        })
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5002, debug=False)
```

---

## 2. تكوين appsettings.json

```json
{
  "AIServices": {
    "SegmentationApiUrl": "https://your-colab-instance-1.ngrok.io/segment",
    "StyleTransferApiUrl": "https://your-colab-instance-2.ngrok.io/transform",
    "RoomClassifierApiUrl": "https://your-colab-instance-3.ngrok.io/classify",
    "UseLocalMock": false
  }
}
```

---

## 3. استخدام ngrok للـ Tunneling

### تثبيت ngrok

```bash
wget https://bin.equinox.io/c/4VmDzA7iaHg/ngrok-stable-linux-amd64.zip
unzip ngrok-stable-linux-amd64.zip
./ngrok authtoken YOUR_TOKEN
```

### تشغيل Tunnel

```bash
./ngrok http 5000
./ngrok http 5001
./ngrok http 5002
```

---

## 4. Google Drive Integration

### تحميل النماذج

```python
from google.colab import drive
drive.mount('/content/drive')

model_path = '/content/drive/MyDrive/AI_Interior_Project/models/segformer_struct/final/'
model = load_model(model_path)
```

### حفظ النتائج

```python
import os

results_dir = '/content/drive/MyDrive/AI_Interior_Project/artifacts/results/'
os.makedirs(results_dir, exist_ok=True)

result_image.save(f'{results_dir}result_{image_id}.jpg')
```

---

## 5. مسارات Google Drive المتفق عليها

```
/content/drive/MyDrive/AI_Interior_Project/
├── models/
│   ├── room_classifier/
│   │   └── model.pth
│   ├── segformer_struct/
│   │   └── final/
│   │       ├── config.json
│   │       ├── model.safetensors
│   │       └── preprocessor_config.json
│   └── sam/
│       └── sam_vit_h.pth
├── data/
│   └── NYU_Depth_Dataset/
│       ├── train/
│       └── test/
└── artifacts/
    ├── masks/
    │   └── mask_*.png
    └── results/
        └── result_*.jpg
```

---

## 6. معالجة الأخطاء

```python
@app.errorhandler(Exception)
def handle_error(error):
    return jsonify({
        'success': False,
        'error': str(error),
        'timestamp': datetime.now().isoformat()
    }), 500
```

---

## 7. اختبار الـ APIs

### اختبار Segmentation

```bash
curl -X POST https://your-colab-instance.ngrok.io/segment \
  -F "image=@room.jpg" \
  -F "surfaceMode=both"
```

### اختبار Style Transfer

```bash
curl -X POST https://your-colab-instance.ngrok.io/transform \
  -F "image=@room.jpg" \
  -F "color=#8B6F47" \
  -F "material=wood"
```

### اختبار Room Classifier

```bash
curl -X POST https://your-colab-instance.ngrok.io/classify \
  -F "image=@room.jpg"
```

---

## 8. الأداء والتحسينات

### تخزين مؤقت للنتائج

```python
from functools import lru_cache
import hashlib

@lru_cache(maxsize=100)
def get_cached_result(image_hash):
    return load_from_cache(image_hash)

def hash_image(image):
    return hashlib.md5(image.tobytes()).hexdigest()
```

### معالجة متوازية

```python
from concurrent.futures import ThreadPoolExecutor

executor = ThreadPoolExecutor(max_workers=4)

def process_batch(images):
    futures = [executor.submit(process_image, img) for img in images]
    return [f.result() for f in futures]
```

---

## 9. المراقبة والسجلات

```python
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@app.route('/segment', methods=['POST'])
def segment():
    logger.info('Segmentation request received')
    try:
        # Process
        logger.info('Segmentation completed successfully')
        return jsonify({'success': True})
    except Exception as e:
        logger.error(f'Segmentation error: {str(e)}')
        return jsonify({'success': False, 'error': str(e)})
```

---

## 10. الأمان

### التحقق من الملفات

```python
ALLOWED_EXTENSIONS = {'jpg', 'jpeg', 'png', 'gif'}
MAX_FILE_SIZE = 50 * 1024 * 1024

def validate_file(file):
    if file.size > MAX_FILE_SIZE:
        raise ValueError('File too large')
    
    ext = file.filename.rsplit('.', 1)[1].lower()
    if ext not in ALLOWED_EXTENSIONS:
        raise ValueError('Invalid file type')
```

### تشفير البيانات

```python
from cryptography.fernet import Fernet

cipher = Fernet(key)

def encrypt_data(data):
    return cipher.encrypt(data.encode())

def decrypt_data(encrypted_data):
    return cipher.decrypt(encrypted_data).decode()
```

---

## 11. استكشاف الأخطاء

### المشكلة: Connection Timeout
**الحل:** تحقق من ngrok tunnel وأعد تشغيله

### المشكلة: Model Not Found
**الحل:** تأكد من وجود النموذج في Google Drive

### المشكلة: Memory Error
**الحل:** استخدم GPU في Colab وقلل حجم الصور

---

## 12. الخطوات التالية

1. تدريب نماذج مخصصة
2. تحسين الأداء
3. إضافة المزيد من الأوضاع
4. دعم الفيديو

---

**آخر تحديث:** ديسمبر 2024
**الإصدار:** 1.0.0
