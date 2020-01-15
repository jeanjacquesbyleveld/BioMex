using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using BioMex_First.BioMexService;
using Microsoft.Win32;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing;
using System.Windows.Controls.DataVisualization.Charting;
using System.Diagnostics;
using System.Windows.Forms;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Drawing.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Threading;
using Emgu.CV.Cuda;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.XFeatures2D;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BioMex_Admin;


namespace BioMex_First
{
    public partial class MainWindow  // : Window
    {

        #region "Variables"

        private const int FACIAL_THRESHOLD = 1200;
        private const int FACIAL_MINIMUM = 500;
        private bool firstMeasured = false;
        private bool measureFace = true;
        private double totalAverage = 0.0;
        private double useThis = 0.0;

        private Matrix<byte> observedDesc = new Matrix<byte>(30, 50);
        private Matrix<byte> modelDesc = new Matrix<byte>(30, 50);
        private string savedDescriptors = "";
        private string savedKeypoints = "";

        private Image<Gray, Byte> recordedImage;
        private VectorOfKeyPoint recordedKeypoints = new VectorOfKeyPoint();
        Mat homography = null;
        private bool recordedAlready = false;
        private bool match = false;

        public KeypointsDescriptors keyDescClass = new KeypointsDescriptors();
        private string keyDescString = "";

        [StructLayout(LayoutKind.Sequential)]
        private struct Kbdllhookstruct
        {
            public readonly Keys key;
            private readonly int scanCode;
            private readonly int flags;
            private readonly int time;
            private readonly IntPtr extra;
        }

        private bool started = false;
        private Service1Client _service;

        private PaddingMode encrPadding = new PaddingMode();

        System.Windows.Threading.DispatcherTimer _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        //System level functions to be used for hook and unhook keyboard input
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        //Declaring Global objects
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private bool activeForTyping = true;
        private int keyCount;
        private bool startedTyping = false;
        private int shiftCategory1; //only left shift is used
        private int shiftCategory2; //only right shift is used
        private int shiftCategory3; //opposite shift is used
        private bool serviceActivated = false;

        //Shift variables, boolean for pressed and integers to keep count of pressed times
        private bool shiftPressed;
        private bool leftShiftPressed;
        private int leftShiftCount;
        private int rightShiftCount;

        //Timer and Integer for timing downpresses of keys
        private DateTime keyDownTimer { get; set; }
        private int keyDownTimeCount;

        //Timer and Integer for timing latency between keys
        private DateTime keyLatencyTimer { get; set; }
        private int keyLatencyCount;

        //Timer and Integer for timing of entire password
        private DateTime passwordTimer { get; set; }
        private int passwordTimeCount;

        //Boolean and integer values to keep track of key order
        private bool keyCurrentlyPressed;
        private int negativeKeyOrderCount;

        //Timer and integer for timing paired keys speed
        private DateTime pairedKeysTimer { get; set; }
        private bool firstKeyPressed;
        private bool recordPairedKeys;

        //Used to keep track of key metrics
        private List<int> _keyDownTime;
        private List<int> _keyLatency;
        private List<int> _keyOrder;
        private List<int> _pairedKeysSpeed;

