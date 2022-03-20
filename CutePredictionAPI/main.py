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
        print(f"Successfully downloaded {len(filenames)} files")
        model_predictions = model.predict(generator, steps=len(filenames))
        for i, filename in enumerate(filenames):
            idx = int(Path(filename).stem)
            prediction = model_predictions[i]
            prediction = np.where(prediction >= 0.9)
            print(prediction)
            if len(prediction[0]) == 0:
                print("model couldn't decide a photo")
            else:
                predictions[idx] = prediction[0].item(0)
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

