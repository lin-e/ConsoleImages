using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;

namespace ConsoleImages
{
    public class ConsoleImage
    {
        private Dictionary<ConsoleColor, List<int[]>> colourDictionary = new Dictionary<ConsoleColor, List<int[]>>();
        private Image _rawImage;
        private Image _resizedImage;
        private ImageType imageType;
        private ConsoleAnimation animationSequence;
        private int _loopCount;
        public int[] OutputDimensions = { Console.WindowWidth, Console.WindowHeight };
        /// <summary>
        /// Creates a new instance of the ConsoleImage type
        /// </summary>
        /// <param name="imagePath">Path to image, on disk or URL</param>
        public ConsoleImage(string imagePath)
        {
            if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
            {
                RawImage = Image.FromStream(new MemoryStream(new WebClient().DownloadData(imagePath)));
            }
            else
            {
                RawImage = Image.FromFile(imagePath);
            }
        }
        /// <summary>
        /// Creates a new instance of the ConsoleImage type
        /// </summary>
        /// <param name="newImage">Image to display on the console</param>
        public ConsoleImage(Image newImage)
        {
            RawImage = newImage;
        }
        /// <summary>
        /// Delay between each frame of an animation
        /// </summary>
        public int FrameDelay
        {
            get
            {
                if (imageType == ImageType.Animated)
                {
                    return animationSequence.FrameDelay;
                }
                return 0;
            }
            set
            {
                if (imageType == ImageType.Animated)
                {
                    animationSequence.FrameDelay = value;
                }
            }
        }
        /// <summary>
        /// Determines how many times an animation will loop; if it is set to 0 or not changed, it will loop forever
        /// </summary>
        public int LoopCount
        {
            get
            {
                if (imageType == ImageType.Animated)
                {
                    return _loopCount;
                }
                return 0;
            }
            set
            {
                if (imageType == ImageType.Animated)
                {
                    _loopCount = value;
                }
            }
        }
        /// <summary>
        /// Raw, unchanged image input
        /// </summary>
        public Image RawImage
        {
            get
            {
                return _rawImage;
            }
            set
            {
                _rawImage = value;
                int frameCount = 0;
                try
                {
                    frameCount = _rawImage.GetFrameCount(FrameDimension.Time);
                }
                catch { }
                if (frameCount > 1)
                {
                    imageType = ImageType.Animated;
                    animationSequence = new ConsoleAnimation(_rawImage);
                }
                else
                {
                    imageType = ImageType.Still;
                    _resizedImage = new Bitmap(_rawImage, new Size(OutputDimensions[0], OutputDimensions[1]));
                    setColourArray();
                }
            }
        }
        private enum ImageType
        {
            Still, Animated
        }
        /// <summary>
        /// Image that has been resized to the set dimensions
        /// </summary>
        public Image ResizedImage
        {
            get
            {
                return _resizedImage;
            }
        }
        /// <summary>
        /// Sets the dimensions of the new image
        /// </summary>
        /// <param name="imageX">Width of frame</param>
        /// <param name="imageY">Height of frame</param>
        public void SetDimensions(int imageX, int imageY)
        {
            OutputDimensions = new int[] { imageX, imageY };
        }
        /// <summary>
        /// Sets the dimensions of the new image
        /// </summary>
        /// <param name="bothDimensions">Integer array with the width and height (in that order)</param>
        public void SetDimensions(int[] bothDimensions)
        {
            OutputDimensions = bothDimensions;
        }
        /// <summary>
        /// Sets the dimensions of the new image
        /// </summary>
        /// <param name="frameSize">Size of the frame</param>
        public void SetDimensions(Size frameSize)
        {
            OutputDimensions = new int[] { frameSize.Width, frameSize.Width };
        }
        /// <summary>
        /// Begins to draw or animate, depending on the image type
        /// </summary>
        public void Draw()
        {
            if (imageType == ImageType.Animated)
            {
                if (LoopCount == 0)
                {
                    while (true)
                    {
                        animationSequence.Play();
                    }
                }
                else
                {
                    for (int x = 0; x < LoopCount; x++)
                    {
                        animationSequence.Play();
                    }
                }
            }
            else
            {
                drawImage();
            }
        }
        private void drawImage()
        {
            if (_resizedImage == null)
            {
                throw new InvalidOperationException("Image cannot be empty.");
            }
            foreach (ConsoleColor singleColour in colourDictionary.Keys)
            {
                Console.BackgroundColor = Console.ForegroundColor = singleColour;
                foreach (int[] pixelPosition in colourDictionary[singleColour])
                {
                    if (!((pixelPosition[0] == Console.WindowWidth - 1) && (pixelPosition[1] == Console.WindowHeight - 1)))
                    {
                        Console.SetCursorPosition(pixelPosition[0], pixelPosition[1]);
                        Console.Write("@");
                    }
                }
            }
        }
        private void setColourArray()
        {
            // doesnt matter what order; it builds using co-ordinates in a list
            List<ConsoleColor> tempColourList = new List<ConsoleColor>();
            for (int pixelX = 0; pixelX < _resizedImage.Width; pixelX++)
            //for (int pixelY = 0; pixelY < _resizedImage.Height; pixelY++)
            {
                for (int pixelY = 0; pixelY < _resizedImage.Height; pixelY++)
                //for (int pixelX = 0; pixelX < _resizedImage.Width; pixelX++)
                {
                    ConsoleColor newColour = getConsoleColour(((Bitmap)_resizedImage).GetPixel(pixelX, pixelY));
                    if (!(colourDictionary.ContainsKey(newColour)))
                    {
                        colourDictionary.Add(newColour, new List<int[]>());
                    }
                    colourDictionary[newColour].Add(new int[] { pixelX, pixelY });
                }
            }
        }
        private ConsoleColor getConsoleColour(Color baseColour)
        {
            //From Glenn Slaydenn @ http://stackoverflow.com/a/12340136
            byte byteRed = baseColour.R;
            byte byteGreen = baseColour.G;
            byte byteBlue = baseColour.B;
            ConsoleColor toReturn = 0;
            double newRed = byteRed;
            double newGreen = byteGreen;
            double newBlue = byteBlue;
            double newDelta = double.MaxValue;
            foreach (ConsoleColor consoleColour in Enum.GetValues(typeof(ConsoleColor)))
            {
                string colourName = Enum.GetName(typeof(ConsoleColor), consoleColour);
                Color fromName = Color.FromName(colourName == "DarkYellow" ? "Orange" : colourName);
                double newValue = Math.Pow(fromName.R - newRed, 2.0) + Math.Pow(fromName.G - newGreen, 2.0) + Math.Pow(fromName.B - newBlue, 2.0);
                if (newValue == 0.0)
                {
                    return consoleColour;
                }
                if (newValue < newDelta)
                {
                    newDelta = newValue;
                    toReturn = consoleColour;
                }
            }
            return toReturn;
        }
        private class ConsoleAnimation
        {
            private List<ConsoleImage> allFrames;
            private Image _inputImage;
            public int FrameDelay;
            public ConsoleAnimation(string imagePath)
            {
                allFrames = new List<ConsoleImage>();
                if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
                {
                    inputImage = Image.FromStream(new MemoryStream(new WebClient().DownloadData(imagePath)));
                }
                else
                {
                    inputImage = Image.FromFile(imagePath);
                }
            }
            public ConsoleAnimation(Image newImage)
            {
                allFrames = new List<ConsoleImage>();
                inputImage = newImage;
            }
            public Image inputImage
            {
                get
                {
                    return _inputImage;
                }
                set
                {
                    _inputImage = value;
                    Image[] frameArray = splitFrames(_inputImage);
                    foreach (Image singleFrame in frameArray)
                    {
                        allFrames.Add(new ConsoleImage(singleFrame));
                    }
                }
            }
            public void Play()
            {
                foreach (ConsoleImage singleFrame in allFrames)
                {
                    Thread.Sleep(FrameDelay);
                    singleFrame.drawImage();
                }
            }
            Image[] splitFrames(Image animatedInputImage)
            {
                //From Neoheurist @ http://stackoverflow.com/a/26178389
                int animationLength = animatedInputImage.GetFrameCount(FrameDimension.Time);
                Image[] allFrames = new Image[animationLength];
                for (int x = 0; x < animationLength; x++)
                {
                    animatedInputImage.SelectActiveFrame(FrameDimension.Time, x);
                    allFrames[x] = new Bitmap(animatedInputImage.Size.Width, animatedInputImage.Size.Height);
                    Graphics.FromImage(allFrames[x]).DrawImage(animatedInputImage, new Point(0, 0));
                }
                return allFrames;
            }
        }
    }
    public static class Extensions
    {
        public static void ConsoleDraw(this Image baseImage)
        {
            new ConsoleImage(baseImage).Draw();
        }
    }
}