        Capture capture;
        private bool _captureInProgress;
                [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private List<KeyValuePair<string, int>> data1A = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data1B = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data2A = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data2B = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data3A = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data3B = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data4A = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data4B = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data5A = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data5B = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data6A = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data6B = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data7A = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> data7B = new List<KeyValuePair<string, int>>();
        private List<KeyValuePair<string, int>> dataHorizon = new List<KeyValuePair<string, int>>();
        
        private LineSeries series1A = new LineSeries();
        private LineSeries series1B = new LineSeries();
        private LineSeries series2A = new LineSeries();
        private LineSeries series2B = new LineSeries();
        private LineSeries series3A = new LineSeries();
        private LineSeries series3B = new LineSeries();
        private ColumnSeries series4A = new ColumnSeries();
        private ColumnSeries series4B = new ColumnSeries();
        private ColumnSeries series5A = new ColumnSeries();
        private ColumnSeries series5B = new ColumnSeries();
        private ColumnSeries series6A = new ColumnSeries();
        private ColumnSeries series6B = new ColumnSeries();
        private ColumnSeries series7A = new ColumnSeries();
        private ColumnSeries series7B = new ColumnSeries();

        #endregion

        #region FacialRecog

        public static BitmapSource ToBitmapSource(IImage image)
        {
           using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop
                  .Imaging.CreateBitmapSourceFromHBitmap(
                  ptr,
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if ((!started))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {

                    started = true;

                    if(measureFace)
                    {
                        try
                        {
                            Mat frame = new Mat();
                            capture.Retrieve(frame, 0);

                            Mat newFrame = frame;

                            Image<Hls, Byte> hlsImage = new Image<Hls, byte>(newFrame.Bitmap);

                            Image<Gray, Byte> imageL = hlsImage[1];

                            imageL._EqualizeHist();

                            hlsImage[1] = imageL;

                            List<Rectangle> faces = new List<Rectangle>();

                            Detect(frame, "haarcascade_frontalface_default.xml", faces, false, false);

                            foreach (Rectangle face in faces)
                                CvInvoke.Rectangle(frame, face, new Bgr(0,255,0).MCvScalar, 3);

                            //TO HERE
            
                            imgFace.Source = ToBitmapSource(frame);
                                                
                            Image<Gray, Byte> imgROI = null;

                            if (faces.Count > 0)
                            {
                                imgROI = imageL;
                                imgROI.ROI = CalculateClosestFace(faces);
                                imgFace2.Source = ToBitmapSource(imgROI);

                                FastDetector fastCPU = new FastDetector(10, true);
                                VectorOfKeyPoint observedKeyPoints = new VectorOfKeyPoint();

                                BriefDescriptorExtractor descriptor = new BriefDescriptorExtractor();
                                SURF surfFeat = new SURF(500);
                                SIFT siftFeat = new SIFT();
                            
                                fastCPU.DetectRaw(imgROI, observedKeyPoints, null);

                                //siftFeat.Compute(imgROI, observedKeyPoints, observedDesc);
                                surfFeat.Compute(imgROI, observedKeyPoints, observedDesc);
                                //descriptor.Compute(imgROI, observedKeyPoints, observedDesc);

                                BFMatcher matcher = new BFMatcher(DistanceType.L2);

                                matcher.Add(observedDesc);


                                if(recordedAlready)
                                {
                                    modelDesc = observedDesc;
                                    recordedKeypoints = observedKeyPoints;
                                    recordedImage = imgROI;
                                    recordedAlready = false;
                                    Console.WriteLine("Recorded");
                                }

                                Matrix<byte> mask = new Matrix<byte>(30, 50);

                                VectorOfVectorOfDMatch vectMatch = new VectorOfVectorOfDMatch();

                                Mat resultMat = new Mat();

                                if(match)
                                {
                                    match = false;                                   

                                    using(Matrix<float> Dist = new Matrix<float>(observedDesc.Rows,2))
                                    {
                                        matcher.KnnMatch(observedDesc, vectMatch, 2, null);
                                        mask = new Matrix<byte>(Dist.Rows, 1);
                                        mask.SetValue(255);
                                        Features2DToolbox.VoteForUniqueness(vectMatch, 0.8, mask.ToUMat().ToMat(AccessType.Read));
                                    }                                 
                                    
                                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                                    if(nonZeroCount >= 4)
                                    {
                                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(recordedKeypoints, observedKeyPoints, vectMatch, mask.ToUMat().ToMat(AccessType.Read), 1.5, 20);
                                        if(nonZeroCount >= 4)
                                        {
                                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(recordedKeypoints, observedKeyPoints, vectMatch, mask.ToUMat().ToMat(AccessType.Read), 2);
                                        }
                                    }

                                    
                                    if(homography != null)
                                    {
                                        Rectangle rect = recordedImage.ROI;
                                        PointF[] pts = new PointF[]{
                                            new PointF(rect.Left, rect.Bottom),
                                            new PointF(rect.Right, rect.Bottom),
                                            new PointF(rect.Right, rect.Top),
                                            new PointF(rect.Left, rect.Top)};
                                    
                                        Image<Bgr, Byte> newResult = resultMat.ToImage<Bgr,Byte>();

                                        newResult.DrawPolyline(Array.ConvertAll<PointF, System.Drawing.Point>(pts, System.Drawing.Point.Round), true, new Bgr(System.Drawing.Color.Red), 2);

                                    }
                                    
                            }

                                using (Matrix<float> Dist = new Matrix<float>(observedDesc.Rows, 2))
                                {
                                    matcher.KnnMatch(observedDesc, vectMatch, 2, null);
                                    mask = new Matrix<byte>(Dist.Rows, 1);
                                    mask.SetValue(255);
                                    Features2DToolbox.VoteForUniqueness(vectMatch, 0.8, mask.ToUMat().ToMat(AccessType.Read));
                                }        


                                if (firstMeasured)
                                {
                                    Features2DToolbox.DrawMatches(recordedImage, recordedKeypoints, imgROI, observedKeyPoints, vectMatch, resultMat, new MCvScalar(0, 255, 0), new MCvScalar(255, 255, 255), null, Emgu.CV.Features2D.Features2DToolbox.KeypointDrawType.Default);

                                    imgFaceMatch.Source = ToBitmapSource(resultMat);
                                }
                                else
                                {
                                    if (observedKeyPoints.Size >= FACIAL_MINIMUM)
                                    {
                                        firstMeasured = true;
                                        modelDesc = observedDesc;
                                        recordedKeypoints = observedKeyPoints;
                                        recordedImage = imgROI;
                                        Console.WriteLine("Recorded");
                                    }
                                }


                            Mat keypointsFrame = imgROI.ToUMat().ToMat(AccessType.Read);

                            string serialKeys = SerializeObject(observedKeyPoints.ToArray());   //USE THIS METHOD TO SERIALIZE AND DESERIALIZE
                            VectorOfKeyPoint newVec = new VectorOfKeyPoint();
                            newVec.Push(DeSerializeObject<MKeyPoint[]>(serialKeys));
                                

                            Features2DToolbox.DrawKeypoints(keypointsFrame, newVec, keypointsFrame, new Bgr(0, 255, 0), Emgu.CV.Features2D.Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

                            imgFace3.Source = ToBitmapSource(keypointsFrame);
                                
                            keyDescClass.descriptors = observedDesc;
                            keyDescClass.keyPoints = observedKeyPoints;

                            
                            keyDescString = SerializeObject<KeypointsDescriptors>(keyDescClass);

                            lblamount.Content = observedKeyPoints.Size;
                            if (observedKeyPoints.Size >= FACIAL_THRESHOLD)
                            {
                                measureFace = false;
                            }
                                              
                        }   
         


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while attempting to process image");
                        Console.WriteLine(ex.Message);
                    }

                }
                    //dispatcherTimer_Tick(sender, arg);
                }

               ));
        }
        }

