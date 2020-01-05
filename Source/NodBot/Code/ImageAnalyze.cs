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

namespace NodBot.Code
{
    public class ImageAnalyze
    {
        private Logger mLogger;
        public ImageAnalyze(Logger aLogger)
        {
            mLogger = aLogger;
        }
        public ImageAnalyze()
        {
        }

        public Point? FindChestCoord(bool debug = false)
        {
            Mat lImage;
            CaptureScreen(); //Take Screenshot
            String obsImage = NodImages.CurrentSS;
            String[] chestImages = { NodImages.Chest1, NodImages.Chest2, NodImages.Chest3 };
            Point? result = null;
            foreach(String chest in chestImages)
            {
                try
                {
                    mLogger.sendMessage("Scanning: " + chest, LogType.DEBUG);
                    result = Draw(chest, NodImages.CurrentSS, out lImage);
                    if (debug) CvInvoke.Imshow(chest, lImage);
                    if (result != null) break;
                }catch(Exception ex)
                {
                    mLogger.sendMessage(ex.ToString(), LogType.WARNING);
                }
            }

            return result;
        }

        public bool ContainsMatch(String modelImagePath, String observedImagePath, Boolean isTemplateMatch = false)
        {
            Mat lImage;
            CaptureScreen(); //Take Screenshot
            if(isTemplateMatch) return FindMatchTemplate(observedImagePath, modelImagePath) != null;
            else return Draw(modelImagePath, observedImagePath, out lImage) != null;
        }

        public Point? getMatchCoord(String modelImagePath, String observedImagePath)
        {
            Mat lImage;
            CaptureScreen(); //Take Screenshot
            return Draw(modelImagePath, observedImagePath, out lImage, false);
        }

        public Point? FindImageMatchDebug(String model, String obs, bool debug = false, bool random = false)
        {
            Mat lImage;
            Point? result = null;
            try
            {
                mLogger.sendMessage("Scanning: " + model, LogType.DEBUG);
                result = Draw(model, obs, out lImage, random);
                if (debug) CvInvoke.Imshow(model, lImage);
            }
            catch (Exception ex)
            {
                mLogger.sendMessage(ex.ToString(), LogType.WARNING);
            }
          
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public static void CaptureScreen()
        {
            Bitmap pImage = GetScreenCapture();
            pImage.Save(NodImages.CurrentSS, ImageFormat.Png);
        }

        /// <summary>
        /// TODO :: Copy w hat is done for #CaptureNeutralPoint() to retrieve a specific portion of the screen capture.
        /// </summary>
        public static void CaptureScreenRight()
        {
            Bitmap pImage = GetScreenCapture();

            pImage.Save(NodImages.CurrentSS_Right, ImageFormat.Png);
        }

        public static void CaputreNeutralPoint()
        {
            Bitmap pImage = GetScreenCapture();

            if (pImage.Width > 700 && pImage.Height > 500)
            {
                // Retrieve specific portion of screen capture
                Rectangle cloneRect = new Rectangle(100, 100, 600, 400);
                System.Drawing.Imaging.PixelFormat format = pImage.PixelFormat;
                Bitmap cloneBitmap = pImage.Clone(cloneRect, format);

                cloneBitmap.Save(NodImages.NeutralSS, ImageFormat.Png);

                cloneBitmap.Dispose();
            }
            else pImage.Save(NodImages.NeutralSS, ImageFormat.Png);

            pImage.Dispose();
        }

        public Point? FindMatchTemplate(String baseImage, String templateImage)
        {
            CaptureScreen();

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

                    //IntPtr hWnd = FindWindow(null, "Calculator");
                    //if (hWnd != null)
                    //{
                    //    Graphics g = Graphics.FromHwnd(hWnd);
                    //    g.DrawImage((Image)imageToShow.Bitmap, 0, 0, g.VisibleClipBounds.Width, g.VisibleClipBounds.Height);
                    //    g.Dispose();
                    //}

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

        private static Bitmap GetScreenCapture()
        {
            IntPtr hWnd = Input.getNodiatisWindowHandle();

            Rectangle rctForm = new Rectangle();
            GetWindowRect(hWnd, ref rctForm);

            using (Graphics grfx = Graphics.FromHdc(GetWindowDC(hWnd)))
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

            return pImage;
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

    }
}
