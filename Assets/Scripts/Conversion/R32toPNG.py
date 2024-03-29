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

import numpy as np
from PIL import Image

path = r".\\..\\..\\Textures\\Map\\Poseidon Mons\\"
inname = "Temp\\height-raw.r32"
size = 4097
divisions = 8
outsize = int((size - 1)/divisions) + 1
resultarray = []

f = open(path + inname, "rb")
data = np.fromfile(f, '<f4')
f.close()

pixel_count = 0
texture_index = 0
while texture_index < divisions * divisions:
    resultarray = np.zeros((outsize, outsize, 3))

    imageX = (texture_index % divisions)
    imageY = divisions - 1 - int(texture_index / divisions)

    lookup_index = 0
    while lookup_index < outsize * outsize:
        x = int(lookup_index % outsize)
        y = int(lookup_index / outsize)

        absX = x + imageX * (outsize - 1)
        absY = y + imageY * (outsize - 1)

        raw_data_index = int(absY * size + absX)

        dataval = int((1-data[raw_data_index]) * 0x00FFFFFE)
        r = (0x000000FF & dataval) >> 0
        g = (0x0000FF00 & dataval) >> 8
        b = (0x00FF0000 & dataval) >> 16
        if(r == 0 and g == 0 and b == 0):
            r = 255
            g = 255
            b = 255
        resultarray[x, y] = (r, g, b)
        lookup_index += 1

        pixel_count += 1

        if pixel_count % 262144 == 0:
            percent = (pixel_count / (outsize * outsize * divisions * divisions)) * 100
            print("%.2f%%" % percent)
    print("Saving Image: " + str(texture_index))

    final = Image.fromarray((resultarray * 255).astype(np.uint8))
    final.save(path + r"Height\\" + "height-" + str(texture_index) + ".png")

    print("Saved Image: " + str(texture_index))

    texture_index += 1

print("Done")
