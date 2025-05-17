from fastapi import FastAPI, HTTPException, Body
from fastapi.responses import JSONResponse
from pydantic import BaseModel
from typing import List, Literal, Optional

from objects import download_image_from_url, count_object_occurrences
from describe import describe_objects
from resume import resumer
from translate import translate_to_french
from models import (
    get_available_models,
    get_current_model,
    list_ollama_models,
    set_current_model,
    get_model_info,
    ModelInfo,
    ModelType,
    DEFAULT_MODEL
)

app = FastAPI(title="Analyse Objet-Texte")

# --- MODELES Pydantic ---

class CurrentModelResponse(BaseModel):
    model_config = {"protected_namespaces": ()}
    name: str
    value: str
    is_default: bool
    type: str
    description: str

class UpdateModelRequest(BaseModel):
    model_config = {"protected_namespaces": ()}
    modelValue: ModelType

class ModelListItem(BaseModel):
    name: str
    value: str
    type: str
    description: str

class ModelsListResponse(BaseModel):
    models: List[ModelListItem]
    default: str

class AnalysisRequest(BaseModel):
    image_url: str
    text: str

class DescribeRequest(BaseModel):
    image_url: str
    objects: List[str]
    model: Optional[ModelType] = None  # Rendre le modèle optionnel


# --- ENDPOINTS ---

@app.get("/current-model", response_model=CurrentModelResponse)
async def api_get_current_model():
    current = get_current_model()
    model_info = get_model_info(current)
    return {
        "name": model_info.name,
        "value": model_info.value,
        "is_default": model_info.value == DEFAULT_MODEL,
        "type": model_info.type,
        "description": model_info.description
    }

@app.post("/update-model")
async def api_update_model(request: UpdateModelRequest):
    if not set_current_model(request.modelValue):
        raise HTTPException(
            status_code=400,
            detail="Modèle non supporté"
        )
    model_info = get_model_info(request.modelValue)
    return {
        "message": "Modèle mis à jour avec succès",
        "new_model": {
            "name": model_info.name,
            "value": model_info.value,
            "is_default": model_info.value == DEFAULT_MODEL,
            "type": model_info.type,
            "description": model_info.description
        }
    }

@app.get("/ollama-models")
async def api_get_ollama_models():
    try:
        models = await list_ollama_models()
        return {"models": models}
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Erreur lors de la récupération des modèles Ollama: {str(e)}"
        )

@app.get("/available-models")
async def api_available_models():
    return {
        "models": [
            {"name": "LLaVA 7B", "value": "llava:7b", "type": "vision", "description": "Modèle visuel basé sur LLaVA"},
            {"name": "Gemma3 4B", "value": "gemma3:4b", "type": "vision", "description": "Modèle visuel basé sur Gemma"}
        ],
        "default": DEFAULT_MODEL
    }

@app.post("/analyze")
async def analyze(request: AnalysisRequest):
    try:
        image_path = await download_image_from_url(request.image_url)
        result = count_object_occurrences(
            image_paths=[image_path],
            texts=[request.text]
        )
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erreur lors de l'analyse : {str(e)}")
@app.post("/describe")
async def describe(request: DescribeRequest):
    try:
        # Validation
        if not request.image_url or not request.objects:
            raise HTTPException(
                status_code=400,
                detail="URL d'image et liste d'objets sont requis"
            )

        # Appel du traitement
        description = await describe_objects(
            image_url=request.image_url,
            objects=request.objects,
            model=request.model
        )

        return {
            "description": description,
            "model_used": request.model or get_current_model(),
            "status": "success"
        }

    except HTTPException:
        raise
    except Exception as e:
        print(f"\n[ERROR] Erreur lors de la description: {str(e)}")
        raise HTTPException(
            status_code=500,
            detail=f"Erreur lors de la description: {str(e)}"
        )
    
@app.post("/resumer")
def get_resumer(body: dict = Body(...)):
    text = body.get("text")
    if not text:
        return JSONResponse(status_code=400, content={"error": "Missing text"})
    summary = resumer(text)
    return {"summary": summary}

@app.post("/translate")
def translate(body: dict = Body(...)):
    try:
        text = body.get("text")
        if not isinstance(text, str) or not text.strip():
            raise HTTPException(status_code=400, detail="Le champ 'text' doit être une chaîne non vide")
        translated_text = translate_to_french(text)
        return {"translated_text": translated_text}
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erreur lors de la traduction : {str(e)}")
