# ConsoleImages
Converts standard images to display on console
# Usage
Referencing
```
using ConsoleImages;
```
Drawing on the console directly from an image
```
Image imageVar; // Image variable to draw
imageVar.Draw(); // Outputs the image on the Console. Note: If you use an animated image, it will loop forever
```
Drawing on the console from a web link
```
ConsoleImage newImage = new ConsoleImage("http://example.com/foobar.jpg");
newImage.Draw();
```
Drawing on the console from a local file
```
ConsoleImage newImage = new ConsoleImage("foobar.jpg");
newImage.Draw();
```
Drawing an animation on the console with settings
```
ConsoleImage newImage = new ConsoleImage("foobar.jpg"); // You can load from a web link too
newImage.LoopCount = 10; // Loops the image 10 times, leave it at 0 if you want it to repeat
newImage.FrameDelay = 10; // 10 millisecond delay between each frame (will still take time to draw the image)
```
