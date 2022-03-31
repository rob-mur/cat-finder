import os
import tempfile
from pathlib import Path
from typing import List

import numpy as np
import requests
from fastapi import FastAPI, Query
from keras_preprocessing.image import img_to_array, load_img
from requests.adapters import HTTPAdapter, Retry
import tensorflow as tf

app = FastAPI()
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 6.0; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0'}

interpreter = tf.lite.Interpreter("cat_dog_neither.tflite")
interpreter.resize_tensor_input(0, [1, 150, 150, 3])
interpreter.allocate_tensors()

s = requests.Session()
retries = Retry(total=5, backoff_factor=1, status_forcelist=[502, 503, 504])
s.mount('http://', HTTPAdapter(max_retries=retries))


@app.get("/health/")
async def health_check():
    return "Service available"


@app.get("/predict/")
async def predict(urls: List[str] = Query(None)):
    result = await make_predictions(urls)
    return result


async def make_predictions(urls):
    predictions = [2] * len(urls)
    with tempfile.TemporaryDirectory() as tempDir:
        input_path = os.path.join(tempDir, "inputs")
        os.mkdir(input_path)
        await download_files(input_path, urls)
        for imgPath in os.listdir(input_path):
            img = load_img(os.path.join(input_path, imgPath), target_size=(150, 150))
            img = img_to_array(img).astype(np.uint8)
            img = np.array([img])
            interpreter.set_tensor(0, img)
            interpreter.invoke()
            output_data = interpreter.get_tensor(186)[0]
            model_prediction = np.argmax(output_data)
            idx = int(Path(imgPath).stem)
            predictions[idx] = model_prediction.item()
    return predictions


async def download_files(temp_dir, urls):
    succeeded_downloads = 0
    for i, url in enumerate(urls):
        try:
            await download_file(i, temp_dir, url)
            succeeded_downloads += 1
        except requests.exceptions.ConnectionError:
            print("Couldn't download file")
    return succeeded_downloads


async def download_file(i, tempDir, url):
    image_data = s.get(url, headers=headers).content
    file_name = os.path.join(tempDir, str(i) + ".jpg")
    with open(file_name, 'wb') as tempFile:
        tempFile.write(image_data)
