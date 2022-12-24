import numpy as np
from PIL import Image

path = r".\\..\\..\\Textures\\Map\\"
inname = "raw.r32"
size = 8193
divisions = 4
outsize = int((size - 1)/divisions) + 1
resultarray = []

f = open(path + inname, "rb")
data = np.fromfile(f, '<f4')
f.close()

pixel_count = 0
texture_index = 0
while texture_index < divisions * divisions:
    resultarray = np.zeros((outsize, outsize, 3))

    imageX = texture_index % divisions
    imageY = int(texture_index / divisions)

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
    final.save(path + r"Out\\" + "height-" + str(texture_index) + ".png")

    print("Saved Image: " + str(texture_index))

    texture_index += 1

print("Done")
