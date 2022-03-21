import os
import tempfile
from pathlib import Path
from typing import List


import numpy as np
import requests
from fastapi import FastAPI, Query
from keras_preprocessing.image import ImageDataGenerator
from requests.adapters import HTTPAdapter, Retry
from tensorflow import keras

app = FastAPI()
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 6.0; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0'}

model = keras.models.load_model("cat_dog_neither.h5")

s = requests.Session()
retries = Retry(total=5, backoff_factor=1, status_forcelist=[ 502, 503, 504 ])
s.mount('http://', HTTPAdapter(max_retries=retries))


@app.get("/predict/")
async def predict(urls: List[str] = Query(None)):
    return await make_predictions(urls)


async def make_predictions(urls):
    predictions = [2] * len(urls)
    datagen = ImageDataGenerator(rescale=1.0 / 255.0)
    datagen.mean = [123.68, 116.779, 103.939]
    with tempfile.TemporaryDirectory() as tempDir:
        input_path = os.path.join(tempDir, "inputs")
        os.mkdir(input_path)
        await download_files(input_path, urls)
        generator = datagen.flow_from_directory(
            tempDir,
            target_size=(224, 224),
            shuffle=False,
            batch_size=1)
        filenames = generator.filenames
        model_predictions = model.predict(generator, steps=len(filenames), verbose=1)
        model_predictions = np.argmax(model_predictions, axis=1)
        for i, filename in enumerate(filenames):
            idx = int(Path(filename).stem)
            predictions[idx] = model_predictions[i].item()
    return predictions


async def download_files(tempDir, urls):
    for i, url in enumerate(urls):
        try:
            await download_file(i, tempDir, url)
        except requests.exceptions.ConnectionError:
            print("Couldn't download file")


async def download_file(i, tempDir, url):
    image_data = s.get(url, headers=headers).content
    file_name = os.path.join(tempDir, str(i) + ".jpg")
    with open(file_name, 'wb') as tempFile:
        tempFile.write(image_data)

