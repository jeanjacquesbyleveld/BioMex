using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using BioMex_First;
using System.Security.Cryptography;
using Image = System.Drawing.Image;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BioMex_Admin;

namespace BioMexWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        private const int _SPEED_DISTANCE_THRESHOLD = 300;
        private const int _KEYDOWN_DISTANCE_THRESHOLD = 400;
        private const int _KEYLATENCY_DISTANCE_THRESHOLD = 500;
        private const int _PAIREDKEY_DISTANCE_THRESHOLD = 300;

        private PaddingMode encrPadding = new PaddingMode();

        public bool ActivateService()
        {
            return true;
            EncryptText("");
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

        public string SerializeIntegers(List<int> intList)
        {
            var output = "(";

            for(int i = 0; i< intList.Count;i++)
            {
                output += intList[i] + ",";
            }

            output += ")";

            return output;
        }

        private IdealFeature GetIdealFeature(List<int> passwordSpeed, List<List<int>> keyDowns, List<List<int>> keyLatencies, List<List<int>> pairedKeys, List<List<int>> keyOrder)
        {
            var newFeat = new IdealFeature();
            var avgPwSpeed = 0;
            var keyDownAvg = new List<int>();
            var keyLatenciesAvg = new List<int>();
            var pairedKeyAvg = new List<int>();
            var keyOrderAvg = new List<int>();

            using (var db = new BioMexDatabaseEntities1())
            {
                try
                {
                    for (int i = 0; i < passwordSpeed.Count;i++)
                    {
                        avgPwSpeed += passwordSpeed[i];
                    }
                    avgPwSpeed = avgPwSpeed / passwordSpeed.Count;

                    for (int i = 0; i < keyDowns.Count; i++)
                    {
                        for (int k = 0; k < keyDowns[i].Count; k++)
                        {
                            //INITIAL VALUE MUST BE PLACED (ADDED) BEFORE ADDITION CAN OCCUR    <<=== FIX!!!
                            if (i==0)
                                keyDownAvg.Add(keyDowns[i][k]);
                            else
                                keyDownAvg[k] += keyDowns[i][k];
                        }
                    }

                    for (int i = 0; i < keyDownAvg.Count; i++)
                    {
                        keyDownAvg[i] = keyDownAvg[i] / 5;
                    }

                    for (int i = 0; i < keyLatencies.Count; i++)
                    {
                        for (int k = 0; k < keyLatencies[i].Count; k++)
                        {
                            if (i == 0)
                                keyLatenciesAvg.Add(keyLatencies[i][k]);
                            else
                                keyLatenciesAvg[k] += keyLatencies[i][k];
                        }
                    }

                    for (int i = 0; i < keyLatenciesAvg.Count; i++)
                    {
                        keyLatenciesAvg[i] = keyLatenciesAvg[i] / 5;
                    }

                    for (int i = 0; i < pairedKeys.Count; i++)
                    {
                        for (int k = 0; k < pairedKeys[i].Count; k++)
                        {
                            if (i == 0)
                                pairedKeyAvg.Add(pairedKeys[i][k]);
                            else
                                pairedKeyAvg[k] += pairedKeys[i][k];
                        }
                    }

                    for (int i = 0; i < pairedKeyAvg.Count; i++)
                    {
                        pairedKeyAvg[i] = pairedKeyAvg[i] / 5;
                    }

                    for (int i = 0; i < keyOrder.Count; i++)
                    {
                        for (int k = 0; k < keyOrder[i].Count; k++)
                        {
                            if (i == 0)
                                keyOrderAvg.Add(keyOrder[i][k]);
                            else
                                keyOrderAvg[k] += keyOrder[i][k];
                        }
                    }

                    for (int i = 0; i < keyOrderAvg.Count; i++)
                    {
                        keyOrderAvg[i] = keyOrderAvg[i] / 5;
                    }
                    
                    newFeat = new IdealFeature
                    {
                        IF_KEY_LATENCIES = SerializeIntegers(keyLatenciesAvg),
                        IF_KEY_ORDER = SerializeIntegers(keyOrderAvg),
                        IF_PAIRED_KEYS = SerializeIntegers(pairedKeyAvg),
                        IF_TYPING_SPEED = avgPwSpeed,
                        IF_KEY_PRESS_DURATION = SerializeIntegers(keyDownAvg)
                    };
                }
                catch(Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine("Exception caught while calculating the ideal feature");
                    System.Diagnostics.Debug.WriteLine(ex.InnerException);
                }
            }

            return newFeat;
        }

        public void RegisterNewUser(string name, string surname, string username, int age, string password, int passwordCount, int shiftClassification, List<int> passwordSpeed, List<List<int>> keyDowns, List<List<int>> keyLatencies, List<List<int>> pairedKeys, List<List<int>> keyOrder, string keyDescClass)
        {
            EncryptText("");
            using (var db = new BioMexDatabaseEntities1())
            {
                try
                {
                    var newUser = new User
                    {
                        US_NAME = name,
                        US_SURNAME = surname,
                        US_PASSWORD = password,
                        US_USERNAME = DecryptText(username),
                        US_AGE = age,
                        US_LAST_LOGGED_IN = DateTime.Now,
                        US_UP_TO_DATE = true,
                        US_IS_ADMIN = false,
                    };

                    for(int i = 0; i<5;i++)
                    {
                        var newFeat = new Feature
                        {
                            FE_PASSWORD_COUNT = passwordCount,
                            FE_SHIFT_CLASS = shiftClassification,
                            FE_TIME_TAKEN = DateTime.Now,
                            FE_TYPING_SPEED = passwordSpeed[i],
                            FE_KEY_PRESS_DURATION = SerializeIntegers(keyDowns[i]),
                            FE_KEY_LATENCIES = SerializeIntegers(keyLatencies[i]),
                            FE_KEY_ORDER = SerializeIntegers(keyOrder[i]),
                            FE_PAIRED_KEYS = SerializeIntegers(pairedKeys[i]),
                        };

                        newUser.Features.Add(newFeat);
                    }

                    var idealFeat = GetIdealFeature(passwordSpeed, keyDowns, keyLatencies, pairedKeys, keyOrder);

                    idealFeat.IF_PASSWORD_COUNT = passwordCount;
                    idealFeat.IF_SHIFT_CLASS = shiftClassification;
                    idealFeat.IF_TIME_TAKEN = DateTime.Now;
                    
                    newUser.IdealFeatures.Add(idealFeat);

                    FaceFeature newFaceFeat = new FaceFeature();

                    //newFaceFeat.FF_DESCRIPTORS = DecryptText(descriptors);
                    //newFaceFeat.FF_KEY_POINTS = DecryptText(keypoints);

                    KeypointsDescriptors builtUpClass = DeSerializeObject<KeypointsDescriptors>(DecryptText(keyDescClass));

                    newFaceFeat.FF_DESCRIPTORS = SerializeObject(builtUpClass.descriptors.ToString());
                    newFaceFeat.FF_KEY_POINTS = SerializeObject(builtUpClass.keyPoints.ToArray());

                    newUser.Features.FirstOrDefault().FaceFeature = newFaceFeat;


                    
                    db.Users.Add(newUser);
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("New user successfully added");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception caught while registering new user");
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        public bool CheckUsernameAvailability(string username)
        {
            using (var db = new BioMexDatabaseEntities1())
            {
                User use = (from person in db.Users where ((person.US_USERNAME == username)) select person).FirstOrDefault();

                if (use != null && use.US_USERNAME.Equals(username))
                {
                    System.Diagnostics.Debug.WriteLine("Username check: Username taken");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("Username check: Username available");
                return true;
            }
        }

        private List<int> DeserializeIntegers(string value)
        {
            char[] splitChars = new[] {','};

            value = value.Substring(1, value.Length - 3);
            
            var splitString = value.Split(splitChars);
            var myList = new List<int>();

            for(int i = 0; i < splitString.Length; i++)
            {
                myList.Add(int.Parse(splitString[i]));
            }

            return myList;
        }

        public List<int> RetrieveKeyDownTime(string username)
        {
            using (var db = new BioMexDatabaseEntities1())
            {
                User use = (from person in db.Users where ((person.US_USERNAME == username)) select person).FirstOrDefault();

                if (use != null && use.US_USERNAME.Equals(username))
                {
                    return DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_PRESS_DURATION);
                }
            }

            return null;

        }

        public List<int> RetrieveKeyLatency(string username)
        {
            using (var db = new BioMexDatabaseEntities1())
            {
                User use = (from person in db.Users where ((person.US_USERNAME == username)) select person).FirstOrDefault();

                if (use != null && use.US_USERNAME.Equals(username))
                {
                    return DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_LATENCIES);
                }
            }

            return null;
        }

        public List<int> RetrievePairedKeyTime(string username)
        {
            using (var db = new BioMexDatabaseEntities1())
            {
                User use = (from person in db.Users where ((person.US_USERNAME == username)) select person).FirstOrDefault();

                if (use != null && use.US_USERNAME.Equals(username))
                {
                    return DeserializeIntegers(use.Features.FirstOrDefault().FE_PAIRED_KEYS);
                }
            }

            return null;
        }

        public string LogUserIn(string user, string pass, int count, int shiftclass, int passwordSpeed, int negOrder, int[] keyorder, int[] keylatency, int[] keydowntime, int[] pairedkeys, string keyDescClass)
        {
            EncryptText("");
            //Remember to set date last logged in to current date
            string decUser = DecryptText(user);

            var tryUser = new UserClass(decUser, pass, count, shiftclass, passwordSpeed, negOrder, keyorder, keylatency, keydowntime, pairedkeys);

            System.Diagnostics.Debug.WriteLine("A user attempted to log in to BioMex");

            using (var db = new BioMexDatabaseEntities1())
            {
                User use = (from person in db.Users where ((person.US_USERNAME == decUser) & (person.US_PASSWORD == pass)) select person).FirstOrDefault();

                var regUser = new UserClass(use.US_USERNAME, use.US_PASSWORD, use.IdealFeatures.FirstOrDefault().IF_PASSWORD_COUNT, use.IdealFeatures.FirstOrDefault().IF_SHIFT_CLASS,
                                            use.IdealFeatures.FirstOrDefault().IF_TYPING_SPEED, 0, DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_ORDER.ToString()).ToArray(), DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_LATENCIES.ToString()).ToArray(),
                                            DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_PRESS_DURATION.ToString()).ToArray(), DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_PAIRED_KEYS.ToString()).ToArray());

               if (use != null && use.US_USERNAME.Equals(decUser) && use.US_PASSWORD.Equals(pass))
                {
                   CalculateMatch(decUser, keyDescClass);

                    int[] useThisInt = RetrieveDistances(decUser, passwordSpeed, keyorder, keylatency, keydowntime, pairedkeys);

                    if (useThisInt[0] <= _SPEED_DISTANCE_THRESHOLD && useThisInt[1] <= _KEYDOWN_DISTANCE_THRESHOLD && useThisInt[2] <= _KEYLATENCY_DISTANCE_THRESHOLD && useThisInt[3] <= _PAIREDKEY_DISTANCE_THRESHOLD)
                    {
                        System.Diagnostics.Debug.WriteLine("User successfully logged in.");

                        return EncryptText("ALLOW");
                    }                   
                }
                
                System.Diagnostics.Debug.WriteLine("User login attempt failed");
                return EncryptText("DONT");                
            }
        }

        private int DetermineDistance(int[] point1, int[] point2)
        {
            var total = 0;
            var dist = 0;
            for(int i = 0; i < point1.Length;i++)
            {
                if((i< point1.Length) && (i<point2.Length))
                if((point1[i] != null) && (point2[i] != null))
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

        private void CalculateMatch(string username, string keyDescClass)
        {
            try
            {
                string decKeyClass = DecryptText(keyDescClass);

                KeypointsDescriptors kpDescriptors = DeSerializeObject<KeypointsDescriptors>(decKeyClass);

                Matrix<byte> useDescriptors = kpDescriptors.descriptors;
                VectorOfKeyPoint useKeyPoints = kpDescriptors.keyPoints;


                using (var db = new BioMexDatabaseEntities1())
                {
                    BFMatcher matcher = new Emgu.CV.Features2D.BFMatcher(DistanceType.L2);
                    Matrix<byte> mask;

                    User use = (from person in db.Users where ((person.US_USERNAME == username)) select person).FirstOrDefault();

                    var regUser = new UserClass(use.US_USERNAME, use.US_PASSWORD, use.IdealFeatures.FirstOrDefault().IF_PASSWORD_COUNT, use.IdealFeatures.FirstOrDefault().IF_SHIFT_CLASS,
                                                use.IdealFeatures.FirstOrDefault().IF_TYPING_SPEED, 0, DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_ORDER.ToString()).ToArray(), DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_LATENCIES.ToString()).ToArray(),
                                                DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_KEY_PRESS_DURATION.ToString()).ToArray(), DeserializeIntegers(use.IdealFeatures.FirstOrDefault().IF_PAIRED_KEYS.ToString()).ToArray());

                    if (use != null && use.US_USERNAME.Equals(username))
                    {
                        matcher.Add(useDescriptors);

                        Matrix<byte> observedDesc = DeSerializeObject<Matrix<byte>>(use.Features.FirstOrDefault().FaceFeature.FF_DESCRIPTORS);
                        VectorOfKeyPoint observedKeypoints = DeSerializeObject<VectorOfKeyPoint>(use.Features.FirstOrDefault().FaceFeature.FF_KEY_POINTS);

                        Matrix<int> indices = new Matrix<int>(observedDesc.Rows, 2);

                        VectorOfVectorOfDMatch vectMatch = new VectorOfVectorOfDMatch();

                        using (Matrix<float> Dist = new Matrix<float>(observedDesc.Rows, 2))
                        {
                            matcher.KnnMatch(observedDesc, vectMatch, 2, null);
                            mask = new Matrix<byte>(Dist.Rows, 1);
                            mask.SetValue(255);
                            Features2DToolbox.VoteForUniqueness(vectMatch, 0.8, mask.ToUMat().ToMat(AccessType.Read));
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error while matching");
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }           

        }

        private bool DetermineOverallDifference(UserClass user1, UserClass user2)
        {
            var speedTotalDifference = 0;
            var downTotalDifference = 0;
            var latencyTotalDifference = 0;
            var pairedTotalDifference = 0;

            if (user1.GetPasswordCount().Equals(user2.GetPasswordCount()))
            {
                speedTotalDifference = DetermineDistance(user1.GetPasswordKeySpeed(), user2.GetPasswordKeySpeed());
                System.Diagnostics.Debug.WriteLine("Speed:  " + speedTotalDifference + "/1500");
                downTotalDifference = DetermineDistance(user1.GetKeyDownTime(), user2.GetKeyDownTime());
                System.Diagnostics.Debug.WriteLine("Key down:   " + downTotalDifference + "/1000");
                latencyTotalDifference = DetermineDistance(user1.GetKeyLatency(), user2.GetKeyLatency());
                System.Diagnostics.Debug.WriteLine("Key Latency:   " + latencyTotalDifference + "/1000");
                pairedTotalDifference = DetermineDistance(user1.GetPairedKeysSpeed(), user2.GetPairedKeysSpeed());
                System.Diagnostics.Debug.WriteLine("Paired Key:   " + pairedTotalDifference + "/1500");
            }

            if ((speedTotalDifference <= _SPEED_DISTANCE_THRESHOLD) && (downTotalDifference <= _KEYDOWN_DISTANCE_THRESHOLD) && (latencyTotalDifference <= _KEYLATENCY_DISTANCE_THRESHOLD) && (pairedTotalDifference <= _PAIREDKEY_DISTANCE_THRESHOLD))
            {
                System.Diagnostics.Debug.WriteLine("ACCESS GRANTED");
                return true;
            }
            System.Diagnostics.Debug.WriteLine("ACCESS DENIED");
            return false;

        }

        public int[] RetrieveDistances(string user, int passwordSpeed, int[] keyorder, int[] keylatency, int[] keydowntime, int[] pairedkeys)
        {
            var tryUser = new UserClass(user, "", 1337, 0, passwordSpeed, 0, keyorder, keylatency, keydowntime, pairedkeys);

            int[] newArray = new int[4];

            using (var db = new BioMexDatabaseEntities1())
            {
                User use = (from person in db.Users where ((person.US_USERNAME == user)) select person).FirstOrDefault();

                var regUser = new UserClass(use.US_USERNAME, use.US_PASSWORD, use.Features.FirstOrDefault().FE_PASSWORD_COUNT, use.Features.FirstOrDefault().FE_SHIFT_CLASS,
                                            use.Features.FirstOrDefault().FE_TYPING_SPEED, 0, DeserializeIntegers(use.Features.FirstOrDefault().FE_KEY_ORDER.ToString()).ToArray(), DeserializeIntegers(use.Features.FirstOrDefault().FE_KEY_LATENCIES.ToString()).ToArray(),
                                            DeserializeIntegers(use.Features.FirstOrDefault().FE_KEY_PRESS_DURATION.ToString()).ToArray(), DeserializeIntegers(use.Features.FirstOrDefault().FE_PAIRED_KEYS.ToString()).ToArray());

                if (use != null && use.US_USERNAME.Equals(user))
                {
                    newArray[0] = DetermineDistance(regUser.GetPasswordKeySpeed(), tryUser.GetPasswordKeySpeed());
                    System.Diagnostics.Debug.WriteLine("Speed:  " + newArray[0] + "/300");
                    newArray[1] = DetermineDistance(regUser.GetKeyDownTime(), tryUser.GetKeyDownTime());
                    System.Diagnostics.Debug.WriteLine("Key Down:  " + newArray[1] + "/400");
                    newArray[2] = DetermineDistance(regUser.GetKeyLatency(), tryUser.GetKeyLatency());
                    System.Diagnostics.Debug.WriteLine("Key Latency:  " + newArray[2] + "/500");
                    newArray[3] = DetermineDistance(regUser.GetPairedKeysSpeed(), tryUser.GetPairedKeysSpeed());
                    System.Diagnostics.Debug.WriteLine("Paired Keys:  " + newArray[3] + "/300");
                }

            }

            return newArray;
        }

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
