from fastapi import FastAPI, Body
import os
from fastapi.responses import JSONResponse
from extraction import download_pdf_from_url, extract_images_and_text
from anlyse import (
    download_image_from_url,
    detect_objects_yolo,
    translate_objects_to_french,
    get_synonyms,
    extract_mentioned_objects,
    generate_description
)
from dotenv import load_dotenv

load_dotenv()

app = FastAPI()

@app.post("/describe_image")
async def describe_image(body: dict = Body(...)):
    image_url = body.get("image_url")
    text = body.get("text")

    if not image_url or not text:
        return JSONResponse(status_code=400, content={"error": "Missing image_url or text"})

    try:
        # Télécharger l'image
        image_path = await download_image_from_url(image_url)

        # Détecter les objets avec YOLO
        detected_objects = detect_objects_yolo(image_path)
        
        # Récupérer les synonymes des objets détectés et les traduire
        detected_objects_with_synonyms = get_synonyms(detected_objects)
        
        # Extraire les objets mentionnés dans le texte en utilisant les objets détectés + synonymes
        mentioned_objects = extract_mentioned_objects(text, detected_objects_with_synonyms)
        
        # Objets à décrire (intersection entre objets mentionnés et objets détectés + synonymes)
        objects_to_describe = mentioned_objects.intersection(detected_objects_with_synonyms)
        
        # Générer la description avec LLaMA Vision
        description = generate_description(image_path, objects_to_describe, text)
        
        # Nettoyer le fichier temporaire
        os.remove(image_path)

        return {
            "detected_objects": list(detected_objects_with_synonyms),
            "mentioned_objects": list(mentioned_objects),
            "objects_to_describe": list(objects_to_describe),
            "description": description
        }

    except Exception as e:
        return JSONResponse(status_code=500, content={"error": str(e)})

@app.post("/extract")
def extract_pdf_data(body: dict = Body(...)):
    pdf_url = body.get("pdf_url")
    if not pdf_url:
        return JSONResponse(status_code=400, content={"error": "Missing pdf_url"})

    try:
        pdf_path = download_pdf_from_url(pdf_url)
        text, image_urls = extract_images_and_text(pdf_path)

        os.remove(pdf_path)

        return {
            "text": text,
            "images": image_urls
        }

    except Exception as e:
        return JSONResponse(status_code=500, content={"error": str(e)})