import sys
from PIL import Image

def remove_green_bg(image_path, out_path, tolerance=150):
    img = Image.open(image_path).convert("RGBA")
    data = img.getdata()
    
    new_data = []
    for item in data:
        # Check if the pixel is predominantly green
        # Green channel > Red and Blue channels
        if item[1] > tolerance and item[0] < tolerance and item[2] < tolerance:
            new_data.append((255, 255, 255, 0)) # transparent
        else:
            new_data.append(item)
            
    img.putdata(new_data)
    img.save(out_path, "PNG")

if __name__ == "__main__":
    remove_green_bg(sys.argv[1], sys.argv[2])
