using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System.Threading;

namespace NodBot.Code
{
    public class ImageService
    {
        private CancellationTokenSource token;
        private Logger logger;
        private IntPtr gameWindow;
        //private Image<Bgr, byte> currentScreen;

        public ImageService(CancellationTokenSource ct,Logger aLogger, IntPtr gameWindowHandle)
        {
            token = ct;
            logger = aLogger;
            gameWindow = gameWindowHandle;
        }

        public ImageService(IntPtr gameWindowHandle)
        {
            gameWindow = gameWindowHandle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="debug"></param>
        /// <returns></returns>
        public Point? FindChestCoord(bool debug = false)
        {
            Mat lImage;
            CaptureScreen(); // Always update current screen before locating chest
            String obsImage = NodImages.CurrentSS;
            String[] chestImages = { NodImages.Chest1, NodImages.Chest2, NodImages.Chest3 };
            Point? result = null;
            foreach(String chest in chestImages)
            {
                try
                {
                    logger.sendMessage("Scanning: " + chest, LogType.DEBUG);
                    result = Draw(chest, NodImages.CurrentSS, out lImage);
                    if (debug) CvInvoke.Imshow(chest, lImage);
                    if (result != null) break;
                }catch(Exception ex)
                {
                    logger.sendMessage(ex.ToString(), LogType.WARNING);
                }
            }

            return result;
        }


        public bool ContainsMatch(String templateImage, String baseImage)
        {
            return getMatchCoord(templateImage, baseImage) != null;
        }

        public Point? getMatchCoord(String templateImage, String baseImage)
        {
            Mat lImage;
            CaptureScreen(); //Take Screenshot
            return Draw(templateImage, baseImage, out lImage, false);
        }

        public Point? FindImageMatchDebug(String model, String obs, bool debug = false, bool random = false)
        {
            Mat lImage;
            Point? result = null;
            try
            {
                logger.sendMessage("Scanning: " + model, LogType.DEBUG);
                result = Draw(model, obs, out lImage, random);
                if (debug) CvInvoke.Imshow(model, lImage);
            }
            catch (Exception ex)
            {
                logger.sendMessage(ex.ToString(), LogType.WARNING);
            }
          
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public void CaptureScreen()
        {
            try {
                Bitmap pImage = GetScreenCapture();
                pImage.Save(NodImages.CurrentSS, ImageFormat.Png);
            }catch(Exception ex)
            { 
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
                token.Cancel();
            }
        }

        public void CaptureScreen(String filePath)
        {
            Console.Out.WriteLine("Screen Caputre: " + filePath);
            Bitmap pImage = GetScreenCapture();
            pImage.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// TODO :: Copy w hat is done for #CaptureNeutralPoint() to retrieve a specific portion of the screen capture.
        /// </summary>
        public void CaptureScreenRight()
        {
            Bitmap pImage = GetScreenCapture();

            pImage.Save(NodImages.CurrentSS_Right, ImageFormat.Png);
        }

        public void CaputreNeutralPoint()
        {
            Bitmap pImage = GetScreenCapture();
            pImage.Save(NodImages.CurrentSS, ImageFormat.Png);

            if (pImage.Width > 700 && pImage.Height > 500)
            {
                // Retrieve specific portion of screen capture
                Rectangle cloneRect = new Rectangle(100, 100, 600, 400);
                Bitmap cloneBitmap = pImage.Clone(cloneRect, pImage.PixelFormat);

                cloneBitmap.Save(NodImages.NeutralSS, ImageFormat.Png);

                cloneBitmap.Dispose();
            }
            else pImage.Save(NodImages.NeutralSS, ImageFormat.Png);

            pImage.Dispose();
        }

        public List<Rectangle> FindTemplateMatchWithXConstraint(String templateImage, int xConstraint, bool lessThanX, bool updateCurrentScreen = false)
        {
            CaptureScreen(NodImages.CompareResultX);
            //CaptureScreen(NodImages.CompareResultX);
            Image<Bgr, byte> source = new Image<Bgr, byte>(NodImages.CompareResultX); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(templateImage); // Image A

            var matches = new List<Rectangle>();
            Rectangle prevMatch = new Rectangle();
            using (Image<Bgr, byte> imgSrc = source.Copy())
            {
                while (true)
                {
                    //updated and changed TemplateMatchingType- CcoeffNormed.
                    using (Image<Gray, float> result = imgSrc.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        var threshold = CvInvoke.Threshold(result, result, 0.75, 1, ThresholdType.ToZero);
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        if (maxValues[0] > threshold)
                        {
                            Rectangle match = new Rectangle(maxLocations[0], template.Size);

                            if (lessThanX && match.X < xConstraint)
                            {
                                imgSrc.Draw(match, new Bgr(Color.Red), -1);

                                matches.Add(match);
                            }
                            else if(!lessThanX && match.X > xConstraint)
                            {
                                imgSrc.Draw(match, new Bgr(Color.Red), -1);
                                matches.Add(match);
                            }
                            else
                            {
                                imgSrc.Draw(match, new Bgr(Color.Pink), -1);
                            }

                            if (prevMatch == match) return matches;
                            prevMatch = match;
                        }
                        else
                        {
                            break;
                        }

                        imgSrc.Bitmap.Save(NodImages.CompareResultX, ImageFormat.Png);
                    }
                }
            }

            return matches;
        }

        public List<Rectangle> FindTemplateMatchWithYConstraint(String templateImage, int yConstraint, bool lessThanY, bool updateCurrentScreen = false)
        {
            CaptureScreen(NodImages.CompareResultY);

            //CaptureScreen(NodImages.CompareResultY);
            Image<Bgr, byte> source = new Image<Bgr, byte>(NodImages.CompareResultY); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(templateImage); // Image A

            var matches = new List<Rectangle>();
            Rectangle prevMatch = new Rectangle();
            using (Image<Bgr, byte> imgSrc = source.Copy())
            {
                while (true)
                {
                    //updated and changed TemplateMatchingType- CcoeffNormed.
                    using (Image<Gray, float> result = imgSrc.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        var threshold = CvInvoke.Threshold(result, result, 0.75, 1, ThresholdType.ToZero);
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        if (maxValues[0] > threshold)
                        {
                            Rectangle match = new Rectangle(maxLocations[0], template.Size);

                            if (lessThanY && match.Y < yConstraint)
                            {
                                imgSrc.Draw(match, new Bgr(Color.Red), -1);
                                matches.Add(match);
                            }
                            else if (!lessThanY && match.Y > yConstraint)
                            {
                                imgSrc.Draw(match, new Bgr(Color.Red), -1);
                                matches.Add(match);
                            }
                            else
                            {
                                imgSrc.Draw(match, new Bgr(Color.Pink), -1);
                            }

                            if (prevMatch == match) return matches;
                            prevMatch = match;
                        }
                        else
                        {
                            break;
                        }

                        imgSrc.Bitmap.Save(NodImages.CompareResultY, ImageFormat.Png);
                    }
                }
            }

            return matches;
        }

        public void findMatchTest(String baseImage, String templateImage, Color invMatchColor, Color storageMatchColor)
        {
            Image<Bgr, byte> source = new Image<Bgr, byte>(baseImage); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(templateImage); // Image A

            var inventory = new List<Rectangle>();
            var storage = new List<Rectangle>();

            using (Image<Bgr, byte> imgSrc = source.Copy())
            {
                while (true)
                {
                    //updated and changed TemplateMatchingType- CcoeffNormed.
                    using (Image<Gray, float> result = imgSrc.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        var threshold = CvInvoke.Threshold(result, result, 0.75, 1, ThresholdType.ToZero);
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                        if (maxValues[0] > threshold)
                        {
                            Rectangle match = new Rectangle(maxLocations[0], template.Size);

                            if (match.X > 950)
                            {
                                imgSrc.Draw(match, new Bgr(invMatchColor), -1);
                                inventory.Add(match);
                            }
                            else
                            {
                                if (match.Y > 800)
                                {
                                    imgSrc.Draw(match, new Bgr(storageMatchColor), -1);
                                    storage.Add(match);
                                }
                                else
                                {
                                    imgSrc.Draw(match, new Bgr(Color.HotPink), -1);
                                };
                            }

                            imgSrc.Bitmap.Save(NodImages.CompareResult, ImageFormat.Png);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            Console.Out.WriteLine("Complete: " + templateImage);
        }


        private Image<Bgr, byte> copySubImage(Rectangle rect, Image<Bgr, byte> image)
        {
            Rectangle roi = image.ROI;
            image.ROI = rect;
            Image <Bgr, byte> newImage = image.Copy();
            image.ROI = roi;

            return newImage;
        }

        public bool isRectEmpty(Rectangle rect, String baseImage)
        {
            try
            {
                //CaptureScreen();
                CaptureScreen(baseImage);

                Image<Bgr, byte> source = new Image<Bgr, byte>(baseImage); // Image B
                copySubImage(rect, source).Save(NodImages.Temp_Inventory_1);
                Image<Bgr, byte> filteredSource = new Image<Bgr, byte>(NodImages.Temp_Inventory_1);

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

        public bool isRectEmpty(String baseImage)
        {
            try
            {
                CaptureScreen();

                Image<Bgr, byte> source = new Image<Bgr, byte>(baseImage); // Image B

                var temp2 = source.GetAverage();
                    var temp = source.CountNonzero();
                    if (temp2.Red < 5 && temp2.Blue < 5 && temp2.Green < 5)
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

        public bool doesRectContainTemplateImage(Rectangle rect, String templateImage, String baseImage)
        {
            try
            {
                CaptureScreen();

                Image<Bgr, byte> source = new Image<Bgr, byte>(baseImage); // Image B
                Image<Bgr, byte> template = new Image<Bgr, byte>(templateImage); // Image 

                template.Bitmap.Save(NodImages.Temp_Inventory_2);

                rect = new Rectangle(rect.X+1, rect.Y+1, template.Width, template.Height);

                copySubImage(rect, source).Save(NodImages.Temp_Inventory_1);
                Image<Bgr, byte> filteredSource = new Image<Bgr, byte>(NodImages.Temp_Inventory_1);
                //filteredSource.Bitmap.Save(NodImages.Temp_Inventory_1);
                //Image<Bgr, byte> filteredSource = copySubImage(rect, source);

                using (Image<Gray, float> result = filteredSource.MatchTemplate(template, TemplateMatchingType.CcorrNormed))
                {
                    var threshold = CvInvoke.Threshold(result, result, 0.95, 1, ThresholdType.ToZero);
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                    if (maxValues[0] > threshold)
                    {
                        return true;
                    }

                    return false;
                }
            } catch(Exception ex)
            {
                Console.Out.WriteLine(ex.StackTrace);
                // Sometimes throws exception if template match is not found
                // Ignore
            }
            return false;
        }

        public Point? FindMatchTemplate(String baseImage, String templateImage,bool updateCurrentScreen = false)
        {
            if(updateCurrentScreen) CaptureScreen(NodImages.CurrentSS);

            Image<Bgr, byte> source = new Image<Bgr, byte>(baseImage); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(templateImage); // Image A
            Image<Bgr, byte> imageToShow = source.Copy();

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
                if (maxValues[0] > 0.9)
                {
                    // This is a match. Do something with it, for example draw a rectangle around it.
                    Rectangle match = new Rectangle(maxLocations[0], template.Size);
                    imageToShow.Draw(match, new Bgr(Color.Red), 3);
                    imageToShow.Bitmap.Save(NodImages.CompareResult, ImageFormat.Png);

                    return maxLocations[0];
                }

                new Bitmap(20, 20).Save(NodImages.CompareResult);
                return null;
            }
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
                        int rand = points.Length-1; // retrieve last point
                        if(random) rand = new Random().Next(0, points.Length - 1); // random point
                        resultPoint =  points[rand];
                    }

                }
                #endregion

                return resultPoint;
            }
        }

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
                }

                pImage.Save(NodImages.PlayerDebug, ImageFormat.Png);
                return pImage;
            }
            catch (Exception ex)
            {
                // There was a problem, stopping bot
                //logger.sendLog(ex.StackTrace, LogType.ERROR);
                Console.Out.WriteLine(ex.StackTrace);
                token.Cancel();
            }
            //finally
            //{
            //    bool result = UpdateWindow(hWnd);
            //    if (result) Console.Out.WriteLine("Updated handle successffully?");
            //    else
            //    {
            //        Console.Out.WriteLine("Well that was a dud..");
            //        IntPtr lastErr = GetLastError();
            //        Console.Out.WriteLine(lastErr);
            //    }
            //}

            return new Bitmap(0, 0);
        }

        /***
         * 
         * 
         **/

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

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


    }
}