        private Rectangle CalculateClosestFace(List<Rectangle> lstFace)
        {
             var temp = lstFace[0];

             for (int i = 0; i < lstFace.Count;i++ )
             {                  
                       if ((lstFace[i].Height > temp.Height) && (lstFace[i].Width > temp.Width))
                       {
                           temp = lstFace[i + 1];
                       }
             }

             return temp;
        }

        public static void Detect(Mat image, String faceFileName, List<Rectangle> faces, bool tryUseCuda, bool tryUseOpenCL)
        {
#if !(IOS || NETFX_CORE)
            if (tryUseCuda && CudaInvoke.HasCuda)
            {
                using (CudaCascadeClassifier face = new CudaCascadeClassifier(faceFileName))
                {
                    face.ScaleFactor = 1.1; //attempt 1.1
                    face.MinNeighbors = 10; //attempt 10
                    face.MinObjectSize = System.Drawing.Size.Empty;
                    
                    using (CudaImage<Bgr, Byte> gpuImage = new CudaImage<Bgr, byte>(image))
                    using (CudaImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                    using (GpuMat region = new GpuMat())
                    {
                        face.DetectMultiScale(gpuGray, region);
                        Rectangle[] faceRegion = face.Convert(region);
                        faces.AddRange(faceRegion);
                    }
                }           
            }
            else
#endif
            {
                CvInvoke.UseOpenCL = tryUseOpenCL && CvInvoke.HaveOpenCLCompatibleGpuDevice;
                
                //Read the HaarCascade objects
                using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                {
                    using (UMat ugray = new UMat())
                    {
                        //if(!ugray.IsEmpty)
                        CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                        CvInvoke.EqualizeHist(ugray, ugray);

                        Rectangle[] facesDetected = face.DetectMultiScale(
                           ugray,
                           1.1,
                           10,
                           new System.Drawing.Size(25, 25));

                        faces.AddRange(facesDetected);

                    }
                }
            }

        }


        #endregion

        #region Keystrokes

        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                var objKeyInfo = (Kbdllhookstruct)Marshal.PtrToStructure(lp, typeof(Kbdllhookstruct));
                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.LControlKey || objKeyInfo.key == Keys.RControlKey || objKeyInfo.key == Keys.F4 || objKeyInfo.key == Keys.Tab || objKeyInfo.key == Keys.Delete || objKeyInfo.key == Keys.Escape || objKeyInfo.key == Keys.Menu || objKeyInfo.key == Keys.LMenu || objKeyInfo.key == Keys.RMenu || objKeyInfo.key == Keys.Alt) // Disabling Windows keys
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        private int CalculateTotalTime(DateTime first, DateTime second)
        {
            var millisecs = (first - second).Milliseconds;
            var secs = (first - second).Seconds;
            var mins = (first - second).Minutes;

            int total = millisecs + (secs * 1000) + (mins * 60000);

            return total;
        }

        private void tb_KeyUp(object sender, KeyEventArgs e)  //Handles most keystroke events
        {
            if (recordPairedKeys)
            {
                firstKeyPressed = false;
                recordPairedKeys = false;

                _pairedKeysSpeed.Add(CalculateTotalTime(DateTime.Now, pairedKeysTimer));
            }

            keyCurrentlyPressed = false;

            if (ValidateKey(e)) //ADDED AS A FIX
            {
                if (!startedTyping)
                {
                    passwordTimer = DateTime.Now;
                }
                startedTyping = true;
            } //FIX STOPPED

            if (e.Key == Key.Enter)
            {
                if (activeForTyping)
                {
                    passwordTimeCount = CalculateTotalTime(DateTime.Now, passwordTimer);
                    DetermineShiftCategory();

                    activeForTyping = false;

                    LogUserIn();
                }
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                if (e.Key == Key.LeftShift)
                {
                    leftShiftPressed = true;
                    ClassifyShift(e.Key.ToString(), true);
                    leftShiftCount += 1;
                }
                else
                {
                    leftShiftPressed = false;
                    ClassifyShift(e.Key.ToString(), false);
                    rightShiftCount += 1;
                }

                shiftPressed = true;
            }
            else
            {
                shiftPressed = false;
            }

            if (e.Key == Key.Back)
            {
                ResetStats();
                activeForTyping = true;
                startedTyping = false;
                if(pwbPassword.IsFocused)
                {
                    pwbPassword.Password = "";
                }
            }
            else
            {
                if (ValidateKey(e))
                {
                    keyLatencyTimer = DateTime.Now;
                    CalculateKeyCount(e);
                    keyDownTimeCount = CalculateTotalTime(DateTime.Now, keyDownTimer);
                    _keyDownTime.Add(keyDownTimeCount);
                }

            }

            keyDownTimeCount = 0;
        }

        private void ResetStats()
        {
            //pwbPassword.Password = "";
            keyCount = 0;
            leftShiftCount = 0;
            rightShiftCount = 0;
            shiftPressed = false;
            startedTyping = false;
            keyCurrentlyPressed = false;
            negativeKeyOrderCount = 0;
            recordPairedKeys = true;
            keyDownTimeCount = 0;
            activeForTyping = true;

            _keyDownTime.Clear();
            _keyLatency.Clear();
            _keyOrder.Clear();
            _pairedKeysSpeed.Clear();

            _keyDownTime = new List<int>();
            _keyLatency = new List<int>();
            _keyOrder = new List<int>();
            _pairedKeysSpeed = new List<int>();
        }

