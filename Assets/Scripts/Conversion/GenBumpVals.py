import random
import math

numVals = 128

for i in range(0, numVals):
    # We generate (x, y, z), then normalize it, then multiplly by the magnitude
    # This is not an even distribution in a circle; it is center biased
    # This is not a problem for this use case
    x = random.uniform(-1, 1)
    y = random.uniform(-1, 1)
    z = random.uniform(-1, 1)
    m = random.uniform( 0, 1)
    l = math.sqrt(x * x + y * y + z * z)
    x /= l
    y /= l
    z /= l
    x *= m
    y *= m
    z *= m
    print("\tnew Vector3(%.3ff, %.3ff, %.3ff)," % (x, y, z))
