using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ServiceModel.Security;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media.Imaging;
using BioMex_Admin.BioService;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Drawing.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Emgu.CV.Cuda;
using Emgu.CV.Util;
using Emgu.CV.Features2D;
using System.Xml.Serialization;
using System.Xml;
using Emgu.CV.XFeatures2D;

namespace BioMex_Admin
{
    public partial class MainWindow : Window
    {
        private const int _DISTANCE_THRESHOLD = 4000;
        private const int FACIAL_THRESHOLD = 300;

        #region Variables

        readonly System.Windows.Threading.DispatcherTimer _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private Matrix<byte> observedDesc = new Matrix<byte>(30, 50);
        private bool _faceCaptured = false;
        public KeypointsDescriptors newKeyDescClass = new KeypointsDescriptors();
        private string keyDescString = "";

        private PaddingMode encrPadding = new PaddingMode();
        private bool activeForTyping = true;
        private int keyCount;
        private bool startedTyping;
        private int shiftCategory1; //only left shift is used
        private int shiftCategory2; //only right shift is used
        private int shiftCategory3; //opposite shift is used

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

        private List<int>[] _keyDownArray = new List<int>[6];
        private List<int>[] _keyLatencyArray = new List<int>[6];
        private List<int>[] _keyOrderArray = new List<int>[6];
        private List<int>[] _pairedKeySpeedArray = new List<int>[6];
        private int[] _passwordSpeedArray = new int[6];
        private int _keyCountFinal = 0;

        private Service1Client _service;

        private int _textboxNumber;
        private int _numberPasswordNoMatch;
        private bool _readyToSubmit;
        private bool _passwordFixEnabled;
        private bool _usernameNeeded;

        private bool started = false;
        private bool secondSaved = false;

        private string savedDescriptors = "";
        private string savedKeypoints = "";

        Capture capture;
        private bool _captureInProgress;

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

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
            if (!started)
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(() =>
                {
                    started = true;

                    if (!_faceCaptured)
                    {
                        try
                        {

                            Mat frame = new Mat();
                            capture.Retrieve(frame, 0);

                            Image<Hls, Byte> hlsImage = new Image<Hls, byte>(frame.Bitmap);

                            Image<Gray, Byte> imageL = hlsImage[1];

                            imageL._EqualizeHist();

                            hlsImage[1] = imageL;


                            //HERE

                            List<Rectangle> faces = new List<Rectangle>();

                            if (!frame.IsEmpty)
                                Detect(frame, "haarcascade_frontalface_default.xml", faces, false, true);

                            foreach (Rectangle face in faces)
                                CvInvoke.Rectangle(frame, face, new Bgr(0, 255, 0).MCvScalar, 3);

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

                                //descriptor.Compute(imgROI, observedKeyPoints, observedDesc);
                                surfFeat.Compute(imgROI, observedKeyPoints, observedDesc);
                                //siftFeat.Compute(imgROI, observedKeyPoints, observedDesc);

                                BFMatcher matcher = new BFMatcher(DistanceType.L2);

                                matcher.Add(observedDesc);


                                Mat keypointsFrame = imgROI.ToUMat().ToMat(AccessType.Read);

                                string serialKeys = SerializeObject(observedKeyPoints.ToArray());
                                VectorOfKeyPoint newVec = new VectorOfKeyPoint();
                                newVec.Push(DeSerializeObject<MKeyPoint[]>(serialKeys));


                                Features2DToolbox.DrawKeypoints(keypointsFrame, newVec, keypointsFrame, new Bgr(0, 255, 0), Emgu.CV.Features2D.Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

                                imgFace3.Source = ToBitmapSource(keypointsFrame);

                                newKeyDescClass.keyPoints = observedKeyPoints;
                                newKeyDescClass.descriptors = observedDesc;

                                keyDescString = SerializeObject<KeypointsDescriptors>(newKeyDescClass);

                                if (observedKeyPoints.Size >= FACIAL_THRESHOLD)
                                {
                                    
                                    SubmitHelper();
                                    _faceCaptured = true;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while attempting to process image");
                            Console.WriteLine(ex.Message);
                        }
                    }

                    dispatcherTimer_Tick(sender, arg);
                }

               ));
        }

