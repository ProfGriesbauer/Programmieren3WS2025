import tensorflow.compat.v1 as tf
import numpy as np
tf.disable_v2_behavior()

#Array von 100 zufälligen Zahlen in [0, 1]
#TensorFlow nutzt numpy, so dass diese Arrays direkt mit TensorFlow
#verwendet werden können
x_data = np.random.rand(100)
y_data = x_data * 1.2345 + 0.6789

W = tf.Variable(tf.random_uniform([1], -1.0, 1.0))
b = tf.Variable(tf.zeros([1]))
y = tf.add(tf.multiply(W, x_data), b)

#Summe/Durchschnitt des quadratischen Fehlers
loss_func = tf.reduce_mean(tf.square(tf.subtract(y, y_data)))

#Genutzte Methode: Gradientenabstieg
my_optimizer = tf.train.GradientDescentOptimizer(0.5)

train = my_optimizer.minimize(loss_func)

with tf.Session() as my_session:
	my_session.run(tf.global_variables_initializer())
	for step in range(200):
		#Einen Gradientenabstieg ausführen
		my_session.run(train)
		#Momentanen Wert für W und b ausgeben
		print(my_session.run(W), my_session.run(b))