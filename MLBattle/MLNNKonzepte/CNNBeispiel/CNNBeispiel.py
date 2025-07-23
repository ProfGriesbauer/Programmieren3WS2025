import numpy as np
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Conv2D, MaxPooling2D, Dense, Flatten
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.datasets import mnist

# Load MNIST dataset using TensorFlow's built-in loader
(train_images, train_labels), (test_images, test_labels) = mnist.load_data()

# Normalize the images.
train_images = (train_images / 255) - 0.5
test_images = (test_images / 255) - 0.5

# Reshape the images.
train_images = np.expand_dims(train_images, axis=3)
test_images = np.expand_dims(test_images, axis=3)
num_filters = 8
filter_size = 3
pool_size = 2

print(to_categorical(train_labels))

#### MODEL ####
model = Sequential()
model.add(Conv2D(num_filters, filter_size, input_shape=(28,28,1)))
model.add(MaxPooling2D(pool_size=pool_size))
model.add(Flatten())
model.add(Dense(10, activation='sigmoid'))

model.compile('adam', loss='categorical_crossentropy', metrics=['accuracy'])
model.fit(train_images, to_categorical(train_labels), epochs=3,
validation_data=(test_images, to_categorical(test_labels)))