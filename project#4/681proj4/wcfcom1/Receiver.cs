//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Receiver.cs:service interface for communication
//Author:Chai,Weiheng,wchai01@syr.edu
//Source:Ammar,Jim Fawcett
//Application:Project#4,CSE 681, Fall2017
//Environment:c#
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*Package Operations:
 * ====================
 * this package implements the public methods
 * 
 * public interface:
 * --------------------
 * public void CreateHost(string address)--creates instance of ServiceHost which services incoming messages
 * public void PostMessage(CommMessage msg)--Sender proxies call this message to enqueue for processing
 * public CommMessage GetMessage()--called by Receiver application to retrieve incoming messages
 * public bool openFileForWrite(string name)--opens a file for storing incoming file blocks
 * public void closeFile()--closes newly uploaded file
 * public bool writeFileBlock(byte[] block)-- writes an incoming file block to storage
 * 
 * 
 * Required Files:
* =============================================
* IComm.cs
* Sender.cs
* Receiver.cs
* BlockingQueue.cs
* 
* 
 * Maintence History:
 * --------------------
 * ver 1.0 : Oct 25 2017
 * - first release
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;
using SWTools;
using System.IO;
using System.Threading;

namespace wcfcom1
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Receiver : IComm
    {
        static BlockingQueue<CommMessage> mQueue;
        public static string WDeirectory { get; set; }
        static FileStream fs { get; set; }
        ServiceHost Host;
        string lastError = "";

        public string Address { get; private set; }

        public Receiver()
        {
            if (mQueue == null)
                mQueue = new BlockingQueue<CommMessage>();
            Host = null;
        }

        public void CreateHost(string address)//creat a host for the service
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(address);
            Host = new ServiceHost(typeof(Receiver), baseAddress);
            Host.AddServiceEndpoint(typeof(IComm), binding, baseAddress);
            Host.Open();


        }

        [OperationBehavior(TransactionAutoComplete = true)]
        public void PostMessage(CommMessage msg)//send message
        {
            mQueue.enQ(msg);
        }

        [OperationBehavior]
        public CommMessage GetMessage()
        {
            return mQueue.deQ();
        }


        [OperationBehavior]
        public bool openFileForWrite(string name,string receivepath)
        {
            try
            {
                string writePath = Path.Combine(receivepath, name);
                fs = File.OpenWrite(writePath);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
        /*----< write a block received from Sender instance >----------*/
        [OperationBehavior]
        public bool writeFileBlock(byte[] block)
        {
            try
            {
                fs.Write(block, 0, block.Length);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
        /*----< close Receiver's uploaded file >-----------------------*/
        [OperationBehavior]
        public void closeFile()
        {
            fs.Close();
        }
    }
#if (TEST_WCF)

  class Program
  {
    static void Main(string[] args)
    {
            Receiver receiver = new Receiver();
            receiver.CreateHost("http://localhost:9077");
            Sender sender = new Sender();
            sender.CreateChannel("http://localhost:9077");
            CommMessage msg=new CommMessage();
            msg.Body = "test";
            msg.Destinaiton = "myself";
            msg.Source = "me";
            sender.PostMessage(msg);
            CommMessage msg2 = new CommMessage();
           msg2= receiver.GetMessage();
            Console.WriteLine("message is:"+msg2.Body);

    }
  }
#endif
    
}

