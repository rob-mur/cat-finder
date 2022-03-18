import os
import sys

import tensorflow as tf
from keras.applications.vgg16 import VGG16
from keras.layers import Dense, Flatten, Dropout
from keras.models import Model
from keras.optimizer_v1 import SGD
from keras.preprocessing.image import img_to_array
from keras.preprocessing.image import load_img, ImageDataGenerator, save_img
from matplotlib import pyplot
from matplotlib.image import imread
from tqdm import tqdm
from keras import backend as K
from keras.callbacks import EarlyStopping, ModelCheckpoint

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
        photo = load_img(os.path.join(src_root, file_type, file), target_size=(224, 224))
        photo = img_to_array(photo)
        save_img(os.path.join(dest_root, file_type, file), photo)


def define_model():
    model = VGG16(include_top=False, input_shape=(224, 224, 3))
    for layer in model.layers:
        layer.trainable = False
    flat1 = Flatten()(model.layers[-1].output)
    output = Dense(3, activation='sigmoid')(flat1)
    model = Model(inputs=model.inputs, outputs=output)
    # compile model
    opt = SGD(lr=0.001, momentum=0.9)
    model.compile(optimizer=opt, loss='categorical_crossentropy', metrics=['accuracy'])
    return model


def train_and_evaluate():
    # define model
    model = define_model()
    # create data generator
    train_datagen = ImageDataGenerator(rescale=1.0 / 255.0, width_shift_range=0.1, height_shift_range=0.1,
                                       horizontal_flip=True)
    test_datagen = ImageDataGenerator(rescale=1.0 / 255.0)
    train_datagen.mean = [123.68, 116.779, 103.939]
    test_datagen.mean = [123.68, 116.779, 103.939]
    # prepare iterators
    train_it = train_datagen.flow_from_directory(resTrainingRoot,
                                                 batch_size=64, target_size=(224, 224))
    test_it = test_datagen.flow_from_directory(resTestRoot, batch_size=64, target_size=(224, 224))
    # fit model
    history = model.fit(train_it, steps_per_epoch=len(train_it),
                        validation_data=test_it, validation_steps=len(test_it), epochs=20, verbose=1)
    _, acc = model.evaluate(test_it, steps=len(test_it), verbose=0)
    print('> %.3f' % (acc * 100.0))
    summarize_diagnostics(history)


def productionise_model():
    es = EarlyStopping(monitor='val_loss', mode='min', verbose=1, patience=5)
    mc = ModelCheckpoint('best_model.h5', monitor='val_loss', mode='min', verbose=1)
    cb_list = [es, mc]
    # define model
    model = define_model()
    # create data generator
    train_datagen = ImageDataGenerator(rescale=1.0 / 255.0, width_shift_range=0.1, height_shift_range=0.1,
                                       horizontal_flip=True)
    test_datagen = ImageDataGenerator(rescale=1.0 / 255.0)
    train_datagen.mean = [123.68, 116.779, 103.939]
    test_datagen.mean = [123.68, 116.779, 103.939]
    # prepare iterators
    train_it = train_datagen.flow_from_directory(resTrainingRoot,
                                                 batch_size=64, target_size=(224, 224))
    test_it = test_datagen.flow_from_directory(resTestRoot, batch_size=64, target_size=(224, 224))
    # fit model
    model.fit(train_it, steps_per_epoch=len(train_it),
              validation_data=test_it, validation_steps=len(test_it), epochs=100, callbacks=cb_list, verbose=1)


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
    # train_and_evaluate()
    productionise_model()
