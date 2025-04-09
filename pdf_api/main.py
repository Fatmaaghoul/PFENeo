from fastapi import FastAPI, Body
from fastapi.responses import JSONResponse
from utils import download_pdf_from_url, extract_images_and_text
import os

app = FastAPI()

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
