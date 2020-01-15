using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;

namespace BioMex
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartFacialRecognition()
        {
            var cascadeClassifier = new CascadeClassifier("J:/Develop/BioMex/BioMex/BioMex/HaarCascade/haarcascade_frontalface_default.xml");
            var capture = new VideoCapture();
            var faceRecognizer = new EigenFaceRecognizer();
        }
    }
}
