# ImageMerge-Core
  Simple Image merge program Core.
  ```
  Support PNG, JPG, JPEG Files.
  ```
# How to Build
```
.Net FrameWork 4.6.2
```
### Add Nuget Package
Costura.Fody

### Add Assembly
System.Drawing
# How to Use
## InPut
```
./*.exe 0~1 ImageFolder ImageExtension(optional)
```
> 0~1 (require)
>> 0(Width) or 1(Length)

> ImageFolder (require)
>> C:\img (Folder with an image)

> ImageExtension (optional)
>> png or jpg or jpeg (select one, "Empty" = Support extension All files are merged in order)
>>> This option only merges the entered extensions.
## Output
```
"ImageFolder"\"output"\final_N.PNG
```
  Output is PNG.
  Cut image from 10,000 pixels.
# License
MIT License
Copyright (c) 2021 qn1213
