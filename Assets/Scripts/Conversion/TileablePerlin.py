# Tileable Perlin FBM Noise
# Completely stolen with only slight tweaks from:
# https://gamedev.stackexchange.com/questions/23625/how-do-you-generate-tileable-perlin-noise

import random
import math
import os
import numpy as np
from PIL import Image

perm = list(range(512))
random.shuffle(perm)
perm += perm
dirs = [(math.cos(a * 2.0 * math.pi / 512),
         math.sin(a * 2.0 * math.pi / 512))
         for a in range(512)]

def noise(x, y, per):
    def surflet(gridX, gridY):
        distX, distY = abs(x-gridX), abs(y-gridY)
        polyX = 1 - 6*distX**5 + 15*distX**4 - 10*distX**3
        polyY = 1 - 6*distY**5 + 15*distY**4 - 10*distY**3
        hashed = perm[perm[int(gridX)%per] + int(gridY)%per]
        grad = (x-gridX)*dirs[hashed][0] + (y-gridY)*dirs[hashed][1]
        return polyX * polyY * grad
    intX, intY = int(x), int(y)
    return (surflet(intX+0, intY+0) + surflet(intX+1, intY+0) +
            surflet(intX+0, intY+1) + surflet(intX+1, intY+1))

def fBm(x, y, per, octs):
    val = 0
    for o in range(octs):
        val += 0.5**o * noise(x*2**o, y*2**o, per*2**o)
    return val

size, freq, octs = 512, 1/64.0, 4
resultarray = np.zeros((size, size, 3))
for y in range(size):
    for x in range(size):
        v = (fBm(x*freq, y*freq, int(size*freq), octs) + 1) * 0.5
        resultarray[x, y] = (v, v, v)

final = Image.fromarray((resultarray * 255).astype(np.uint8))
final.save(r".\\Assets\\Textures\\noise.png")