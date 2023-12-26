import random
import math
import matplotlib.pyplot as plt
from cProfile import Profile
from pstats import SortKey, Stats
import functools

def generate_data(n):
    angle = random.random() * 2 * math.pi
    xvals = []
    yvals = []
    x, y = 0, 0
    while len(xvals) < n:
        flag = False
        if random.random() < abs(math.cos(angle)):
            flag = True
            x += math.copysign(1, math.cos(angle))
        if random.random() < abs(math.sin(angle)):
            flag = True
            y += math.copysign(1, math.sin(angle))
        if not flag:
            continue
        angle += (random.random() - 0.5) * 2 * math.pi * 0.05
        xvals.append(x)
        yvals.append(y)
    return (xvals, yvals)

def graph(data, splines):
    fig = plt.figure()
    ax = fig.add_subplot()
    ax.plot(data[0], data[1])
    for spline in splines:
        ax.plot(spline[0], spline[1])
    ax.set_aspect('equal')
    plt.show()

def local_interpolate(data, factor=10, dist=[1,2,3,4,3,2,1]):
    center = len(dist) // 2
    def dat(i):
        if i < 0:
            return (data[0][0], data[1][0])
        if i >= len(data[0]):
            return (data[0][-1], data[1][-1])
        return (data[0][i], data[1][i])
    def evalInt(i):
        xtotal = 0
        ytotal = 0
        weighttotal = 0
        for j in range(len(dist)):
            xtotal += dist[j] * dat(i - center + j)[0]
            ytotal += dist[j] * dat(i - center + j)[1]
            weighttotal += dist[j]
        return (xtotal/weighttotal, ytotal/weighttotal)
    xvals = []
    yvals = []
    for j in range((len(data[0]) + center * 2) * factor):
        i = j / factor
        mod = i - int(i)
        i = int(i)
        i -= center
        mod0val = evalInt(i)
        mod1val = evalInt(i + 1)
        val = (mod0val[0] * (1 - mod) + mod1val[0] * mod,
               mod0val[1] * (1 - mod) + mod1val[1] * mod)
        xvals.append(val[0])
        yvals.append(val[1])
    return (xvals, yvals)
if __name__ == "__main__":
    random.seed(0)
    data = generate_data(100)
    graph(data, [local_interpolate(data, 10)])