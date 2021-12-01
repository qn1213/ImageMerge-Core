# ImageMerge-Core
  Simple Image merge program Core.
  ```
  Support PNG, JPG, JPEG Files.
  ```
  ![1](https://user-images.githubusercontent.com/3080569/144019791-7ea4381c-d593-47fb-bc03-e164a962cdf0.png)
  ![2](https://user-images.githubusercontent.com/3080569/144020461-4623311e-9443-44a0-b7aa-baead54a6ce7.png)
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
