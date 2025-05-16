import ollama
import asyncio
import base64
import os
from typing import List
from models import ModelType, get_model_info, get_available_models, get_current_model


async def describe_objects(image_path: str, objects: List[str], model: ModelType = None) -> str:
    """
    Version corrigée avec gestion synchrone/asynchrone
    """
    if not objects:
        raise ValueError("La liste d'objets ne peut pas être vide")

    model = model or get_current_model()
    available_models = [m.value for m in get_available_models()]

    if model not in available_models:
        raise ValueError(f"Modèle {model} non supporté. Disponibles: {', '.join(available_models)}")

    if not os.path.exists(image_path):
        raise FileNotFoundError(f"Image non trouvée: {image_path}")

    try:
        print(f"\n[DEBUG] Début description avec modèle {model}")
        print(f"Chemin image: {image_path}")
        print(f"Objets à analyser: {objects}")

        model_info = get_model_info(model)
        prompt = build_adaptive_prompt(objects, model_info.type == "vision")

        # Solution hybride synchrone/asynchrone
        if model_info.type == "vision":
            with open(image_path, "rb") as image_file:
                image_data = base64.b64encode(image_file.read()).decode("utf-8")
            response = await run_ollama_sync(model, prompt, image_data)
        else:
            response = await run_ollama_sync(model, prompt)
        print(f"\n[DEBUG] Description brute du modèle:")
        print(response['message']['content'])
        return format_response(response, objects, model_info.type == "vision")

    except Exception as e:
        error_msg = f"Erreur avec {model}: {str(e)}"
        print(f"\n[ERROR] Erreur dans describe_objects: {str(e)}")

        print(f"[ERROR] {error_msg}")
        raise RuntimeError(error_msg)


async def run_ollama_sync(model: str, prompt: str, image_data: str = None):
    """Wrapper pour exécuter Ollama de manière asynchrone"""
    loop = asyncio.get_event_loop()
    messages = [{'role': 'user', 'content': prompt}]

    if image_data:
        messages[0]['images'] = [image_data]

    try:
        # Exécution dans un thread séparé
        return await loop.run_in_executor(
            None,
            lambda: ollama.chat(model=model, messages=messages)
        )
    except Exception as e:
        raise RuntimeError(f"Erreur Ollama: {str(e)}")


def build_adaptive_prompt(objects: List[str], is_vision_model: bool) -> str:
    """Construit un prompt adapté au type de modèle"""
    object_list = ", ".join(objects)

    if is_vision_model:
        return f"""
[INSTRUCTIONS]
Analyse cette image et décris UNIQUEMENT ces objets: {object_list}.

Règles:
- Description visuelle concise
- Une ligne par objet: "objet: description"
- Maximum 2 phrases par objet
- Français uniquement
"""
    else:
        return f"""
[INSTRUCTIONS]
Génère une description générique de ces objets: {object_list}.

Règles:
- Description conceptuelle
- Une ligne par objet: "objet: description"
- Maximum 2 phrases par objet
- Français uniquement
"""


def format_response(response: dict, objects: List[str], is_vision: bool) -> str:
    """Formate la réponse du modèle"""
    if not response or 'message' not in response or 'content' not in response['message']:
        raise ValueError("Réponse du modèle invalide")

    content = response['message']['content'].strip()

    # Pour les modèles vision, vérifier que tous les objets ont été traités
    if is_vision:
        missing = [obj for obj in objects if obj.lower() not in content.lower()]
        if missing:
            print(f"[WARNING] Objets non décrits: {', '.join(missing)}")

    return content
