import aiohttp
from ultralytics import YOLO
import ollama
import torch

# Vérifier la disponibilité du GPU
device = 'cuda' if torch.cuda.is_available() else 'cpu'
print(f"[INFO] Utilisation du dispositif : {device}")


# Générer une resumer
def resumer(text: str = "") -> str:
    prompt = f"""
Résume le texte suivant en un seul paragraphe clair et concis, en conservant les idées principales :

{text}
"""


    try:
        response = ollama.chat(
            model='llava:7b',
            messages=[{
                'role': 'user',
                'content': prompt,
            }]
        )
        print(f"[INFO] Utilisation du dispositif : {device}")
        print(f"[INFO] resumé : {response['message']['content']}")
        return response['message']['content']
    except Exception as e:
        print(f"[ERROR] Erreur lors de la génération de la description : {str(e)}")
        return f"Erreur lors de la génération de la description : {str(e)}"