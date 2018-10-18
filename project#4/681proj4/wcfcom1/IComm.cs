///////////////////////////////////////////////////////////////////////////
// IComm.cs - interface for other packages
// Version 1                                                          
// Author: Chai Weiheng
// Application:CSE681 proj4, Fall 2017
///////////////////////////////////////////////////////////////////////////
/*
 * This package provides:
 * ----------------------
 * 
 * - IComm   : interface for the communication service
 * - IVersion   : interface for versioning repository contents
 * 
 * 
 * Maintenance History:
 * --------------------
 *ver 1.0 : Oct 25 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.IO;

namespace wcfcom1
{
    using Argument = String;
    using Command = String;

    [ServiceContract]
    public interface IComm
    {


        [OperationContract(IsOneWay = true)]
        void PostMessage(CommMessage msg);
        [OperationContract]
        bool openFileForWrite(string name,string receivepath);

        [OperationContract]
        bool writeFileBlock(byte[] block);
        [OperationContract(IsOneWay = true)]
        void closeFile();
    }

    public enum FileMessage
    {
        [EnumMember]
        CommMessage,

        [EnumMember]
        FileMessge
    }

    [DataContract]
    public class CommMessage
    {
        public enum MessageType
        {
            [EnumMember]
            connect,           // initial message sent on successfully connecting
            [EnumMember]
            request,           // request for action from receiver
            [EnumMember]
            reply,             // response to a request
            [EnumMember]
            closeSender,       // close down client
            [EnumMember]
            closeReceiver      // close down server for graceful termination
        }
   
        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string Destinaiton { get; set; }

        [DataMember]
        public string Body { get; set; }

        [DataMember]
        public MessageCommand Command { get; set; }
        [DataMember]
        public List<Argument> arguments { get; set; } = new List<Argument>();
        [DataMember]
        public Command command { get; set; }//only used for message dispatcher
        public enum MessageCommand
        {
            [EnumMember]
            ConnectionTest,

            [EnumMember]
            BuildRequest,

            [EnumMember]
            TestRequest,

            [EnumMember]
            Processdone,

            [EnumMember]
            Sendbuildrequest,
            [EnumMember]
            getfiles,
            [EnumMember]
            requestfiles,
            [EnumMember]
            CloseChildBuilder,
            [EnumMember]
            startchild
        }
    }
}
