using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioMex_Admin
{
    public class KeypointsDescriptors
    {
        public Matrix<Byte> descriptors = null;
        public VectorOfKeyPoint keyPoints = new VectorOfKeyPoint();

        public KeypointsDescriptors(Matrix<Byte> desc, VectorOfKeyPoint keys)
        {
            descriptors = desc;
            keyPoints = keys;
        }
        public KeypointsDescriptors(){ }
    }
}