        private void CalculateKeyCount(KeyEventArgs key)
        {
            switch (key.Key)
            {
                case Key.A:
                    keyCount += 1;
                    break;
                case Key.B:
                    keyCount += 1;
                    break;
                case Key.C:
                    keyCount += 1;
                    break;
                case Key.D:
                    keyCount += 1;
                    break;
                case Key.E:
                    keyCount += 1;
                    break;
                case Key.F:
                    keyCount += 1;
                    break;
                case Key.G:
                    keyCount += 1;
                    break;
                case Key.H:
                    keyCount += 1;
                    break;
                case Key.I:
                    keyCount += 1;
                    break;
                case Key.J:
                    keyCount += 1;
                    break;
                case Key.K:
                    keyCount += 1;
                    break;
                case Key.L:
                    keyCount += 1;
                    break;
                case Key.M:
                    keyCount += 1;
                    break;
                case Key.N:
                    keyCount += 1;
                    break;
                case Key.O:
                    keyCount += 1;
                    break;
                case Key.P:
                    keyCount += 1;
                    break;
                case Key.Q:
                    keyCount += 1;
                    break;
                case Key.R:
                    keyCount += 1;
                    break;
                case Key.S:
                    keyCount += 1;
                    break;
                case Key.T:
                    keyCount += 1;
                    break;
                case Key.U:
                    keyCount += 1;
                    break;
                case Key.V:
                    keyCount += 1;
                    break;
                case Key.W:
                    keyCount += 1;
                    break;
                case Key.X:
                    keyCount += 1;
                    break;
                case Key.Y:
                    keyCount += 1;
                    break;
                case Key.Z:
                    keyCount += 1;
                    break;
                case Key.NumPad0:
                    keyCount += 1;
                    break;
                case Key.NumPad1:
                    keyCount += 1;
                    break;
                case Key.NumPad2:
                    keyCount += 1;
                    break;
                case Key.NumPad3:
                    keyCount += 1;
                    break;
                case Key.NumPad4:
                    keyCount += 1;
                    break;
                case Key.NumPad5:
                    keyCount += 1;
                    break;
                case Key.NumPad6:
                    keyCount += 1;
                    break;
                case Key.NumPad7:
                    keyCount += 1;
                    break;
                case Key.NumPad8:
                    keyCount += 1;
                    break;
                case Key.NumPad9:
                    keyCount += 1;
                    break;
                case Key.D0:
                    keyCount += 1;
                    break;
                case Key.D1:
                    keyCount += 1;
                    break;
                case Key.D2:
                    keyCount += 1;
                    break;
                case Key.D3:
                    keyCount += 1;
                    break;
                case Key.D4:
                    keyCount += 1;
                    break;
                case Key.D5:
                    keyCount += 1;
                    break;
                case Key.D6:
                    keyCount += 1;
                    break;
                case Key.D7:
                    keyCount += 1;
                    break;
                case Key.D8:
                    keyCount += 1;
                    break;
                case Key.D9:
                    keyCount += 1;
                    break;

            }
        }

        private bool ValidateKey(KeyEventArgs key)
        {
            switch (key.Key)
            {
                case Key.A:
                    return true;

                case Key.B:
                    return true;

                case Key.C:
                    return true;

                case Key.D:
                    return true;

                case Key.E:
                    return true;

                case Key.F:
                    return true;

                case Key.G:
                    return true;

                case Key.H:
                    return true;

                case Key.I:
                    return true;

                case Key.J:
                    return true;

                case Key.K:
                    return true;

                case Key.L:
                    return true;

                case Key.M:
                    return true;

                case Key.N:
                    return true;

                case Key.O:
                    return true;

                case Key.P:
                    return true;

                case Key.Q:
                    return true;

                case Key.R:
                    return true;

                case Key.S:
                    return true;

                case Key.T:
                    return true;

                case Key.U:
                    return true;

                case Key.V:
                    return true;

                case Key.W:
                    return true;

                case Key.X:
                    return true;

                case Key.Y:
                    return true;

                case Key.Z:
                    return true;

                case Key.NumPad0:
                    return true;

                case Key.NumPad1:
                    return true;

                case Key.NumPad2:
                    return true;

                case Key.NumPad3:
                    return true;

                case Key.NumPad4:
                    return true;

                case Key.NumPad5:
                    return true;

                case Key.NumPad6:
                    return true;

                case Key.NumPad7:
                    return true;

                case Key.NumPad8:
                    return true;

                case Key.NumPad9:
                    return true;

                case Key.D0:
                    return true;

                case Key.D1:
                    return true;

                case Key.D2:
                    return true;

                case Key.D3:
                    return true;

                case Key.D4:
                    return true;

                case Key.D5:
                    return true;

                case Key.D6:
                    return true;

                case Key.D7:
                    return true;

                case Key.D8:
                    return true;

                case Key.D9:
                    return true;

            }

            return false;
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (ValidateKey(e))
            {
                if (!firstKeyPressed)
                {
                    firstKeyPressed = true;
                    recordPairedKeys = false;

                    pairedKeysTimer = DateTime.Now;
                }
                else
                    recordPairedKeys = true;



                if (keyCurrentlyPressed)
                {
                    negativeKeyOrderCount += 1;
                    _keyOrder.Add(1);
                }
                else
                {
                    _keyOrder.Add(0);
                }

            }

            keyCurrentlyPressed = true;
            keyDownTimer = DateTime.Now;

            if (startedTyping && ValidateKey(e))
            {
                keyLatencyCount = CalculateTotalTime(DateTime.Now, keyLatencyTimer);
                _keyLatency.Add(keyLatencyCount);
            }

        }

