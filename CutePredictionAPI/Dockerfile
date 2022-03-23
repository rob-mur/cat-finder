FROM python:3.8

ARG AWS_DEFAULT_REGION='eu-west-2'
ARG AWS_ACCESS_KEY_ID
ARG AWS_SECRET_ACCESS_KEY


WORKDIR .

COPY . .

RUN pip install awscli

RUN aws s3 cp s3://cat-dog-neither-ml/cat_dog_neither.h5 cat_dog_neither.h5

RUN pip install --no-cache-dir --upgrade -r /requirements.txt


CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "80"]