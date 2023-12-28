import matplotlib.pyplot as plt
import numpy as np
import math

def graph(data, ylabel):
    plt.plot(data)
    plt.ylabel(ylabel)
    plt.xlabel('Time (hr)')
    plt.show()

def generate_temperatures(n, stride=1):
    temps = []
    prev_temp = 20
    for i in range(n):
        temp = generate_temperature(i, prev_temp)
        if i % stride == 0:
            temps.append(temp)
        prev_temp = temp
    return np.array(temps)

def generate_temperature(n, prev):
    tod = n % 24 # time of day, hour from 0 - 24
    tod *= 2 * math.pi / 24 # tod now from from 0 - 2pi

    prev_tod = (n - 1) % 24 # time of day, hour from 0 - 24
    prev_tod *= 2 * math.pi / 24 # tod now from from 0 - 2pi
    
    tod_adjust = (-math.cos(prev_tod) + math.cos(tod)) * 5

    tod_adjust = 0 # no tod for now
    
    target = 20 + tod_adjust
    
    delta = target - prev
    delta_adjust = delta * 0.0005 * 0.1
    temp = prev + np.random.normal(0, 1) * 0.35 * 0.1 + delta_adjust + tod_adjust

    return temp

if __name__ == "__main__":
    graph(generate_temperatures(100000, 1), 'Temperature')