using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BioMexWCF
{
    public class KeyPressedTime
    {
        private int[] keyArray;


        public void SetKeyArray(int number)
        {
            keyArray = new int[number];
        }
    }
}