import os
import sys

import tensorflow as tf
from tensorflow.keras.applications.inception_v3 import InceptionV3
from keras.layers import Dense, Flatten, Dropout, Input
from keras.models import Model
from keras.optimizer_v1 import rmsprop
from keras.preprocessing.image import img_to_array
from keras.preprocessing.image import load_img, ImageDataGenerator, save_img
from matplotlib import pyplot
from matplotlib.image import imread
from tqdm import tqdm
from keras import backend as K
from keras.callbacks import EarlyStopping, ModelCheckpoint
import keras
from tensorflow.python.framework.convert_to_constants import convert_variables_to_constants_v2

tf.compat.v1.disable_eager_execution()
config = tf.compat.v1.ConfigProto()
config.gpu_options.allow_growth = True
session = tf.compat.v1.Session(config=config)
K.set_session(session)


def plot():
    dog_training = os.path.join(dataRoot, "training_set/dogs")

    # plot first few images
    for i, filename in enumerate(os.listdir(dog_training)[:9]):
        # define subplot
        pyplot.subplot(330 + 1 + i)
        # load image pixels
        image = imread(os.path.join(dog_training, filename))
        # plot raw pixel data
        pyplot.imshow(image)
    # show the figure
    pyplot.show()


def resize_images(src_root, dest_root):
    load_images(src_root, dest_root, "cats")
    load_images(src_root, dest_root, "dogs")
    load_images(src_root, dest_root, "neither")


def load_images(src_root, dest_root, file_type):
    for file in tqdm(os.listdir(os.path.join(src_root, file_type))):
        photo = load_img(os.path.join(src_root, file_type, file), target_size=(150, 150))
        photo = img_to_array(photo)
        save_img(os.path.join(dest_root, file_type, file), photo)


def define_model():
    model = InceptionV3(input_shape=(150, 150, 3), include_top=False, weights='imagenet')
    for layer in model.layers:
        layer.trainable = False
    i = Input([None, None, 3], dtype=tf.uint8)
    x = tf.cast(i, tf.float32)
    x = tf.keras.applications.inception_v3.preprocess_input(x)
    x = model(x)

    x = Flatten()(x)
    x = Dense(1024, activation='relu')(x)
    x = Dropout(0.2)(x)
    predictions = Dense(3, activation='sigmoid')(x)
    model = Model(inputs=i, outputs=predictions)
    # compile model
    opt = rmsprop(lr=0.0001)
    model.compile(optimizer=opt, loss='categorical_crossentropy', metrics=['accuracy'])
    return model


def productionise_model():
    es = EarlyStopping(monitor='val_accuracy', mode='max', verbose=1, patience=50)
    mc = ModelCheckpoint('best_model.h5', monitor='val_accuracy', mode='max')
    cb_list = [es, mc]
    # define model
    model = define_model()
    # create data generator
    train_datagen = ImageDataGenerator(rotation_range=40, width_shift_range=0.2,
                                       height_shift_range=0.2, shear_range=0.2, zoom_range=0.2, horizontal_flip=True)
    test_datagen = ImageDataGenerator()
    # prepare iterators
    train_it = train_datagen.flow_from_directory(resTrainingRoot,
                                                 batch_size=32, target_size=(150, 150))
    test_it = test_datagen.flow_from_directory(resTestRoot, batch_size=32, target_size=(150, 150))
    # fit model
    history = model.fit(train_it, steps_per_epoch=len(train_it),
                        validation_data=test_it, validation_steps=len(test_it), epochs=1000, callbacks=cb_list,
                        verbose=1)
    summarize_diagnostics(history)


def summarize_diagnostics(history):
    # plot loss
    pyplot.subplot(211)
    pyplot.title('Cross Entropy Loss')
    pyplot.plot(history.history['loss'], color='blue', label='train')
    pyplot.plot(history.history['val_loss'], color='orange', label='test')
    # plot accuracy
    pyplot.subplot(212)
    pyplot.title('Classification Accuracy')
    pyplot.plot(history.history['accuracy'], color='blue', label='train')
    pyplot.plot(history.history['val_accuracy'], color='orange', label='test')
    # save plot to file
    filename = sys.argv[0].split('/')[-1]
    pyplot.savefig(filename + '_plot.png')
    pyplot.close()


if __name__ == "__main__":
    # define location of dataset
    dataRoot = r"C:\data\cat_dog_neither"
    trainingRoot = os.path.join(dataRoot, "training_set")
    testRoot = os.path.join(dataRoot, "test_set")
    resDataRoot = os.path.join(dataRoot, "processed_data")
    resTrainingRoot = os.path.join(resDataRoot, "training_set")
    resTestRoot = os.path.join(resDataRoot, "test_set")
    finalDataRoot = os.path.join(dataRoot, "finalised_data")
    # resize_images(trainingRoot, resTrainingRoot)
    # resize_images(testRoot, resTestRoot)
    productionise_model()
