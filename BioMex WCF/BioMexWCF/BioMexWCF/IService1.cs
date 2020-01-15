using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using BioMex_First;

namespace BioMexWCF
{
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        void RegisterNewUser(string name, string surname, string username, int age, string password, int passwordCount, int shiftClassification, List<int> passwordSpeed, List<List<int>> keyDowns, List<List<int>> keyLatencies, List<List<int>> pairedKeys, List<List<int>> keyOrder, string keyDescClass);

        [OperationContract]
        string LogUserIn(string user, string pass, int count, int shiftclass, int passwordSpeed, int negOrder, int[] keyorder, int[] keylatency, int[] keydowntime, int[] pairedkeys, string keyDescClass);

        [OperationContract]
        bool CheckUsernameAvailability(string username);

        [OperationContract]
        List<int> RetrieveKeyDownTime(string username);

        [OperationContract]
        List<int> RetrieveKeyLatency(string username);

        [OperationContract]
        List<int> RetrievePairedKeyTime(string username);

        [OperationContract]
        bool ActivateService();

        [OperationContract]
        int[] RetrieveDistances(string user, int passwordSpeed, int[] keyorder, int[] keylatency, int[] keydowntime, int[] pairedkeys);
    }
}
