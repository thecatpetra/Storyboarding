import fontforge
import sys

# sys.argv[1] --- path to font .ttf
# sys.argv[2] --- path to save rendered images
# sys.argv[3] --- image size
# sys.argv[4] --- charset

F = fontforge.open(sys.argv[1])
subset = set(sys.argv[4])
F.selection.select(*(ord(c) for c in subset))
for g in list(F.selection.byGlyphs):
    print(f"Exporting: {g.glyphname}/{g.unicode}")
    filename = sys.argv[2] + "/" + str(g.unicode) + ".png"
    g.export(filename, pixelsize=int(sys.argv[3]))
