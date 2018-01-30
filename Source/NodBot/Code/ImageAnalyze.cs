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

        public bool ContainsMatch(String modelImagePath, String observedImagePath)
        {
            Mat lImage;
            CaptureScreen(); //Take Screenshot
            return Draw(modelImagePath, observedImagePath, out lImage) != null;
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
            IntPtr hWnd = Input.getNodiatisWindowHandle();

            Rectangle rctForm = new Rectangle();
            GetWindowRect(hWnd, ref rctForm);

            using (Graphics grfx = Graphics.FromHdc(GetWindowDC(hWnd)))
            {
                rctForm = Rectangle.Round(grfx.VisibleClipBounds);
            }
            Bitmap pImage = new Bitmap((int)(rctForm.Width /*/ 1.5*/), rctForm.Height);  // Clip off the right side of the screen capture to remove inventory
                                                                                     // This is useful for not getting false positives when looking for 
                                                                                     // similar symbols for combat state.
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

            pImage.Save(NodImages.CurrentSS, ImageFormat.Png);
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