        private void ClassifyShift(string key, bool leftShiftUsed)
        {
            string[] leftKeyArray = { "a", "s", "d", "f", "q", "w", "e", "r", "t", "z", "x", "c", "v" };
            string[] rightKeyArray = { "y", "u", "i", "o", "p", "g", "h", "j", "k", "l", "b", "n", "m" };

            for (int i = 1; i < 13; i++)
            {
                if (key == leftKeyArray[i] && !leftShiftUsed)
                {
                    shiftCategory3 += 1;
                    break;
                }
                if (key == rightKeyArray[i] && leftShiftUsed)
                {
                    shiftCategory3 += 1;
                    break;
                }
                if (leftShiftUsed)
                {
                    shiftCategory1 += 1;
                }
                else
                {
                    shiftCategory2 += 1;
                }
            }
        }

        private int DetermineShiftCategory()
        {
            var totalShiftCount = leftShiftCount + rightShiftCount;
            int overrulingShiftCategory = 0;

            if (shiftCategory1 < shiftCategory2)
            {
                if (shiftCategory2 > shiftCategory3)
                    overrulingShiftCategory = 2;
                else if (shiftCategory2 < shiftCategory3 && shiftCategory3 > (totalShiftCount / 2))
                    overrulingShiftCategory = 3;
            }
            else if (shiftCategory1 > shiftCategory2)
            {
                if (shiftCategory1 > shiftCategory3)
                    overrulingShiftCategory = 1;
                else if (shiftCategory1 < shiftCategory3 && shiftCategory3 > (totalShiftCount / 2))
                    overrulingShiftCategory = 3;
            }
            else
                overrulingShiftCategory = 4;


            return overrulingShiftCategory;
        }

        #endregion

        #region FormMethods

        public MainWindow()
        {
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule; //Get Current Module
            objKeyboardProcess = CaptureKey; //Assign callback function each time keyboard process
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);

            try
            {
                var discard = EncryptText("nothing");

                capture = new Capture(0);
                capture.ImageGrabbed += ProcessFrame;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while attempting to create new capture");
                Console.WriteLine(ex.InnerException);
            }

            try
            {
                _service = new Service1Client();
                serviceActivated = _service.ActivateService(); //First service call to reduce time of login. Used to check if service is ready to receive data
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while creating a new service and making first call");
                Console.WriteLine(ex.InnerException);
            }

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            keyCount = 0;
            leftShiftCount = 0;
            rightShiftCount = 0;
            pwbPassword.KeyUp += tb_KeyUp;
            pwbPassword.KeyDown += tb_KeyDown;
            shiftPressed = false;
            keyCurrentlyPressed = false;
            negativeKeyOrderCount = 0;

            _keyDownTime = new List<int>();
            _keyLatency = new List<int>();
            _keyOrder = new List<int>();
            _pairedKeysSpeed = new List<int>();
            ResetStats();
            grbReport.Visibility = Visibility.Hidden;

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (capture != null)
                {
                    if (_captureInProgress)
                    {  //stop the capture
                        capture.Pause();
                    }
                    else
                    {
                        //start the capture
                        capture.Start();
                    }

                    _captureInProgress = !_captureInProgress;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while attempting to capture");
                Console.WriteLine(ex.InnerException);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            frmBioMex.Width = SystemParameters.FullPrimaryScreenWidth;
            frmBioMex.Height = SystemParameters.FullPrimaryScreenHeight;
            imgBack.Height = SystemParameters.FullPrimaryScreenHeight + 105;
            imgBack.Width = SystemParameters.FullPrimaryScreenWidth + 50;

            var marg = grbSignIn.Margin;
            marg.Left = (SystemParameters.FullPrimaryScreenWidth / 2) - 181;
            marg.Top = (SystemParameters.FullPrimaryScreenHeight / 2) - 113;
            grbSignIn.Margin = marg;

            //var margContact = grbContactAdmin.Margin;
            //margContact.Left = (SystemParameters.FullPrimaryScreenWidth / 2) - 108;
            //margContact.Top = (SystemParameters.FullPrimaryScreenHeight / 2) - 65;
            //grbContactAdmin.Margin = margContact;

            var margInfo = grbInfo.Margin;
            margInfo.Left = (SystemParameters.FullPrimaryScreenWidth / 2) - 108;
            margInfo.Top = (SystemParameters.FullPrimaryScreenHeight / 2) - 65;
            grbInfo.Margin = margInfo;

            var mrgLog = grbLoggedIn.Margin;
            mrgLog.Left = (SystemParameters.FullPrimaryScreenWidth / 2) - 80;
            mrgLog.Top = (SystemParameters.FullPrimaryScreenHeight / 2) - 35;
            grbLoggedIn.Margin = mrgLog;

            var mrgRep = grbReport.Margin;
            mrgRep.Left = (SystemParameters.FullPrimaryScreenWidth / 2) - 250;
            mrgRep.Top = (SystemParameters.FullPrimaryScreenHeight / 2) - 200;
            grbReport.Margin = mrgRep;

            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            Console.WriteLine(GetCurrentWallpaper());

            imgBack.Source = new BitmapImage(new Uri(GetCurrentWallpaper()));

            grbSignIn.Visibility = Visibility.Visible;
           // grbContactAdmin.Visibility = Visibility.Hidden;
            cvsBlur.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;
            grbLoggedIn.Visibility = Visibility.Hidden;

            ResetStats();
        }

