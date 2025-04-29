import aiohttp
from ultralytics import YOLO
from deep_translator import GoogleTranslator
import ollama
import tempfile
import os
import nltk
from nltk.corpus import wordnet
import torch

# Vérifier la disponibilité du GPU
device = 'cuda' if torch.cuda.is_available() else 'cpu'
print(f"[INFO] Utilisation du dispositif : {device}")

# Initialisation du traducteur
translator = GoogleTranslator(source='en', target='fr')

# Chargement du modèle YOLO avec le dispositif spécifié
model = YOLO('yolov8n.pt')
model.to(device)  # Déplacer le modèle vers le GPU si disponible
print(f"[INFO] Modèle YOLO chargé sur : {device}")

# Fonction pour télécharger une image depuis une URL
async def download_image_from_url(image_url: str) -> str:
    async with aiohttp.ClientSession() as session:
        async with session.get(image_url) as response:
            if response.status == 200:
                # Enregistrer l'image temporairement
                with tempfile.NamedTemporaryFile(delete=False, suffix=".png") as temp_img:
                    temp_img.write(await response.read())
                    temp_img_path = temp_img.name
                print(f"[INFO] Image téléchargée et enregistrée à : {temp_img_path}")
                return temp_img_path
            else:
                raise Exception(f"Unable to download image. Status code: {response.status}")

# Détecter les objets avec YOLO
def detect_objects_yolo(image_path: str) -> set:
    # Effectuer la détection avec YOLO sur le dispositif spécifié
    results = model(image_path, device=device)
    detected_objects = set()
    for result in results:
        for box in result.boxes:
            cls_id = int(box.cls)
            label = model.names[cls_id]
            detected_objects.add(label.lower())
    print(f"[INFO] Objets détectés par YOLO : {detected_objects}")
    return detected_objects

# Traduire les objets en français
def translate_objects_to_french(objects: set) -> set:
    translated_objects = set()
    for obj in objects:
        try:
            translated = translator.translate(obj).lower()
            translated_objects.add(translated)
        except Exception as e:
            print(f"[WARNING] Erreur de traduction pour '{obj}' : {e}")
            translated_objects.add(obj)  # Garder l'original en cas d'erreur
    print(f"[INFO] Objets traduits en français : {translated_objects}")
    return translated_objects

# Récupérer les synonymes des objets détectés et les traduire
def get_synonyms(objects: set) -> set:
    synonyms = set()
    for obj in objects:
        # Récupérer les synonymes via WordNet, limités aux noms (objets physiques)
        for syn in wordnet.synsets(obj, pos=wordnet.NOUN, lang='eng'):
            for lemma in syn.lemmas():
                synonym = lemma.name().lower().replace('_', ' ')
                synonyms.add(synonym)
        # Ajouter l'objet original
        synonyms.add(obj)
    print(f"[INFO] Synonymes en anglais : {synonyms}")
    
    # Traduire les objets et synonymes en français
    translated_synonyms = translate_objects_to_french(synonyms)
    print(f"[INFO] Synonymes traduits en français : {translated_synonyms}")
    
    return translated_synonyms

# Extraire les objets mentionnés dans le texte
def extract_mentioned_objects(text: str, reference_objects: set) -> set:
    text = text.lower()
    mentioned_objects = set()
    
    # Parcourir le texte pour trouver des correspondances avec les objets de référence
    for obj in reference_objects:
        if obj in text:
            mentioned_objects.add(obj)
    
    print(f"[INFO] Objets mentionnés dans le texte : {mentioned_objects}")
    return mentioned_objects

# Générer une description avec LLaMA Vision via ollama
def generate_description(image_path: str, objects_to_describe: set, text: str = "") -> str:
    print(f"[INFO] Objets à décrire : {objects_to_describe}")
    if not objects_to_describe:
        print("[INFO] Aucun objet à décrire, retour : 'Il n'y a pas d'objets'")
        return "Il n'y a pas d'objets"

    object_list = ", ".join(objects_to_describe)
    prompt = f"""
Analyse attentivement l'image fournie et concentre-toi exclusivement sur les objets suivants : {object_list}.

Ta tâche est de :
- Décrire uniquement ces objets, en ignorant tout autre élément, même s'il est clairement visible ou important dans l'image.
- Ne jamais mentionner un objet ou un détail qui ne figure pas explicitement dans la liste {object_list}.
- Pour chaque objet, précise son apparence (forme, couleur, taille, texture, état visuel...).
- Décris sa position approximative dans l'image (ex : en haut à gauche, au centre, proche du bord inférieur, etc.).
- Si pertinent, indique brièvement son environnement immédiat mais uniquement en lien avec l'objet concerné.

Important :
- Ignore totalement l'arrière-plan général, les éléments non mentionnés et les détails superflus.
- Concentre-toi uniquement sur les objets listés. Fais comme si le reste de l'image n'existait pas.
- Ne fais aucune hypothèse sur des objets absents de la liste, même s'ils paraissent évidents.

Ta réponse doit être rédigée en français clair, précis et descriptif.
Pas d'introduction, pas de conclusion. Va directement aux descriptions ciblées.
"""

    try:
        response = ollama.chat(
            # model='llama3.2-vision',
            model='llava',
            messages=[{
                'role': 'user',
                'content': prompt,
                'images': [image_path]
            }]
        )
        print(f"[INFO] Utilisation du dispositif : {device}")
        print(f"[INFO] Description générée : {response['message']['content']}")
        return response['message']['content']
    except Exception as e:
        print(f"[ERROR] Erreur lors de la génération de la description : {str(e)}")
        return f"Erreur lors de la génération de la description : {str(e)}"