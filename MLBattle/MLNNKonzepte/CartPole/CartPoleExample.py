import gymnasium as gym
import numpy as np
import keras as kr
import random

env = gym.make('CartPole-v1', render_mode='human')
observation, info = env.reset()

model = kr.Sequential()
model.add(kr.layers.Dense(64, input_dim=4, activation='relu'))
model.add(kr.layers.Dense(128,  activation='relu'))
model.add(kr.layers.Dense(256,  activation='relu'))
model.add(kr.layers.Dense(256,  activation='relu'))
model.add(kr.layers.Dense(2,  activation='sigmoid'))
model.compile(optimizer='adam', loss='categorical_crossentropy')

firstrun = True
while True:
    #Buidl a Set of good games (reward > minReward)
    observations = []
    actions = []
    minReward = 70
    for i in range(1000):
        observation, info = env.reset()
        action = env.action_space.sample()
        obserVationList = []
        actionList = []
        score = 0
        while True:
            #env.render()
            observation, reward, terminated, truncated, info = env.step(action)
            done = terminated or truncated
            if firstrun:
                action = env.action_space.sample()
            else:
                if random.random() < 0.8:
                    action = env.action_space.sample()
                else:
                    action = np.argmax(model.predict(observation.reshape(1, 4), verbose=0))
            obserVationList.append(observation)
            if action == 1:
                actionList.append([0, 1])
            elif action == 0:
                actionList.append([1, 0])
            score += reward
            if done:
                break
        if score > minReward:
            observations.extend(obserVationList)
            actions.extend(actionList)
    observations = np.array(observations)
    actions = np.array(actions)

    #Train a model with good games
    model.fit(observations, actions, epochs=10)
    firstrun = False

    #Show what worked
    for i in range(10):
        observation, info = env.reset()
        score=0
        while True:
            env.render()
            observation, reward, terminated, truncated, info = env.step(action)
            done = terminated or truncated
            action = np.argmax(model.predict(observation.reshape(1,4), verbose=0))
            score += reward
            print(score)
            if done:
                break