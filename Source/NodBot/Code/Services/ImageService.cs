using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System.Threading;
using NodBot.Code.Enums;
using NodBot.Code.Model;
using System.Collections;
using System.Text.RegularExpressions;

namespace NodBot.Code
{
    public class ImageService
    {
        private readonly CancellationTokenSource token;
        private readonly Logger logger;
        private readonly IntPtr gameWindow;

        public ImageService(CancellationTokenSource ct, Logger aLogger, IntPtr gameWindowHandle)
        {
            token = ct;
            logger = aLogger;
            gameWindow = gameWindowHandle;
        }

        public ImageService(IntPtr gameWindowHandle)
        {
            gameWindow = gameWindowHandle;
        }

        #region Public Fucntion API

        /// <summary>
        /// 
        /// </summary>
        public void CaptureScreen()
        {
            try
            {
                CaptureScreen(NodImages.GameWindow);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
                token.Cancel();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="screenSection"></param>
        public Image<Bgr, byte> CaptureScreen(String filePath, ScreenSection screenSection = ScreenSection.All)
        {
            Bitmap pImage = GetScreenCapture();
            Image<Bgr, byte> source = new Image<Bgr, byte>(pImage);

            source = copySubImage(screenSection.getSubRect(source), source);
            filePath = filePath.Replace(".png", "_" + screenSection.ToString() + ".png");
            SaveImageFile(source, filePath);

            return source;
        }

        /// <summary>
        /// TODO :: Needs to be updated, still functional
        /// </summary>
        public void CaputreNeutralPoint()
        {
            Bitmap pImage = GetScreenCapture();

            Image<Bgr, byte> current = new Image<Bgr, byte>(pImage);

            if (pImage.Width > 700 && pImage.Height > 500)
            {
                // Retrieve specific portion of screen capture
                Rectangle cloneRect = new Rectangle(100, 100, 600, 400);
                Image<Bgr, byte> neutral = copySubImage(cloneRect, current);
                SaveImageFile(neutral, NodImages.NeutralSS);
            }
            else SaveImageFile(current, NodImages.NeutralSS);

            pImage.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="debug"></param>
        /// <returns></returns>
        public Point? FindChestCoord(bool debug = false)
        {
            Mat lImage;
            CaptureScreen(NodImages.GameWindow, ScreenSection.Game); // Always update current screen before locating chest
            String[] chestImages = { NodImages.Chest1, NodImages.Chest2, NodImages.Chest3 };
            Point? point = null;
            foreach (String chest in chestImages)
            {
                try
                {
                    logger.sendMessage("Scanning: " + chest, LogType.DEBUG);
                    point = Draw(chest, NodImages.GameWindow_Game, out lImage);
                    if (debug) CvInvoke.Imshow(chest, lImage);
                    if (point != null) break;
                }
                catch (Exception ex)
                {
                    logger.sendMessage(ex.ToString(), LogType.WARNING);
                }
            }

            if (point != null)
            {
                Point result = point.Value;
                result.X += ScreenSection.Game.getXOffset();
                result.Y += ScreenSection.Game.getYOffset();
                return result;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <returns></returns>
        public List<UIPoint> FindTemplateMatches(String templateImage, ScreenSection screenSection = ScreenSection.All, double threshold = .95, TemplateMatchingType matchType = TemplateMatchingType.CcoeffNormed)
        {
            List<Rectangle> result = FindTemplateMatchesImpl(templateImage: templateImage, screenSection: screenSection, threshold: threshold, matchType: matchType);

            if (result == null)
            {
                return new List<UIPoint>();
            }

            return result.Select(rect => new UIPoint(new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2), rect)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="screenSection"></param>
        /// <param name="threshold"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        public UIPoint FindTemplateMatch(String templateImage, ScreenSection screenSection, double threshold = .95, TemplateMatchingType matchType = TemplateMatchingType.CcoeffNormed)
        {
            var result = FindTemplateMatchImpl(templateImage, screenSection: screenSection, threshold: threshold, matchType: matchType);

            if (result != null)
            {
                return new UIPoint(new Point(result.Value.X + result.Value.Width / 2, result.Value.Y + result.Value.Height / 2), result.Value);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="screenSection"></param>
        /// <param name="threshold"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        public List<UIPoint> FindTemplateMatch(List<String> templateImages, ScreenSection screenSection, double threshold = .95, TemplateMatchingType matchType = TemplateMatchingType.CcoeffNormed)
        {
            var result = FindTemplateMatchImpl(templateImages, screenSection: screenSection, threshold: threshold, matchType: matchType);

            return result.Select(item =>
            {
                if (item == null) return null;
                return new UIPoint(new Point(item.Value.X + item.Value.Width / 2, item.Value.Y + item.Value.Height / 2), item.Value);
            }).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="screenSection"></param>
        /// <param name="threshold"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        public bool ContainsTemplateMatch(String templateImage, ScreenSection screenSection, double threshold = .75, TemplateMatchingType matchType = TemplateMatchingType.CcoeffNormed)
        {
            var result = FindTemplateMatchImpl(templateImage, screenSection: ScreenSection.Game, threshold: threshold, matchType: matchType);
            return result != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="whitelist"></param>
        /// <param name="blacklist"></param>
        /// <param name="resultWhite"></param>
        /// <param name="resultBlack"></param>
        public void ScanForItems(ScreenSection section, List<Item> whitelist, List<Item> blacklist, out List<Item> resultWhite, out List<Item> resultBlack)
        {
            String baseFile = NodImages.GameWindow.Replace(".png", "_" + section.ToString() + ".png");

            resultWhite = new List<Item>();
            resultBlack = new List<Item>();

            Image<Bgr, byte> source = CaptureScreen(baseFile, section);

            using (Image<Bgr, byte> imgSrc = source.Copy())
            {
                for (int i = 0; i < whitelist.Count; i++)
                {
                    Item currentItem = whitelist.ElementAt(i);
                    Image<Bgr, byte> template = OpenImageFile(currentItem.imageFile); // Image A

                    Point[] minLocations, maxLocations;
                    bool result = ContainsTemplate(imgSrc, template, out maxLocations, out minLocations, threshold: currentItem.threshold);

                    if (result)
                    {
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);

                        imgSrc.Draw(match, new Bgr(Color.Red), -1);
                        resultWhite.Add(currentItem);
                    }

                    SaveImageFile(imgSrc, baseFile);
                }

                for (int i = 0; i < blacklist.Count; i++)
                {
                    Item currentItem = blacklist.ElementAt(i);
                    Image<Bgr, byte> template = OpenImageFile(currentItem.imageFile); // Image A

                    //updated and changed TemplateMatchingType- CcoeffNormed.

                    Point[] minLocations, maxLocations;
                    bool result = ContainsTemplate(imgSrc, template, out maxLocations, out minLocations, threshold:currentItem.threshold);

                    if (result)
                    {
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        imgSrc.Draw(match, new Bgr(Color.Pink), -1);
                        resultBlack.Add(currentItem);
                    }

                    SaveImageFile(imgSrc, baseFile);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="baseImage"></param>
        /// <returns></returns>
        public bool IsRectEmpty(Rectangle rect, String baseImage, ScreenSection screenSection)
        {
            try
            {
                Image<Bgr, byte> source = CaptureScreen(baseImage);
                Image<Bgr, byte> filteredSource = copySubImage(rect, source);

                SaveImageFile(filteredSource, NodImages.Temp_Inventory_1);

                var avgColors = filteredSource.GetAverage();
                if (avgColors.Red < 38 && avgColors.Blue < 38 && avgColors.Green < 38)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool ContainsMatch(String templateImage, ScreenSection screenSection)
        {
            CaptureScreen(NodImages.GameWindow, screenSection: screenSection); //Take Screenshot
            String baseFile = NodImages.GameWindow.Replace(".png", "_" + screenSection.ToString() + ".png");
            return Draw(templateImage, baseFile, out Mat image, false) != null;
        }

        #endregion

        #region Private helper functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private Image<Bgr, byte> OpenImageFile(String filename)
        {
            lock (filename)
            {
                return new Image<Bgr, byte>(filename);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filename"></param>
        private void SaveImageFile(Image<Bgr, byte> image, String filename)
        {
            lock (filename)
            {
                image.Save(filename);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="screenSection"></param>
        /// <param name="updateCurrentScreen"></param>
        /// <param name="threshold"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        private List<Rectangle> FindTemplateMatchesImpl(String templateImage, ScreenSection screenSection = ScreenSection.All, bool updateCurrentScreen = false, double threshold = .95, TemplateMatchingType matchType = TemplateMatchingType.CcoeffNormed)
        {
            String baseFile = NodImages.GameWindow.Replace(".png", "_" + screenSection.ToString() + ".png");

            Image<Bgr, byte> source = CaptureScreen(baseFile);
            Image<Bgr, byte> template = OpenImageFile(templateImage); // Image A

            source = copySubImage(screenSection.getSubRect(source), source);
            SaveImageFile(source, baseFile);

            var matches = new List<Rectangle>();
            using (Image<Bgr, byte> imgSrc = source.Copy())
            {
                while (true)
                {
                    Point[] minLocations, maxLocations;
                    bool result = ContainsTemplate(imgSrc, template, out maxLocations, out minLocations, threshold: threshold, matchingType: matchType);
                    if (result)
                    {
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);

                        imgSrc.Draw(match, new Bgr(Color.Red), -1);

                        match.X += screenSection.getXOffset();
                        match.Y += screenSection.getYOffset();

                        matches.Add(match);
                    }
                    else break;

                    SaveImageFile(imgSrc, baseFile);
                }
            }

            return matches;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="screenSection"></param>
        /// <param name="updateCurrentScreen"></param>
        /// <param name="threshold"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        private Rectangle? FindTemplateMatchImpl(String templateImage, ScreenSection screenSection, double threshold = .95, TemplateMatchingType matchType = TemplateMatchingType.CcoeffNormed)
        {
            String baseFile = NodImages.GameWindow.Replace(".png", "_" + screenSection.ToString() + ".png");

            Image<Bgr, byte> source = CaptureScreen(baseFile, screenSection);
            Image<Bgr, byte> template = OpenImageFile(templateImage); // Image A

            using (Image<Bgr, byte> imgSrc = source.Copy())
            {
                while (true)
                {

                    Point[] minLocations, maxLocations;
                    bool result = ContainsTemplate(imgSrc, template, out maxLocations, out minLocations, threshold: threshold, matchingType: matchType);
                    if (result)
                    {
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        imgSrc.Draw(match, new Bgr(Color.Red), -1);
                        SaveImageFile(imgSrc, baseFile);

                        match.X += screenSection.getXOffset();
                        match.Y += screenSection.getYOffset();

                        return match;
                    }
                    else break;
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateImage"></param>
        /// <param name="screenSection"></param>
        /// <param name="updateCurrentScreen"></param>
        /// <param name="threshold"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        private List<Rectangle?> FindTemplateMatchImpl(List<String> templateImage, ScreenSection screenSection, double threshold = .95, TemplateMatchingType matchType = TemplateMatchingType.CcoeffNormed)
        {
            String baseFile = NodImages.GameWindow.Replace(".png", "_" + screenSection.ToString() + ".png");

            Image<Bgr, byte> source = CaptureScreen(baseFile, screenSection);
            Image<Bgr, byte> template;

            List<Rectangle?> matchResult = new List<Rectangle?>();

            using (Image<Bgr, byte> imgSrc = source.Copy())
            {
                templateImage.ForEach(str =>
                {
                    template = OpenImageFile(str); // Image A

                    Point[] minLocations, maxLocations;
                    bool result = ContainsTemplate(imgSrc, template, out maxLocations, out minLocations, threshold: threshold, matchingType: matchType);
                    if (result)
                    {
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        imgSrc.Draw(match, new Bgr(Color.Red), -1);
                        SaveImageFile(imgSrc, baseFile);

                        match.X += screenSection.getXOffset();
                        match.Y += screenSection.getYOffset();
                        matchResult.Add(match);

                    }
                    else matchResult.Add(null);
                });
            }

            return matchResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgSrc"></param>
        /// <param name="template"></param>
        /// <param name="maxLocations"></param>
        /// <param name="minLocations"></param>
        /// <param name="threshold"></param>
        /// <param name="matchingType"></param>
        /// <returns></returns>
        private bool ContainsTemplate(Image<Bgr, byte> imgSrc, Image<Bgr, byte> template, out Point[] maxLocations, out Point[] minLocations, double threshold = 0.9, TemplateMatchingType matchingType = TemplateMatchingType.CcoeffNormed)
        {
            using (Image<Gray, float> result = imgSrc.MatchTemplate(template, matchingType))
            {
                SaveImageFile(imgSrc, "Images//Poft//__1.png");
                SaveImageFile(template, "Images//Poft//__2.png");

                var _threshold = CvInvoke.Threshold(result, result, threshold, 1, ThresholdType.ToZero);
                double[] minValues, maxValues;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                if (maxValues[0] > _threshold)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private Image<Bgr, byte> copySubImage(Rectangle rect, Image<Bgr, byte> image)
        {
            Rectangle roi = image.ROI;
            image.ROI = rect;
            Image<Bgr, byte> newImage = image.Copy();
            image.ROI = roi;

            return newImage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelImage"></param>
        /// <param name="observedImage"></param>
        /// <param name="matchTime"></param>
        /// <param name="modelKeyPoints"></param>
        /// <param name="observedKeyPoints"></param>
        /// <param name="matches"></param>
        /// <param name="mask"></param>
        /// <param name="homography"></param>
        private void FindMatchSURF(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.4;
            double hessianThresh = 500;

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();
            {
                using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
                using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
                {
                    SURF surfCPU = new SURF(hessianThresh);
                    //extract features from the object image
                    UMat modelDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    UMat observedDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                    BFMatcher matcher = new BFMatcher(DistanceType.L2);
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 3)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                           matches, mask, 1.5, 20);
                        if (nonZeroCount >= 3)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                               observedKeyPoints, matches, mask, 2);
                    }

                    watch.Stop();
                }
            }
            matchTime = watch.ElapsedMilliseconds;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelImage"></param>
        /// <param name="observedImage"></param>
        /// <param name="matchTime"></param>
        /// <param name="modelKeyPoints"></param>
        /// <param name="observedKeyPoints"></param>
        /// <param name="matches"></param>
        /// <param name="mask"></param>
        /// <param name="homography"></param>
        private void FindMatchSIFT(Mat modelImage, Mat observedImage, out long matchTime, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.4;

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();
            {
                using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
                using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
                {
                    SIFT surfCPU = new SIFT();
                    //extract features from the object image
                    UMat modelDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                    watch = Stopwatch.StartNew();

                    // extract features from the observed image
                    UMat observedDescriptors = new UMat();
                    surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                    BFMatcher matcher = new BFMatcher(DistanceType.L2);
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k, null);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 3)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                           matches, mask, 1.5, 20);
                        if (nonZeroCount >= 3)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                               observedKeyPoints, matches, mask, 2);
                    }

                    watch.Stop();
                }
            }
            matchTime = watch.ElapsedMilliseconds;
        }



        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        private Point? Draw(String modelImagePath, String observedImagePath, out Mat result, bool random = true)
        {
            Point? resultPoint = null;

            Mat modelImage = CvInvoke.Imread(modelImagePath, Emgu.CV.CvEnum.ImreadModes.Color);
            Mat observedImage = CvInvoke.Imread(observedImagePath, Emgu.CV.CvEnum.ImreadModes.Color);

            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            long matchTime; // DEBUG time to find match
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FindMatchSIFT(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography);

                //Draw the matched keypoints
                result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                   matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);

                #region draw the projected region on the image

                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
                    {
                          new PointF(rect.Left, rect.Bottom),
                          new PointF(rect.Right, rect.Bottom),
                          new PointF(rect.Right, rect.Top),
                          new PointF(rect.Left, rect.Top)
                    };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);

                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    using (VectorOfPoint vp = new VectorOfPoint(points))
                    {
                        CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);

                        // return random point to click
                        int rand = points.Length - 1; // retrieve last point
                        if (random) rand = new Random().Next(0, points.Length - 1); // random point
                        resultPoint = points[rand];
                    }

                }
                #endregion

                return resultPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateScreen"></param>
        /// <returns></returns>
        private Bitmap GetScreenCapture(bool updateScreen = true)
        {
            IntPtr hWnd = gameWindow;
            try
            {
                bool res = UpdateWindow(hWnd);

                Rectangle rctForm = new Rectangle();
                GetWindowRect(hWnd, ref rctForm);
                IntPtr val = GetWindowDC(hWnd);


                using (Graphics grfx = Graphics.FromHdc(val))
                {
                    rctForm = Rectangle.Round(grfx.VisibleClipBounds);
                }

                Bitmap pImage = new Bitmap((int)(rctForm.Width), rctForm.Height);

                Graphics graphics = Graphics.FromImage(pImage);
                IntPtr hDC = graphics.GetHdc();
                //paint control onto graphics using provided options        
                try
                {
                    PrintWindow(hWnd, hDC, (uint)0);
                }
                finally
                {
                    graphics.ReleaseHdc(hDC);
                    graphics.Dispose();
                    ReleaseDC(hWnd, val);
                }

                return pImage;
            }
            catch (Exception ex)
            {
                // There was a problem, stopping bot
                logger.sendLog(ex.Message, LogType.ERROR);
                logger.sendLog(ex.StackTrace, LogType.ERROR);
                Console.Out.WriteLine(ex.StackTrace);
                if (token != null) token.Cancel();
            }

            return new Bitmap(0, 0);
        }

        #endregion

        #region User32.dll Functions
        /***
         * 
         * 
         **/

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetLastError();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateWindow(IntPtr hWnd);

        #endregion
    }
}
