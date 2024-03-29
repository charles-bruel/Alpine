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
