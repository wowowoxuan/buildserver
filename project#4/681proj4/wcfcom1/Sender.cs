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
 * public void PostMessage(CommMessage msg)--send CommMessage instance to a Receiver instance
 * public void CreateChannel(string address)--creat channe to send file and messages
 * public bool postFile(string fileName,string fileStorage,long blockSize)--called by a Sender instance to transfer a file
 * 
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
using System.ServiceModel;
using System.IO;

namespace wcfcom1
{
    public class Sender
    {
        private ChannelFactory<IComm> factory = null;
        string lastError = "";
        private IComm channel;
        Receiver receiver = new Receiver();
        public Sender()
        {
        }

        public void PostMessage(CommMessage msg)
        {
            if (channel == null)
            {
                Console.Write("\n Cannot send message. The channel is not initial.");
                return;
            }
           // Console.Write("\n Sending message to {0}.Body{1}", msg.Destinaiton,msg.Body);
            channel.PostMessage(msg);
        }

        public void CreateChannel(string address)
        {
            EndpointAddress baseaddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            factory = new ChannelFactory<IComm>(binding, address);
            channel = factory.CreateChannel();
        }
        public bool postFile(string fileName,string fileStorage,long blockSize,string receivepath)
        {

            FileStream fs = null;
            long bytesRemaining;

            try
            {
                string path = Path.Combine(fileStorage, fileName);
                fs = File.OpenRead(path);
                bytesRemaining = fs.Length;
                channel.openFileForWrite(fileName,receivepath);

                while (true)
                {
                    long bytesToRead = Math.Min(blockSize, bytesRemaining);
                    byte[] blk = new byte[bytesToRead];
                    long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
                    bytesRemaining -= numBytesRead;

                    channel.writeFileBlock(blk);
                    if (bytesRemaining <= 0)

                        break;
                }
                fs.Close();
                fs.Dispose();
                channel.closeFile();
                

            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
            return true;
        }
    }
}
