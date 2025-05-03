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
resumer ce text {text} dans une simple paragraphe en 3 phrases maximum
"""

    try:
        response = ollama.chat(
            model='llava',
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