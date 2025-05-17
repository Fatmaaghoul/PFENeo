import ollama
import asyncio
import base64
import tempfile
import aiohttp
import os
from typing import List
from models import ModelType, get_model_info, get_available_models, get_current_model

async def download_image_to_temp(url: str) -> str:
    """Télécharge une image dans un fichier temporaire et retourne son chemin"""
    try:
        async with aiohttp.ClientSession() as session:
            async with session.get(url) as resp:
                if resp.status != 200:
                    raise ValueError(f"Échec du téléchargement (HTTP {resp.status})")
                
                # Crée un fichier temporaire avec extension .jpg
                temp_file = tempfile.NamedTemporaryFile(delete=False, suffix=".jpg")
                temp_file.write(await resp.read())
                temp_path = temp_file.name
                temp_file.close()
                return temp_path
                
    except Exception as e:
        raise RuntimeError(f"Erreur de téléchargement: {str(e)}")

async def describe_objects(image_url: str, objects: List[str], model: ModelType = None) -> str:
    """
    Version corrigée avec :
    - Téléchargement temporaire
    - Nettoyage automatique
    - Gestion robuste des erreurs
    """
    temp_path = None
    try:
        # Validation des entrées
        if not objects:
            raise ValueError("La liste d'objets ne peut pas être vide")
        
        if not image_url.startswith(('http://', 'https://')):
            raise ValueError("URL d'image invalide")

        model = model or get_current_model()
        model_info = get_model_info(model)

        print(f"\n[DEBUG] Début description avec modèle {model}")
        print(f"URL Image: {image_url}")
        print(f"Objets à analyser: {objects}")

        # Téléchargement de l'image
        temp_path = await download_image_to_temp(image_url)
        print(f"[DEBUG] Image téléchargée dans: {temp_path}")

        # Préparation du prompt
        prompt = build_adaptive_prompt(objects, model_info.type == "vision")

        if model_info.type == "vision":
            with open(temp_path, "rb") as f:
                image_data = base64.b64encode(f.read()).decode("utf-8")
            response = await run_ollama_sync(model, prompt, image_data)
        else:
            response = await run_ollama_sync(model, prompt)

        return format_response(response, objects, model_info.type == "vision")

    except Exception as e:
        error_msg = f"Erreur avec {model}: {str(e)}"
        print(f"\n[ERROR] {error_msg}")
        raise RuntimeError(error_msg)
        
    finally:
        # Nettoyage garantie du fichier temporaire
        if temp_path and os.path.exists(temp_path):
            try:
                os.unlink(temp_path)
                print(f"[DEBUG] Fichier temporaire supprimé: {temp_path}")
            except Exception as e:
                print(f"[WARNING] Échec suppression fichier temporaire: {str(e)}")


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
Analyse cette image et fournis une description UNIQUE qui inclut tous ces objets: {object_list}.

Règles:
- Décris la scène de manière cohérente en montrant les relations entre les objets
- Mentionne la position relative des objets dans l'image
- Décris les interactions entre les objets le cas échéant
- Maximum 5 phrases au total
- Français uniquement
- Structure: "Description: [texte descriptif unifié]"
"""
    else:
        return f"""
[INSTRUCTIONS]
Génère une description unifiée de ces objets: {object_list}.

Règles:
- Décris comment ces objets pourraient être liés dans une scène typique
- Mentionne leurs relations potentielles
- Maximum 3 phrases au total
- Français uniquement
- Structure: "Description: [texte descriptif unifié]"
"""
def format_response(response: dict, objects: List[str], is_vision: bool) -> str:
    """Formate la réponse du modèle"""
    if not response or 'message' not in response or 'content' not in response['message']:
        raise ValueError("Réponse du modèle invalide")

    content = response['message']['content'].strip()
    
    # Vérifie si tous les objets sont mentionnés (pour les modèles vision)
    if is_vision:
        missing = [obj for obj in objects if obj.lower() not in content.lower()]
        if missing:
            print(f"[WARNING] Objets non mentionnés: {', '.join(missing)}")
            # Ajoute les objets manquants à la description
            content += f" (Note: l'image contient aussi {', '.join(missing)})"

    return content