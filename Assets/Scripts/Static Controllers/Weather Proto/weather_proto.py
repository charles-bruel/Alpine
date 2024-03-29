#>============================================================================<
#
#    Alpine, Ski Resort Tycoon Game
#    Copyright (C) 2024  Charles Bruel
#
#    This program is free software: you can redistribute it and/or modify
#    it under the terms of the GNU General Public License as published by
#    the Free Software Foundation, either version 3 of the License, or
#    (at your option) any later version.
#
#    This program is distributed in the hope that it will be useful,
#    but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#    GNU General Public License for more details.
#
#    You should have received a copy of the GNU General Public License
#    along with this program.  If not, see <https://www.gnu.org/licenses/>.
#>============================================================================<

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