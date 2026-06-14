from PIL import Image
import numpy as np

img = Image.open('Assets/Sprites/Weapons/gun.png')
img = img.convert('RGBA')
data = np.array(img)
alpha = data[:,:,3]
non_transparent = np.count_nonzero(alpha > 0)
print(f"Non-transparent pixels: {non_transparent}")
