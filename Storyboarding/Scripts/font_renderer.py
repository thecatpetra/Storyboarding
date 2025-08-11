import fontforge
import sys

# sys.argv[1] --- path to font .ttf
# sys.argv[2] --- path to save rendered images
# sys.argv[3] --- image size

F = fontforge.open(sys.argv[1])
F.selection.select(("ranges", None), "A", "Z")
for g in F.glyphs():
    print(f"Exporting: {g.glyphname}/{g.unicode}")
    filename = sys.argv[2] + "/" + str(g.unicode) + ".png"
    g.export(filename, pixelsize=int(sys.argv[3]))
