from typing import List
import requests
import tempfile
import os
import numpy as np
from PIL import UnidentifiedImageError

from keras_preprocessing.image import load_img, img_to_array
from requests.adapters import HTTPAdapter, Retry
from tensorflow import keras

from fastapi import FastAPI, Query

app = FastAPI()
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 6.0; WOW64; rv:24.0) Gecko/20100101 Firefox/24.0'}

model = keras.models.load_model("cat_dog_neither.h5")

s = requests.Session()
retries = Retry(total=5, backoff_factor=1, status_forcelist=[ 502, 503, 504 ])
s.mount('http://', HTTPAdapter(max_retries=retries))


@app.get("/predict/")
async def predict(urls: List[str] = Query(None)):
    return [download_and_predict(url) for url in urls]


def download_and_predict(url: str):
    try:
        image_data = s.get(url, headers=headers).content
        with tempfile.TemporaryDirectory() as tempDir:
            file_name = os.path.join(tempDir, "image.jpg")
            with open(file_name, 'wb') as tempFile:
                tempFile.write(image_data)
            photo = load_img(file_name, target_size=(224, 224))
            image_array = img_to_array(photo)
            image_array = image_array.reshape(1, 224, 224, 3)
            image_array = image_array.astype('float32')
            image_array -= [123.68, 116.779, 103.939]
            result = model.predict(image_array)[0]
            result = np.where(result == 1.)
            if len(result[0]) == 0:
                print("model couldn't decide a photo")
                return 2
            return result[0].item(0)
    except UnidentifiedImageError:
        print("PIL Couldn't open the image")
        return 2
    except requests.exceptions.ConnectionError:
        print("Couldn't download the image")
        return 2
