"""Build the chosen icon (10 — Double anneau) as a multi-size .ico for the exe.
Redrawn at 1024px, then downscaled per size for the sharpest result.
Sizes: 16, 32, 48, 256. Same design as 10-double-anneau.png.
"""
from PIL import Image, ImageDraw

OUT = "/home/hatch/workspace/CrosshairOverlay/src/CrosshairOverlay.ico"
S = 1024
WHITE = (255, 255, 255, 255)

# Dark rounded tile (same as the other proposals)
img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d = ImageDraw.Draw(img)
d.rounded_rectangle([0, 0, S - 1, S - 1], radius=220, fill=(32, 32, 32, 255))
C = S // 2
# Double anneau: outer ring r=330 w=30, inner ring r=200 w=30, center dot r=55
d.ellipse([C - 330, C - 330, C + 330, C + 330], outline=WHITE, width=30)
d.ellipse([C - 200, C - 200, C + 200, C + 200], outline=WHITE, width=30)
d.ellipse([C - 55, C - 55, C + 55, C + 55], fill=WHITE)

img.save(OUT, format="ICO", sizes=[(16, 16), (32, 32), (48, 48), (256, 256)])
print("saved", OUT)
ico = Image.open(OUT)
print("ico sizes:", ico.info.get("sizes"))
import os
print("ico bytes:", os.path.getsize(OUT))