        private Rectangle CalculateClosestFace(List<Rectangle> lstFace)
        {
            var temp = lstFace[0];

            for (int i = 0; i < lstFace.Count; i++)
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
                    face.ScaleFactor = 1.1;
                    face.MinNeighbors = 10;
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
                //Many opencl functions require opencl compatible gpu devices. 
                //As of opencv 3.0-alpha, opencv will crash if opencl is enable and only opencv compatible cpu device is presented
                //So we need to call CvInvoke.HaveOpenCLCompatibleGpuDevice instead of CvInvoke.HaveOpenCL (which also returns true on a system that only have cpu opencl devices).
                CvInvoke.UseOpenCL = tryUseOpenCL && CvInvoke.HaveOpenCLCompatibleGpuDevice;


                //Read the HaarCascade objects
                using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                {
                    using (UMat ugray = new UMat())
                    {
                        //if(!ugray.IsEmpty)
                        CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                        //normalizes brightness and increases contrast of the image
                        CvInvoke.EqualizeHist(ugray, ugray);

                        //Detect the faces  from the gray scale image and store the locations as rectangle
                        //The first dimensional is the channel
                        //The second dimension is the index of the rectangle in the specific channel
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

        private int CalculateTotalTime(DateTime first, DateTime second)
        {
            var millisecs = (first - second).Milliseconds;
            var secs = (first - second).Seconds;
            var mins = (first - second).Minutes;

            int total = millisecs + (secs * 1000) + (mins * 60000);

            return total;
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


        private int DetermineDistance(int[] point1, int[] point2)
        {
            var total = 0;
            var dist = 0;
            var count = 0;
                
            if (point1.Length <point2.Length)
                count = point1.Length;
            else
                count = point2.Length;

                 for (int i = 0; i < count; i++)
                {
                  
                        dist = point1[i] - point2[i];

                        if (dist < 0)
                            dist = dist * -1;

                        total += dist;
                 }
            
            return total;
        }

        private int DetermineDistance(int point1, int point2)
        {
            var dist = 0;

            dist = point1 - point2;

            if (dist < 0)
                 dist = dist * -1;

            return dist;
        }

        private bool RunDistanceCheck(int number)
        {
            var totalDist = 0;

             totalDist += DetermineDistance(_keyDownArray[0].ToArray(), _keyDownArray[number-1].ToArray());
            totalDist += DetermineDistance(_keyLatencyArray[0].ToArray(), _keyLatencyArray[number-1].ToArray());
            totalDist += DetermineDistance(_pairedKeySpeedArray[0].ToArray(), _pairedKeySpeedArray[number-1].ToArray());
            totalDist += DetermineDistance(_keyOrderArray[0].ToArray(), _keyOrderArray[number-1].ToArray());
            totalDist += DetermineDistance(_passwordSpeedArray[0], _passwordSpeedArray[number-1]);

            if(totalDist <= _DISTANCE_THRESHOLD)
            {
                return true;
            }

            return false;

        }

        private bool RunPreDistanceCheck()
        {
            var totalDist = 0;

            totalDist += DetermineDistance(_keyDownArray[0].ToArray(), _keyDownTime.ToArray());
            totalDist += DetermineDistance(_keyLatencyArray[0].ToArray(), _keyLatency.ToArray());
            totalDist += DetermineDistance(_pairedKeySpeedArray[0].ToArray(), _pairedKeysSpeed.ToArray());
            totalDist += DetermineDistance(_keyOrderArray[0].ToArray(), _keyOrder.ToArray());
            totalDist += DetermineDistance(_passwordSpeedArray[0], passwordTimeCount);

            if (totalDist <= _DISTANCE_THRESHOLD)
            {
                return true;
            }

            return false;

        }
        
        private void tb_KeyUp(object sender, KeyEventArgs e)
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
                    passwordTimer = DateTime.Now; //used to keep track of speed of entire password
                }
                startedTyping = true;

            } //FIX STOPPED

            if (e.Key == Key.Enter)
            {
                if (activeForTyping)
                {
                    passwordTimeCount = CalculateTotalTime(DateTime.Now, passwordTimer);
                    DetermineShiftCategory();

                    if (_passwordFixEnabled) //FOLLOW THIS FUNCTION TO RECTIFY THE PASSWORD BOXES NOT DISABLING AFTER FIX
                    {
                        if (_numberPasswordNoMatch == 2)
                        {
                            pwb2.BorderBrush = Brushes.Gray;
                            pwb2.IsEnabled = false;
                            _passwordFixEnabled = false;
                        }
                        if (_numberPasswordNoMatch == 3)
                        {
                            pwb3.BorderBrush = Brushes.Gray;
                            pwb3.IsEnabled = false;
                            _passwordFixEnabled = false;
                        }
                        if (_numberPasswordNoMatch == 4)
                        {
                            pwb4.BorderBrush = Brushes.Gray;
                            pwb4.IsEnabled = false;
                            _passwordFixEnabled = false;
                        }
                        if (_numberPasswordNoMatch == 5)
                        {
                            pwb5.BorderBrush = Brushes.Gray;
                            pwb5.IsEnabled = false;
                            _passwordFixEnabled = false;
                        }
                        if (_numberPasswordNoMatch == 6)
                        {
                            pwb6.BorderBrush = Brushes.Gray;
                            pwb6.IsEnabled = false;
                            _passwordFixEnabled = false;
                        }
                    }

                    //Console.WriteLine(keyCount);

                    //for (int i = 0; i < _keyDownTime.Count; i++)
                    //    Console.WriteLine(_keyDownTime[i]);
                    //Console.WriteLine(@"break");

                    //for (int i = 0; i < _keyLatency.Count; i++)
                    //    Console.WriteLine(_keyLatency[i]);
                    //Console.WriteLine(@"break");

                    //for (int i = 0; i < _keyOrder.Count; i++)
                    //    Console.WriteLine(_keyOrder[i]);
                    //Console.WriteLine(@"break");

                    //for (int i = 0; i < _pairedKeysSpeed.Count; i++)
                    //    Console.WriteLine(_pairedKeysSpeed[i]);
                    //Console.WriteLine(@"break");

                    //Console.WriteLine(passwordTimeCount);
                    //Console.WriteLine(@"break");
                    //Console.WriteLine(DetermineShiftCategory());

                    //if (_textboxNumber == 0)
                    //{
                    //    SaveandResetStats(1);
                    //}
                    //if (_textboxNumber == 1)
                    //{
                    //    SaveandResetStats(2);
                    //}
                    //if (_textboxNumber == 2)
                    //{
                    //    SaveandResetStats(3);
                    //}
                    //if (_textboxNumber == 3)
                    //{
                    //    SaveandResetStats(4);
                    //}
                    //if (_textboxNumber == 4)
                    //{
                    //    SaveandResetStats(5);
                    //}
                    //if (_textboxNumber == 5)
                    //{
                    //    SaveandResetStats(6);
                    //}
                    



                    if (_textboxNumber == 0)
                    {
                        if (pwb1.Password.Length >= 10)
                        {
                            SaveandResetStats(0);
                            _textboxNumber = 1;
                            pwb1.BorderBrush = Brushes.Gray;
                            pwb1.IsEnabled = false;
                            pwb2.IsEnabled = true;
                            pwb2.Focus();

                            pwb1.ToolTip = "Enter Password";
                            pwb2.ToolTip = "Enter Password";
                            pwb3.ToolTip = "Enter Password";
                            pwb4.ToolTip = "Enter Password";
                            pwb5.ToolTip = "Enter Password";
                            pwb6.ToolTip = "Enter Password";                            
                        }
                        else
                        {
                            pwb1.BorderBrush = Brushes.Red;
                            this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                            pwb1.ToolTip = "Password must be 10 or more characters";
                        }

                    }
                    else
                        if (_textboxNumber == 1 && RunPreDistanceCheck())
                        {
                            _textboxNumber = 2;
                            pwb2.IsEnabled = false;
                            pwb2.BorderBrush = Brushes.Gray;
                            pwb3.IsEnabled = true;
                            pwb3.Focus();
                            SaveandResetStats(1);
                        }
                        else
                            if (_textboxNumber == 2 && RunPreDistanceCheck())
                            {
                                _textboxNumber = 3;
                                pwb3.IsEnabled = false;
                                pwb3.BorderBrush = Brushes.Gray;
                                pwb4.IsEnabled = true;
                                pwb4.Focus();
                                SaveandResetStats(2);
                            }
                            else
                                if (_textboxNumber == 3 && RunPreDistanceCheck())
                                {
                                    _textboxNumber = 4;
                                    pwb4.IsEnabled = false;
                                    pwb4.BorderBrush = Brushes.Gray;
                                    pwb5.IsEnabled = true;
                                    pwb5.Focus();
                                    SaveandResetStats(3);
                                }
                                else
                                    if (_textboxNumber == 4 && RunPreDistanceCheck())
                                    {
                                        _textboxNumber = 5;
                                        pwb5.IsEnabled = false;
                                        pwb5.BorderBrush = Brushes.Gray;
                                        pwb6.IsEnabled = true;
                                        pwb6.Focus();
                                        SaveandResetStats(4);
                                    }
                                    else
                                        if (_textboxNumber == 5 && RunPreDistanceCheck())
                                        {
                                            _textboxNumber = 6;
                                            pwb6.IsEnabled = false;
                                            pwb6.BorderBrush = Brushes.Gray;
                                            SaveandResetStats(5);                                            
                                        }



                        if (_textboxNumber == 2)
                        {
                            if (!RunDistanceCheck(2))
                            {
                                pwb2.BorderBrush = Brushes.Red;
                                pwb2.ToolTip = "Attempt not efficiently matched to first attempt";
                                ResetStats();
                                activeForTyping = true;
                            }
                        }
                        if (_textboxNumber ==3)
                        {
                            if (!RunDistanceCheck(3))
                            {
                                pwb3.BorderBrush = Brushes.Red;
                                pwb3.ToolTip = "Attempt not efficiently matched to first attempt";
                                ResetStats();
                                activeForTyping = true;
                            }
                        }
                        if (_textboxNumber == 4)
                        {
                            if (!RunDistanceCheck(4))
                            {
                                pwb4.BorderBrush = Brushes.Red;
                                pwb4.ToolTip = "Attempt not efficiently matched to first attempt";
                                ResetStats();
                                activeForTyping = true;
                            }
                        }
                        if (_textboxNumber == 5)
                        {
                            if (!RunDistanceCheck(5))
                            {
                                pwb5.BorderBrush = Brushes.Red;
                                pwb5.ToolTip = "Attempt not efficiently matched to first attempt";
                                ResetStats();
                                activeForTyping = true;
                            }
                        }
                        if (_textboxNumber == 6)
                        {
                            if (!RunDistanceCheck(6))
                            {
                                pwb6.BorderBrush = Brushes.Red;
                                pwb6.ToolTip = "Attempt not efficiently matched to first attempt";
                                ResetStats();
                                activeForTyping = true;
                            }
                        }





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

                if (_textboxNumber == 0)
                {
                    pwb1.Password = "";
                }
                if (_textboxNumber == 1)
                {
                    pwb2.Password = "";
                }
                if (_textboxNumber == 2)
                {
                    pwb3.Password = "";
                }
                if (_textboxNumber == 3)
                {
                    pwb4.Password = "";
                }
                if (_textboxNumber == 4)
                {
                    pwb5.Password = "";
                }
                if (_textboxNumber == 5)
                {
                    pwb6.Password = "";
                }

                ResetStats();
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
            shiftPressed = false;
            keyCurrentlyPressed = false;
            negativeKeyOrderCount = 0;
            leftShiftCount = 0;
            rightShiftCount = 0;

            _keyDownTime.Clear();
            _keyLatency.Clear();
            _keyOrder.Clear();
            _pairedKeysSpeed.Clear();

            _keyDownTime = new List<int>();
            _keyLatency = new List<int>();
            _keyOrder = new List<int>();
            _pairedKeysSpeed = new List<int>();

            keyDownTimeCount = 0;
            keyLatencyCount = 0;
            keyCount = 0;
            passwordTimeCount = 0;
            passwordTimer = DateTime.Now;
            keyCurrentlyPressed = false;
            startedTyping = false;
            activeForTyping = true;

        }

        private void SaveandResetStats(int numberToBeLogged) //Used to save the data in respective allocated places and reset metrics
        {
            if(numberToBeLogged == 0)
            {
                Console.WriteLine(keyCount);

                for (int i = 0; i < _keyDownTime.Count; i++)
                    Console.WriteLine(_keyDownTime[i]);
                Console.WriteLine(@"break");

                for (int i = 0; i < _keyLatency.Count; i++)
                    Console.WriteLine(_keyLatency[i]);
                Console.WriteLine(@"break");

                for (int i = 0; i < _keyOrder.Count; i++)
                    Console.WriteLine(_keyOrder[i]);
                Console.WriteLine(@"break");

                for (int i = 0; i < _pairedKeysSpeed.Count; i++)
                    Console.WriteLine(_pairedKeysSpeed[i]);
                Console.WriteLine(@"break");

                Console.WriteLine(passwordTimeCount);
                Console.WriteLine(@"break");
                Console.WriteLine(DetermineShiftCategory());
            }

            _keyDownArray[numberToBeLogged] = _keyDownTime;
            _keyLatencyArray[numberToBeLogged] = _keyLatency;
            _keyOrderArray[numberToBeLogged] = _keyOrder;
            _pairedKeySpeedArray[numberToBeLogged] = _pairedKeysSpeed;
            _passwordSpeedArray[numberToBeLogged] = passwordTimeCount;
            _keyCountFinal = keyCount;

            if (numberToBeLogged == 5)
            {
                if (txtName.Text != "" && txtSurname.Text != "" && txtUsername.Text != "")
                    _readyToSubmit = true;
            }

            shiftPressed = false;
            keyCurrentlyPressed = false;
            negativeKeyOrderCount = 0;

            _keyDownTime = new List<int>();
            _keyLatency = new List<int>();
            _keyOrder = new List<int>();
            _pairedKeysSpeed = new List<int>();

            keyDownTimeCount = 0;
            keyLatencyCount = 0;
            keyCount = 0;
            passwordTimeCount = 0;
            passwordTimer = DateTime.Now;
            keyCurrentlyPressed = false;
            activeForTyping = true;
            startedTyping = false;
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
        
        #endregion

        #region FormMethods

        public MainWindow()
        {
            InitializeComponent();
            _dispatcherTimer.Tick += dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
            grbMain.Visibility = Visibility.Visible;
            grbRegister.Visibility = Visibility.Hidden;
            grbSuccess.Visibility = Visibility.Hidden;
            grbFaceRec.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Hidden;
            imgBack.Visibility = Visibility.Hidden;
            imgPrint.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;

            _textboxNumber = 0;
            
            //CvInvoke.UseOpenCL = false;

            //try
            //{
            //    capture = new Capture(0);
            //    capture.ImageGrabbed += ProcessFrame;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error while attempting to create new capture");
            //    Console.WriteLine(ex.InnerException);
            //}


            //Add the key up and down events to each of the password boxes
            //Keep count of how many is filled in (boolean and counter)


            keyCount = 0;
            leftShiftCount = 0;
            rightShiftCount = 0;
            pwb1.KeyUp += tb_KeyUp;
            pwb1.KeyDown += tb_KeyDown;
            pwb2.KeyUp += tb_KeyUp;
            pwb2.KeyDown += tb_KeyDown;
            pwb3.KeyUp += tb_KeyUp;
            pwb3.KeyDown += tb_KeyDown;
            pwb4.KeyUp += tb_KeyUp;
            pwb4.KeyDown += tb_KeyDown;
            pwb5.KeyUp += tb_KeyUp;
            pwb5.KeyDown += tb_KeyDown;
            pwb6.KeyUp += tb_KeyUp;
            pwb6.KeyDown += tb_KeyDown;
            shiftPressed = false;
            keyCurrentlyPressed = false;
            negativeKeyOrderCount = 0;

            _keyDownTime = new List<int>();
            _keyLatency = new List<int>();
            _keyOrder = new List<int>();
            _pairedKeysSpeed = new List<int>();

            _service = new Service1Client();
            _readyToSubmit = false;
            _passwordFixEnabled = false;
            _usernameNeeded = false;

            EncryptText("");
        }

        private void frmBioMexAdmin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (capture != null)
                capture.Dispose();
        }

        #endregion

        #region HelperMethods

        private void SubmitHelper()
        {
            try
            {
                _service.RegisterNewUser(txtName.Text, txtSurname.Text, EncryptText(txtUsername.Text), CalculateAge(), Sha512(pwb1.Password), _keyCountFinal, DetermineShiftCategory(), _passwordSpeedArray.ToArray(), _keyDownArray.Select(a => a.ToArray()).ToArray(), _keyLatencyArray.Select(a => a.ToArray()).ToArray(), _pairedKeySpeedArray.Select(a => a.ToArray()).ToArray(), _keyOrderArray.Select(a => a.ToArray()).ToArray(), EncryptText(keyDescString));
                //_service.RegisterNewUser(txtName.Text, txtSurname.Text, EncryptText(txtUsername.Text), CalculateAge(), Sha512(pwb1.Password), _keyCountFinal, DetermineShiftCategory(), _passwordSpeedArray.ToArray(), _keyDownArray.Select(a => a.ToArray()).ToArray(), _keyLatencyArray.Select(a => a.ToArray()).ToArray(), _pairedKeySpeedArray.Select(a => a.ToArray()).ToArray(), _keyOrderArray.Select(a => a.ToArray()).ToArray(), "","");

                grbMain.Visibility = Visibility.Hidden;
                grbRegister.Visibility = Visibility.Hidden;
                btnBack.Visibility = Visibility.Hidden;
                imgBack.Visibility = Visibility.Hidden;
                imgPrint.Visibility = Visibility.Hidden;
                grbFaceRec.Visibility = Visibility.Hidden;
                grbInfo.Visibility = Visibility.Hidden;
                grbSuccess.Visibility = Visibility.Visible;

                //if (capture != null)
                //    capture.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while attempting to register new user");
                Console.WriteLine(ex.InnerException);
            }
        }
        private bool CheckPasswords()
        {
            if (pwb1.Password.Equals(pwb2.Password))
            {
                if (pwb2.Password.Equals(pwb3.Password))
                {
                    if (pwb3.Password.Equals(pwb4.Password))
                    {
                        if (pwb4.Password.Equals(pwb5.Password))
                        {
                            if (pwb5.Password.Equals(pwb6.Password))
                            {
                                return true;
                            }
                            else
                            {
                                _numberPasswordNoMatch = 6;
                            }
                        }
                        else
                        {
                            _numberPasswordNoMatch = 5;
                        }
                    }
                    else
                    {
                        _numberPasswordNoMatch = 4;
                    }
                }
                else
                {
                    _numberPasswordNoMatch = 3;
                }
            }
            else
            {
                _numberPasswordNoMatch = 2;
            }


            return false;
        }

        private bool CheckUsernameTaken(string user)
        {
            var username = false;
            try
            {
                if (!_service.CheckUsernameAvailability(user))
                {
                    _usernameNeeded = true;
                    username = true;
                }
                else
                    _usernameNeeded = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error attempting to find username");
                Console.WriteLine(ex.InnerException);
            }

            return _usernameNeeded;
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

        private int CalculateAge()
        {
            return (DateTime.Now.Year - dtpDOB.SelectedDate.Value.Year);
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

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            grbMain.Visibility = Visibility.Visible;
            grbRegister.Visibility = Visibility.Hidden;
            grbSuccess.Visibility = Visibility.Hidden;
            grbFaceRec.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Hidden;
            imgBack.Visibility = Visibility.Hidden;
            imgPrint.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;

            txtName.Text = "Name...";
            txtSurname.Text = "Surname...";
            txtUsername.Text = "Username...";
            pwb1.Password = ".....";
            pwb2.Password = ".....";
            pwb3.Password = ".....";
            pwb4.Password = ".....";
            pwb5.Password = ".....";
            pwb6.Password = ".....";
            pwb1.IsEnabled = true;
            _textboxNumber = 0;
            activeForTyping = true;
            ResetStats();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            grbMain.Visibility = Visibility.Hidden;
            grbRegister.Visibility = Visibility.Visible;
            grbSuccess.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Visible;
            imgBack.Visibility = Visibility.Visible;
            imgPrint.Visibility = Visibility.Visible;
            grbFaceRec.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;

            pwb1.BorderBrush = Brushes.Gray;
            pwb2.BorderBrush = Brushes.Gray;
            pwb3.BorderBrush = Brushes.Gray;
            pwb4.BorderBrush = Brushes.Gray;
            pwb5.BorderBrush = Brushes.Gray;
            pwb6.BorderBrush = Brushes.Gray;

            pwb1.IsEnabled = true;
            pwb2.IsEnabled = false;
            pwb3.IsEnabled = false;
            pwb4.IsEnabled = false;
            pwb5.IsEnabled = false;
            pwb6.IsEnabled = false;

            pwb1.ToolTip = "Enter Password";
            pwb2.ToolTip = "Enter Password";
            pwb3.ToolTip = "Enter Password";
            pwb4.ToolTip = "Enter Password";
            pwb5.ToolTip = "Enter Password";
            pwb6.ToolTip = "Enter Password";
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //use this button to submit both keystroke and facial metrics to service

            SubmitHelper();

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            grbMain.Visibility = Visibility.Visible;
            grbRegister.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Hidden;
            grbSuccess.Visibility = Visibility.Hidden;
            imgBack.Visibility = Visibility.Hidden;
            imgPrint.Visibility = Visibility.Hidden;
            grbFaceRec.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Hidden;

            txtName.Text = "Name...";
            txtSurname.Text = "Surname...";
            txtUsername.Text = "Username...";
            pwb1.Password = ".....";
            pwb2.Password = ".....";
            pwb3.Password = ".....";
            pwb4.Password = ".....";
            pwb5.Password = ".....";
            pwb6.Password = ".....";
            pwb1.IsEnabled = true;
            _textboxNumber = 0;
            activeForTyping = true;
            ResetStats();
        }

        private void btnRegister_MouseEnter(object sender, MouseEventArgs e)
        {
            img1.Opacity = 50;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            pwb1.BorderBrush = Brushes.Gray;
            pwb2.BorderBrush = Brushes.Gray;
            pwb3.BorderBrush = Brushes.Gray;
            pwb4.BorderBrush = Brushes.Gray;
            pwb5.BorderBrush = Brushes.Gray;
            pwb6.BorderBrush = Brushes.Gray;
            //pwb1.IsEnabled = false;
            //pwb2.IsEnabled = false;
            //pwb3.IsEnabled = false;
            //pwb4.IsEnabled = false;
            //pwb5.IsEnabled = false;
            //pwb6.IsEnabled = false;

            if (_readyToSubmit)
            {
                //if(CheckPasswords() && !CheckUsernameTaken(txtUsername.Text))
                if (CheckPasswords())
                {
                    //REMEMBER TO SET USER DETAILS
                    //_service.RegisterNewUser(txtName.Text, txtSurname.Text,CalculateAge(),_user);

                    if (!_usernameNeeded)
                    {
                        try
                        {
                            capture = new Capture(0);
                            capture.ImageGrabbed += ProcessFrame;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while attempting to create new capture");
                            Console.WriteLine(ex.InnerException);
                        }

                        grbFaceRec.Visibility = Visibility.Visible;
                        grbMain.Visibility = Visibility.Hidden;
                        grbRegister.Visibility = Visibility.Hidden;
                        btnBack.Visibility = Visibility.Visible;
                        imgBack.Visibility = Visibility.Visible;
                        imgPrint.Visibility = Visibility.Visible;
                        grbSuccess.Visibility = Visibility.Hidden;
                        grbInfo.Visibility = Visibility.Hidden;

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
                }
                else
                {
                    activeForTyping = false;

                    if (_numberPasswordNoMatch == 2)
                    {
                        pwb2.BorderBrush = Brushes.Red;
                        pwb2.IsEnabled = true;
                        _passwordFixEnabled = true;
                        _textboxNumber = 2;
                        pwb2.ToolTip = "Password does not match original";
                    }
                    if (_numberPasswordNoMatch == 3)
                    {
                        pwb3.BorderBrush = Brushes.Red;
                        pwb3.IsEnabled = true;
                        _passwordFixEnabled = true;
                        _textboxNumber = 3;
                        pwb3.ToolTip = "Password does not match original";
                    }
                    if (_numberPasswordNoMatch == 4)
                    {
                        pwb4.BorderBrush = Brushes.Red;
                        pwb4.IsEnabled = true;
                        _passwordFixEnabled = true;
                        _textboxNumber = 4;
                        pwb4.ToolTip = "Password does not match original";
                    }
                    if (_numberPasswordNoMatch == 5)
                    {
                        pwb5.BorderBrush = Brushes.Red;
                        pwb5.IsEnabled = true;
                        _passwordFixEnabled = true;
                        _textboxNumber = 5;
                        pwb5.ToolTip = "Password does not match original";
                    }
                    if (_numberPasswordNoMatch == 6)
                    {
                        pwb6.BorderBrush = Brushes.Red;
                        pwb6.IsEnabled = true;
                        _passwordFixEnabled = true;
                        _textboxNumber = 6;
                        pwb6.ToolTip = "Password does not match original";
                    }
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            grbMain.Visibility = Visibility.Hidden;
            grbRegister.Visibility = Visibility.Visible;
            grbSuccess.Visibility = Visibility.Hidden;
            //grbMessages.Visibility = Visibility.Hidden;
            grbFaceRec.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Visible;
            imgBack.Visibility = Visibility.Visible;
            imgPrint.Visibility = Visibility.Visible;
            grbInfo.Visibility = Visibility.Hidden;
        }

        private void imgInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            grbMain.Visibility = Visibility.Hidden;
            grbRegister.Visibility = Visibility.Hidden;
            //grbMessages.Visibility = Visibility.Hidden;
            grbFaceRec.Visibility = Visibility.Hidden;
            grbSuccess.Visibility = Visibility.Hidden;
            btnBack.Visibility = Visibility.Hidden;
            imgBack.Visibility = Visibility.Hidden;
            imgPrint.Visibility = Visibility.Hidden;
            grbInfo.Visibility = Visibility.Visible;
        }

        #endregion

        #region ValueChanged

        private void txtName_GotFocus(object sender, RoutedEventArgs e)
        {
            txtName.Text = "";
        }

        private void txtSurname_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSurname.Text = "";
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            txtUsername.Text = "";
        }

        private void pwb1_GotFocus(object sender, RoutedEventArgs e)
        {
            pwb1.Password = "";
        }

        private void pwb2_GotFocus(object sender, RoutedEventArgs e)
        {
            pwb2.Password = "";
        }

        private void pwb3_GotFocus(object sender, RoutedEventArgs e)
        {
            pwb3.Password = "";
        }

        private void pwb4_GotFocus(object sender, RoutedEventArgs e)
        {
            pwb4.Password = "";
        }

        private void pwb5_GotFocus(object sender, RoutedEventArgs e)
        {
            pwb5.Password = "";
        }

        private void pwb6_GotFocus(object sender, RoutedEventArgs e)
        {
            pwb6.Password = "";
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CheckUsernameTaken(txtUsername.Text))
            {
                txtUsername.BorderBrush = Brushes.Red;
                txtUsername.ToolTip = "Username already taken";
            }
            else
            {
                txtUsername.BorderBrush = Brushes.Gray;
            }
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
