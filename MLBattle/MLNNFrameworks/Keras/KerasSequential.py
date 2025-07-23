import keras as kr
import numpy as np

data_samples_x = []
data_samples_y = []
for i in range(100):
    data_samples_x.append([i, i+1, i+2, i+3, i+4, i+5, i+6, i+7, i+8, i+9])
    data_samples_y.append([i*0.01, 1.0-i*0.01])

model = kr.Sequential()
model.add(kr.layers.Dense(10, activation="sigmoid", input_shape=(10,)))
model.add(kr.layers.Dense(6, activation="sigmoid"))
model.add(kr.layers.Dense(2, activation="sigmoid"))

#Compilieren: Als Optimierer wird „Stochstic Gradient Descent“, eine #Verbesserung des „Gradient Descent“ eingesetzt.
#Als „Metrik“ (metrics) können zusätzliche oder die gleiche Loss-Funktion
#angegeben werden, die zusätzlich berechnet, aber meist nicht weiter genutzt
#wird.
model.compile(optimizer="sgd", loss="mean_squared_error", metrics=[kr.losses.mean_squared_error])

#Training: sind die Daten als Liste gegeben sollten sie zunächst in ein
#NumPy-array umgewandelt werden (np.array). Trainiert wird 100 Runden/Epochen
#Mit der batch_size kann angegeben werden wieviele Datenpaare gleichzeitig
#genutzt werden sollen, in diesem Fall nur eines.
model.fit(np.array(data_samples_x), np.array(data_samples_y), epochs=100, batch_size=1)

#Zusätzlich: Evaluation mit Trainingsdaten (sollten Testdaten sein)
print(model.evaluate(np.array(data_samples_x), np.array(data_samples_y), batch_size=1))

#Zusätzlich: Vorhersage auf Trainingsdaten (sollten Testdaten sein)
print(model.predict(np.array(data_samples_x), batch_size=1))