        private void frmBioMex_Closing(object sender, CancelEventArgs e)
        {
            this.Dispatcher.InvokeShutdown();
            if (capture != null)
                capture.Dispose();
        }


        #endregion

        #region ValueChangedMethods
        
        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            txtUsername.Text = "";
            txtUsername.Foreground = Brushes.White;
        }

        private void pwbPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            pwbPassword.Foreground = Brushes.White;
            ResetStats();
        }

        //private void txtContactAdmin_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    //txtContactAdmin.Text = "";
        //    txtContactAdmin.Foreground = Brushes.Black;
        //}


        #endregion

        #region HelperMethods

        private void LogUserIn()
        {
            if (!activeForTyping && startedTyping)  //MAY CAUSE A PROBLEM LOGGING IN
            {
                grbReport.Visibility = Visibility.Visible;

                try
                {
                    var keyDownVarList = _service.RetrieveKeyDownTime(txtUsername.Text);
                    var keyLatencyVarList = _service.RetrieveKeyLatency(txtUsername.Text);
                    var keyPairedVarList = _service.RetrievePairedKeyTime(txtUsername.Text);

                    for (int i = 0; i < keyDownVarList.Length; i++)
                    {
                        data1A.Add(new KeyValuePair<string, int>(i.ToString(), keyDownVarList[i]));

                        if (_keyDownTime.Count == keyDownVarList.Length)
                        {
                            data1B.Add(new KeyValuePair<string, int>(i.ToString(), _keyDownTime[i]));
                        }
                    }

                    for (int i = 0; i < keyLatencyVarList.Length; i++)
                    {
                        data2A.Add(new KeyValuePair<string, int>(i.ToString(), keyLatencyVarList[i]));

                        if (_keyLatency.Count == keyLatencyVarList.Length)
                        {
                            data2B.Add(new KeyValuePair<string, int>(i.ToString(), _keyLatency[i]));
                        }
                    }

                    for (int i = 0; i < keyPairedVarList.Length; i++)
                    {
                        data3A.Add(new KeyValuePair<string, int>(i.ToString(), keyPairedVarList[i]));

                        if (_pairedKeysSpeed.Count == keyPairedVarList.Length)
                        {
                            data3B.Add(new KeyValuePair<string, int>(i.ToString(), _pairedKeysSpeed[i]));
                        }
                    }


                    int[] distanceArray = _service.RetrieveDistances(txtUsername.Text, passwordTimeCount, _keyOrder.ToArray(), _keyLatency.ToArray(), _keyDownTime.ToArray(), _pairedKeysSpeed.ToArray());

                    data4A.Add(new KeyValuePair<string, int>("Amount", distanceArray[0]));
                    data4B.Add(new KeyValuePair<string, int>("Threshold", 300));

                    data5A.Add(new KeyValuePair<string, int>("Amount", distanceArray[1]));
                    data5B.Add(new KeyValuePair<string, int>("Threshold", 400));

                    data6A.Add(new KeyValuePair<string, int>("Amount", distanceArray[2]));
                    data6B.Add(new KeyValuePair<string, int>("Threshold", 500));

                    data7A.Add(new KeyValuePair<string, int>("Amount", distanceArray[3]));
                    data7B.Add(new KeyValuePair<string, int>("Threshold", 300));

                    if(distanceArray[0] < 300)
                    {
                        int arr = distanceArray[0];
                        useThis = (((double)arr / (double)300) * 100) * 0.20;
                        totalAverage = useThis;
                        Console.WriteLine(((double)arr / (double)300) * 100);
                        Console.WriteLine((((double)arr / (double)300) * 100) * 0.20);
                        Console.WriteLine("Total Average:  " + totalAverage);
                    }
                    else
                    {
                        totalAverage = 20;
                    }
                    
                    if(distanceArray[1] < 400)
                    {
                        int arr = distanceArray[1];
                        useThis = ((double)arr / (double)400);
                        useThis = useThis * 100;
                        useThis = useThis * 0.2;
                        totalAverage += useThis;
                        Console.WriteLine("Total Average:  " + totalAverage);
                    }
                    else
                    {
                        totalAverage += 30;
                    }

                    if(distanceArray[2] < 500)
                    {
                        int arr = distanceArray[2];
                        useThis = (((double)arr / (double)500) * 100) * 0.30;
                        totalAverage += useThis;
                        Console.WriteLine("Total Average:  " + totalAverage);
                    }
                    else
                    {
                        totalAverage += 30;
                    }
                    
                    if(distanceArray[3] < 300)
                    {
                        int arr = distanceArray[3];
                        useThis = (((double)arr / (double)300) * 100) * 0.20;
                        totalAverage += useThis;
                        Console.WriteLine("Total Average:  " + totalAverage);
                    }
                    else
                    {
                        totalAverage += 20;
                    }
                    

                    //totalAverage = ((distanceArray[0] + distanceArray[1] + distanceArray[2] + distanceArray[3]) / 1500) * 100;

                    int finalTotal = (int)totalAverage;

                    lblKeystrokes.Content = "Keystroke % Match =   " + (100 - finalTotal);
                    Console.WriteLine("Keystroke % Match =   " + (100 - finalTotal));

                    totalAverage = 0.0;

                    series1A.DependentValuePath = "Value";
                    series1A.IndependentValuePath = "Key";
                    series1A.ItemsSource = data1A;
                    series1A.Title = "Stored Sample";

                    series1B.DependentValuePath = "Value";
                    series1B.IndependentValuePath = "Key";
                    series1B.ItemsSource = data1B;
                    series1B.Title = "Recorded Sample";

                    series2A.DependentValuePath = "Value";
                    series2A.IndependentValuePath = "Key";
                    series2A.ItemsSource = data2A;
                    series2A.Title = "Stored Sample";

                    series2B.DependentValuePath = "Value";
                    series2B.IndependentValuePath = "Key";
                    series2B.ItemsSource = data2B;
                    series2B.Title = "Recorded Sample";

                    series3A.DependentValuePath = "Value";
                    series3A.IndependentValuePath = "Key";
                    series3A.ItemsSource = data3A;
                    series3A.Title = "Stored Sample";

                    series3B.DependentValuePath = "Value";
                    series3B.IndependentValuePath = "Key";
                    series3B.ItemsSource = data3B;
                    series3B.Title = "Recorded Sample";

                    series4A.DependentValuePath = "Value";
                    series4A.IndependentValuePath = "Key";
                    series4A.ItemsSource = data4A;
                    series4A.Title = "Difference";

                    series4B.DependentValuePath = "Value";
                    series4B.IndependentValuePath = "Key";
                    series4B.ItemsSource = data4B;
                    series4B.Title = "Threshold";

                    series5A.DependentValuePath = "Value";
                    series5A.IndependentValuePath = "Key";
                    series5A.ItemsSource = data5A;
                    series5A.Title = "Difference";

                    series5B.DependentValuePath = "Value";
                    series5B.IndependentValuePath = "Key";
                    series5B.ItemsSource = data5B;
                    series5B.Title = "Threshold";

                    series6A.DependentValuePath = "Value";
                    series6A.IndependentValuePath = "Key";
                    series6A.ItemsSource = data6A;
                    series6A.Title = "Difference";

                    series6B.DependentValuePath = "Value";
                    series6B.IndependentValuePath = "Key";
                    series6B.ItemsSource = data6B;
                    series6B.Title = "Threshold";

                    series7A.DependentValuePath = "Value";
                    series7A.IndependentValuePath = "Key";
                    series7A.ItemsSource = data7A;
                    series7A.Title = "Difference";

                    series7B.DependentValuePath = "Value";
                    series7B.IndependentValuePath = "Key";
                    series7B.ItemsSource = data7B;
                    series7B.Title = "Threshold";
                    
                    chtKeyDown.Series.Add(series1A);
                    chtKeyDown.Series.Add(series1B);
                    chtKeyLatency.Series.Add(series2A);
                    chtKeyLatency.Series.Add(series2B);
                    chtPairedKey.Series.Add(series3A);
                    chtPairedKey.Series.Add(series3B);
                    chtPasswordSpeedThreshold.Series.Add(series4A);
                    chtPasswordSpeedThreshold.Series.Add(series4B);
                    chtKeyDownThreshold.Series.Add(series5A);
                    chtKeyDownThreshold.Series.Add(series5B);
                    chtKeyLatencyTreshold.Series.Add(series6A);
                    chtKeyLatencyTreshold.Series.Add(series6B);
                    chtPairedKeyThreshold.Series.Add(series7A);
                    chtPairedKeyThreshold.Series.Add(series7B);


                    string pass = _service.LogUserIn(EncryptText(txtUsername.Text), Sha512(pwbPassword.Password), keyCount,
                        DetermineShiftCategory(), passwordTimeCount, negativeKeyOrderCount, _keyOrder.ToArray(),
                        _keyLatency.ToArray(), _keyDownTime.ToArray(), _pairedKeysSpeed.ToArray(), EncryptText(keyDescString));

                    //if (serviceActivated)
                    if (DecryptText(pass).Equals("ALLOW"))
                    {
                        Console.WriteLine("Application closed/User: '" + txtUsername.Text + "' logged in");
                        grbLoggedIn.Visibility = Visibility.Visible;
                        cvsBlur.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ResetStats();
                        activeForTyping = true;
                        Console.WriteLine("ACCESS DENIED");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Error with user log in");
                    Console.WriteLine(ex.ToString());
                }

            }
        }

        private string GetCurrentWallpaper()
        {
            // The current wallpaper path is stored in the registry at HKEY_CURRENT_USER\\Control Panel\\Desktop\\WallPaper
            RegistryKey rkWallPaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            if (rkWallPaper == null)
            {
                return @"C:\Windows\Web\Screen\img100.png";
            }

            var wallpaperPath = rkWallPaper.GetValue("WallPaper").ToString();
            rkWallPaper.Close();
            // Return the current wallpaper path
            return wallpaperPath;
        }


        public string Sha512(string input)
        {
            string hash;

            var data = Encoding.UTF8.GetBytes("text");
            using (SHA512 shaM = new SHA512Managed())
            {
                hash = shaM.ComputeHash(data).ToString();
            }

            return hash;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Date.Day < 10)
            {
                lblDateD.Content = "0" + DateTime.Now.Date.Day;
            }
            else
            {
                lblDateD.Content = DateTime.Now.Date.Day;
            }

            if (DateTime.Now.Date.Month < 10)
            {
                lblDateM.Content = "0" + DateTime.Now.Date.Month;
            }
            else
            {
                lblDateM.Content = DateTime.Now.Date.Month;
            }

            lblDateY.Content = DateTime.Now.Date.Year;

            if (DateTime.Now.Hour < 10)
            {
                lblTimeHour.Content = "0" + DateTime.Now.Hour;
            }
            else
            {
                lblTimeHour.Content = DateTime.Now.Hour;
            }

            if (DateTime.Now.Minute < 10)
            {
                lblTimeMin.Content = "0" + DateTime.Now.Minute;
            }
            else
            {
                lblTimeMin.Content = DateTime.Now.Minute;
            }

            if (DateTime.Now.Second < 10)
            {
                lblTimeSec.Content = "0" + DateTime.Now.Second;
            }
            else
            {
                lblTimeSec.Content = DateTime.Now.Second;
            }

        }
                
        #endregion
        
        #region ButtonMethods

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            grbReport.Visibility = Visibility.Visible;
            LogUserIn();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //private void btnContact_Click(object sender, RoutedEventArgs e)
        //{
        //    grbContactAdmin.Visibility = Visibility.Visible;
        //    cvsBlur.Visibility = Visibility.Visible;
        //    grbInfo.Visibility = Visibility.Hidden;
        //}

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //grbContactAdmin.Visibility = Visibility.Hidden;
            cvsBlur.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;
            grbLoggedIn.Visibility = Visibility.Hidden;
            grbReport.Visibility = Visibility.Hidden;
            //_dispatcherTimer.Tick += dispatcherTimer_Tick;
            //_dispatcherTimer.Start();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cvsBlur.Visibility = Visibility.Hidden;
            //grbContactAdmin.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;
            grbLoggedIn.Visibility = Visibility.Hidden;
            grbReport.Visibility = Visibility.Hidden;
        }

        private void imgInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cvsBlur.Visibility = Visibility.Hidden;
           // grbContactAdmin.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Visible;
            grbLoggedIn.Visibility = Visibility.Hidden;
            grbReport.Visibility = Visibility.Hidden;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            cvsBlur.Visibility = Visibility.Hidden;
            //grbContactAdmin.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;
            grbLoggedIn.Visibility = Visibility.Hidden;
            grbReport.Visibility = Visibility.Hidden;
        }
        
        private void btnCloseLoggedIn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnReportClose_Click(object sender, RoutedEventArgs e)
        {
            grbReport.Visibility = Visibility.Hidden;

            data1A = new List<KeyValuePair<string, int>>();
            data1B = new List<KeyValuePair<string, int>>();
            data2A = new List<KeyValuePair<string, int>>();
            data2B = new List<KeyValuePair<string, int>>();
            data3A = new List<KeyValuePair<string, int>>();
            data3B = new List<KeyValuePair<string, int>>();
            data4A = new List<KeyValuePair<string, int>>();
            data4B = new List<KeyValuePair<string, int>>();
            data5A = new List<KeyValuePair<string, int>>();
            data5B = new List<KeyValuePair<string, int>>();
            data6A = new List<KeyValuePair<string, int>>();
            data6B = new List<KeyValuePair<string, int>>();
            data7A = new List<KeyValuePair<string, int>>();
            data7B = new List<KeyValuePair<string, int>>();

            //series1A = new LineSeries();
            //series1B = new LineSeries();
            //series2A = new LineSeries();
            //series2B = new LineSeries();
            //series3A = new LineSeries();
            //series3B = new LineSeries();

            chtKeyDown.Series.Clear();
            chtKeyDownThreshold.Series.Clear();
            chtKeyLatency.Series.Clear();
            chtKeyLatencyTreshold.Series.Clear();
            chtPairedKey.Series.Clear();
            chtPairedKeyThreshold.Series.Clear();
            chtPasswordSpeedThreshold.Series.Clear();


        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            recordedAlready = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            match = true;
        }


        #endregion

        #region Encryption/Decryption

        private byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 9, 4, 3, 7, 6, 1, 8, 2 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    encrPadding = AES.Padding;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 9, 4, 3, 7, 6, 1, 8, 2 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    AES.Padding = encrPadding;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        private string EncryptText(string input)
        {
            // Get the bytes of the string
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes("JEC11H4Y6SR7AN8YPS1E");

            // Hash the password with SHA256
            passwordBytes = Encoding.UTF8.GetBytes(Sha512(Encoding.UTF8.GetString(passwordBytes)));

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            string result = Convert.ToBase64String(bytesEncrypted);

            return result;
        }

        private string DecryptText(string input)
        {
            // Get the bytes of the string
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes("JEC11H4Y6SR7AN8YPS1E");
            passwordBytes = Encoding.UTF8.GetBytes(Sha512(Encoding.UTF8.GetString(passwordBytes)));

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string result = Encoding.UTF8.GetString(bytesDecrypted);

            return result;
        }

        #endregion

        #region De/Serializing

        public T DeSerializeObject<T>(string serialFile)
        {
            if (string.IsNullOrEmpty(serialFile)) { return default(T); }

            T objectOut = default(T);

            try
            {
                string attributeXml = string.Empty;

                XmlDocument xmlDocument = new XmlDocument();
                string xmlString = serialFile;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while deserializing object");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
            }

            return objectOut;
        }
        
        private string SerializeObject<T>(T serializableObject)
        {
            if (serializableObject == null) { return ""; }

            string outp = "";

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    outp = Encoding.UTF8.GetString(stream.GetBuffer());
                    stream.Close();
                }

                return outp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while serializing object");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
            }

            return "";
        }

        #endregion

    }

}