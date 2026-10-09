三合院倖存者 — Title Menu UI (option 1a)
All art drawn on a 4px grid (1 art pixel = 4 screen px at 1920-wide reference).

Unity import (each PNG):
  Texture Type: Sprite (2D and UI) · Filter Mode: Point · Compression: None · Mesh Type: Full Rect
  Open Sprite Editor and set Border (L / R / T / B) for 9-slice, then Image Type = Sliced.

File                         Size      9-slice border L R T B
btn_primary_*.png            420x106   16 16 16 22
btn_secondary_*.png          340x90    12 12 12 18
tag_hanging.png              250x92    40 40 40 12
cursor_arrow.png             20x32     (no slice)

Button states → Button component: Transition = Sprite Swap
  Highlighted = *_hover · Pressed = *_pressed (secondary pressed reuses *_pressed)
Pressed sprites already include the 4px push-down; shadow is baked in under each button.

Text (set in Unity, not baked):
  Primary label    48px · #FFE9A0 · Shadow #4A120C offset (4,-4) · letter spacing ~14%
  Secondary label  32px · #F1DCAB · Shadow #1B0E08 offset (3,-3)
  Tag label (關卡/難度) 16px #E3B548 · Tag value 20px #F1DCAB, shadow #1B0E08 (2,-2)
  Suggested font: Cubic 11 (俐方體11號, OFL) for CJK; Silkscreen for Latin values.
Cursor sits 24px left of the selected button, vertically centered.
