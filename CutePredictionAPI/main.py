import os
import tempfile
from pathlib import Path
from typing import List

import asyncio
import numpy as np
from fastapi import FastAPI, Query
from keras_preprocessing.image import img_to_array, load_img
import tensorflow as tf
import aiohttp
import aiofiles

app = FastAPI()
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 6.0; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0'}

interpreter = tf.lite.Interpreter("cat_dog_neither.tflite")
output_details = interpreter.get_output_details()


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
        interpreter.resize_tensor_input(0, [len(os.listdir(input_path)), 150, 150, 3])
        interpreter.allocate_tensors()
        image_names = os.listdir(input_path)
        images = [img_to_array(load_img(os.path.join(input_path, x), target_size=(150, 150))) for x in image_names]
        interpreter.set_tensor(0, images)
        interpreter.invoke()
        output_data = interpreter.get_tensor(output_details[0]['index'])
        for i, prediction in enumerate(output_data):
            model_prediction = np.argmax(prediction)
            idx = int(Path(image_names[i]).stem)
            predictions[idx] = model_prediction.item()
    return predictions


async def download_files(temp_dir, urls):
    coros = [fetch_file(i, temp_dir, urls) for i, urls in enumerate(urls)]
    await asyncio.gather(*coros)


async def fetch_file(i, temp_dir, url):
    try:
        await download_file(i, temp_dir, url)
    except aiohttp.ClientConnectionError:
        print("Couldn't download file")
    except asyncio.TimeoutError:
        print("Couldn't download file")


async def download_file(i, temp_dir, url):
    file_name = os.path.join(temp_dir, str(i) + ".jpg")
    async with aiohttp.ClientSession() as session:
        async with session.get(url, headers=headers, timeout=1) as resp:
            f = await aiofiles.open(file_name, mode='wb')
            await f.write(await resp.read())
            await f.close()
