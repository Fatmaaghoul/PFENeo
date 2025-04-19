import concurrent.futures
from typing import Dict
import torch
from ultralytics import YOLO
from PIL import Image
import requests
from flask import Flask, request, jsonify
import logging
import base64
from io import BytesIO
import ollama
from functools import lru_cache
import cloudinary
import cloudinary.uploader

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize Cloudinary
cloudinary.config(
    cloud_name="dzkoqiz6w",
    api_key="232375459128386",
    api_secret="1UxaoFUzvUYgMJROqPARnIq9lL0",
    secure=True
)

class DocumentAnalyzer:
    def __init__(self):
        self.yolo_model = YOLO('yolov8n.pt').half()
        self.yolo_model.fuse()
        self.ollama_model = 'llava:7b'

    @lru_cache(maxsize=100)
    def ollama_generate_description(self, img_bytes: bytes) -> str:
        try:
            img_base64 = base64.b64encode(img_bytes).decode('utf-8')
            response = ollama.chat(
                model=self.ollama_model,
                messages=[{
                    'role': 'user',
                    'content': "Décris cette image en français. Sois concis.",
                    'images': [img_base64]
                }],
                options={'timeout': 30}
            )
            return response['message']['content']
        except Exception as e:
            logger.error(f"Erreur Ollama: {str(e)}")
            return "Description non disponible"

    def analyze_image(self, image_url: str) -> Dict:
        try:
            # Download image
            response = requests.get(image_url, timeout=10)
            img = Image.open(BytesIO(response.content))
            
            # Resize if too large
            if max(img.size) > 1024:
                img.thumbnail((1024, 1024), Image.Resampling.LANCZOS)
            
            # YOLO analysis
            yolo_results = self.yolo_model(img, verbose=False)
            
            # Object detection
            detected_objects = []
            for result in yolo_results:
                for box in result.boxes:
                    detected_objects.append({
                        "label": result.names[int(box.cls)],
                        "confidence": float(box.conf),
                        "bbox": [float(x) for x in box.xyxy[0].tolist()],
                        "area": int((box.xyxy[0][2] - box.xyxy[0][0]) * (box.xyxy[0][3] - box.xyxy[0][1]))
                    })
            
            # Prepare image for upload and description
            img_byte_arr = BytesIO()
            img.save(img_byte_arr, format='JPEG', quality=85)
            img_bytes = img_byte_arr.getvalue()
            
            # Parallel processing
            with concurrent.futures.ThreadPoolExecutor() as executor:
                upload_future = executor.submit(
                    cloudinary.uploader.upload,
                    BytesIO(img_bytes),
                    folder="annotated_images"
                )
                desc_future = executor.submit(
                    self.ollama_generate_description,
                    img_bytes
                )
                
                upload_result = upload_future.result()
                description = desc_future.result()
            
            return {
                "original_image": image_url,
                "annotated_image": upload_result['secure_url'],
                "detected_objects": detected_objects,
                "llava_description": description,
                "status": "success"
            }
            
        except Exception as e:
            logger.error(f"Erreur analyse image: {str(e)}")
            return {
                "original_image": image_url,
                "error": str(e),
                "status": "error"
            }

app = Flask(__name__)
analyzer = DocumentAnalyzer()

@app.route('/analyze', methods=['POST'])
def analyze():
    try:
        data = request.get_json()
        
        if not data:
            return jsonify({"error": "No data provided"}), 400
            
        if 'image_url' in data:
            result = analyzer.analyze_image(data['image_url'])
            return jsonify(result)
        elif 'images' in data:
            results = []
            for img in data['images']:
                if 'image_url' in img:
                    result = analyzer.analyze_image(img['image_url'])
                    result['image_id'] = img.get('id', '')
                    results.append(result)
            
            return jsonify({
                "status": "success",
                "images": results,
                "stats": {
                    "images_analyzed": len(results),
                    "processing_time_sec": 0  # Vous pouvez ajouter un timer réel ici
                }
            })
        else:
            return jsonify({"error": "Format de requête non reconnu"}), 400

    except Exception as e:
        logger.error(f"Erreur API: {str(e)}")
        return jsonify({"status": "error", "message": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)