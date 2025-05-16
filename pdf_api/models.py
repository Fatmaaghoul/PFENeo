# models.py
from typing import List, Dict, Literal
import ollama
from pydantic import BaseModel

ModelType = Literal['llava:7b', 'gemma3:4b']

class ModelInfo(BaseModel):
    name: str
    value: ModelType
    type: Literal['vision', 'text']
    description: str

AVAILABLE_MODELS: List[ModelInfo] = [
    ModelInfo(
        name="LLaVA 7B",
        value="llava:7b",
        type="vision",
        description="Modèle vision pour analyse d'images"
    ),
    ModelInfo(
        name="Gemma3 4B",
        value="gemma3:4b",
        type="text",
        description="Modèle texte optimisé pour traitement linguistique"
    )
]

DEFAULT_MODEL = "llava:7b"
current_model = DEFAULT_MODEL

async def list_ollama_models() -> List[str]:  # Changed from list_models to list_ollama_models
    """List available Ollama models"""
    try:
        response = ollama.list()
        return [model['name'] for model in response.get('models', [])]
    except Exception as e:
        print(f"Error fetching Ollama models: {str(e)}")
        return []

def get_available_models() -> List[ModelInfo]:
    return AVAILABLE_MODELS

def get_current_model() -> ModelType:
    return current_model

def set_current_model(modelValue: ModelType) -> bool:
    global current_model
    if modelValue not in [m.value for m in AVAILABLE_MODELS]:
        return False
    current_model = modelValue
    return True

def get_model_info(modelValue: ModelType) -> ModelInfo:
    for model in AVAILABLE_MODELS:
        if model.value == modelValue:
            return model
    raise ValueError(f"Modèle {modelValue} non trouvé")