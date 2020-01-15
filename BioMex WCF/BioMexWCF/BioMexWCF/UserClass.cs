using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BioMex_First
{
    public class UserClass
    {

        private string _username;
        private string _password;
        private int _passwordCount;
        private int _passwordKeySpeed;
        private int _shiftClassification;
        private int _negativeKeyOrder;
        private int[] _keyDownTime;
        private int[] _keyLatency;
        private int[] _keyOrder;
        private int[] _pairedKeysSpeed;


        public void SetUsername(string value)
        {
            _username = value;
        }

        public string GetUsername()
        {
            return _username;
        }

         public void SetPassword(string value)
        {
            _password = value;
        }

        public string GetPassword()
        {
            return _password;
        }

        public void SetPasswordKeySpeed(int value)
        {
            _passwordKeySpeed = value;
        }

        public int GetPasswordKeySpeed()
        {
            return _passwordKeySpeed;
        }

        public void SetPasswordCount(int value)
        {
            _passwordCount = value;
        }

        public int GetPasswordCount()
        {
            return _passwordCount;
        }

        public void SetNegativeKeyOrder(int value)
        {
            _negativeKeyOrder = value;
        }

        public int GetNegativeKeyOrder()
        {
            return _negativeKeyOrder;
        }

        public void SetShiftClassification(int value)
        {
            _shiftClassification = value;
        }

        public int GetShiftClassification()
        {
            return _shiftClassification;
        }

        public void SetKeyDownTime(int[] value)
        {
            _keyDownTime = value;
        }

        public int[] GetKeyDownTime()
        {
            return _keyDownTime;
        }

        public void SetKeyLatency(int[] value)
        {
            _keyLatency = value;
        }

        public int[] GetKeyLatency()
        {
            return _keyLatency;
        }

        public void SetKeyOder(int[] value)
        {
            _keyOrder = value;
        }

        public int[] GetKeyOrder()
        {
            return _keyOrder;
        }

        public void SetPairedKeysSpeed(int[] value)
        {
            _pairedKeysSpeed = value;
        }

        public int[] GetPairedKeysSpeed()
        {
            return _pairedKeysSpeed;
        }

        public UserClass()
        {
            
        }

        public UserClass(string user, string pass, int count, int shiftclass, int passwordSpeed, int negOrder, int[] keyorder, int[] keylatency, int[] keydowntime, int[] pairedkeys)
        {
            _username = user;
            _password = pass;
            _pairedKeysSpeed = pairedkeys;
            _keyOrder = keyorder;
            _keyDownTime = keydowntime;
            _keyLatency = keylatency;
            _passwordCount = count;
            _shiftClassification = shiftclass;
            _passwordKeySpeed = passwordSpeed;
            _negativeKeyOrder = negOrder;
        }
        
        public void SetArrays(int count, int shiftClass)
        {
            _passwordCount = count;
            _keyDownTime = new int[_passwordCount];
            _pairedKeysSpeed = new int[(_passwordCount/2) +1];
            _keyLatency = new int[_passwordCount];
            _keyOrder = new int[(_passwordCount / 2) +1];
            _shiftClassification = shiftClass;
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

    }
}
