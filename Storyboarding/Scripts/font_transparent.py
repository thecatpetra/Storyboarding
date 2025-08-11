from PIL import Image
import os
import sys

# sys.argv[1] --- path to read and save font

for image_name in os.listdir(sys.argv[1]):

    image = Image.open(sys.argv[1] + "/" + image_name)
    new_image = Image.new(mode="RGBA", size=image.size)
    print(image_name)

    image_pixels = image.load()
    new_image_pixels = new_image.load()

    d = 130

    for i in range(image.size[0]):
        for j in range(image.size[1]):
            r = image_pixels[i, j]
            new_image_pixels[i, j] = (255, 255, 255, r)

    new_image.save(sys.argv[1] + "/" + image_name)