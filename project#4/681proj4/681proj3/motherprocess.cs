///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//motherprocess.cs:build Server for proj#4                                                                                   //
//Author:Chai,Weiheng,wchai01@syr.edu                                                                                        //
//Application:CSE681 Proj4-motherprocess                                                                                     //
//Environment:c# console                                                                                                     //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
*1.create a several child builder according to the message from the Client
*2.receive build request from the repository 
*3.send the build request to the child builder which is not occupied 
*4.close the child builder when receive the quit message.
* ==================================================================
* No public interface in this package
* 
* All Other Functions used in this package
* ==================================================================
* MotherBuilder()-----constructor, create the two queue of the mother builder for message processing.
* static void ReceiveMessage()-----using wcf receive message from other servers and the child builder
* static void SendMessage(string buildrequest, string str)-----send message to the child builder
* static void SendFile(string filename)-----send files to the child builder
* 
* 
* 
* Required Files:
* =============================================
* wcfcom1
* 
* 
* Build command:
* =============================================
* csc{App.config,blockingqueue.cs,motherprocess.cs}
* dnvenv motherbuilder.csproj
* 
*  * Maintenance History: 
* -------------------- 
* ver 1.0 : 07/12/2017 
* - first release 
*/
using System;
using System.Diagnostics;
using wcfcom1;
using System.Threading;
using SWTools;
using System.IO;
namespace _681proj3
{
    class MotherBuilder
    {
        public string storagePath { get; set; } = @"..\..\..\681proj3\FileStore";
        public static string hostAddress { get; set; }
        public static int NoChildProcesses { get; private set; }
        static BlockingQueue<CommMessage> q1=new BlockingQueue<CommMessage>();
        static BlockingQueue<CommMessage> q2 = new BlockingQueue<CommMessage>();
        static BlockingQueue<CommMessage> childcommQue=q1;//the blocking queue for store message from the child builder
        static BlockingQueue<CommMessage> repocommQue=q2;//the blocking queue for store message from the client and therepository
        MotherBuilder()//constructor
        {
            if (childcommQue == null)
               childcommQue = new BlockingQueue<CommMessage>();
            if (repocommQue == null)
                repocommQue = new BlockingQueue<CommMessage>();
        }
        static void ReceiveMessage()//receive the message using wcf
        {
            Receiver receiver = new Receiver();
            receiver.CreateHost("http://localhost:9999");
            while (true)
            {
                CommMessage msg = receiver.GetMessage();                                                         //the mother builder can receive 3 kinds of message
                if (msg.Command == CommMessage.MessageCommand.startchild) //1.command=startchild, the mother builder will start several child builder according to the message                                     
                {
                    int k = Int32.Parse(msg.Body);
                    try
                    {
                        Console.WriteLine("\n------------start child builder meet the requirement 5--------------");
                        for (int j = 0; j < k; j++)
                        {
                            string str = Convert.ToString(j);
                            CommMessage msg1 = new CommMessage();                        //when start a new child builder, put this message in the child queue so that the mother builder
                            msg1.Body = "initial";                                       //knows that the new started child builder is not occupied
                            msg1.Source = "100" + j;
                            msg1.Command = CommMessage.MessageCommand.Processdone;
                            msg1.Destinaiton = "motherbuilder";
                            childcommQue.enQ(msg1);
                            Process.Start(@"..\..\..\ChildBuilder\bin\Debug\ChildBuilder.exe", "100" + j);
                        }
                    }
                    catch { continue; }
                }
                else
              {
                    if (msg.Command != CommMessage.MessageCommand.CloseChildBuilder)
                    {
                        Console.WriteLine("\n=================the mother process receive a new message===================");
                        Console.WriteLine("\nmessage is from:" + msg.Source);
                        Console.WriteLine("\nmessage is:{0}", msg.Body);
                        Console.WriteLine("\n============================================================================");
                    }
                    if (msg.Body == "done")                     //the ready message from the child builder     
                    {
                        Console.WriteLine("\nchild builder:{0} is ready for new work",msg.Source);
                        childcommQue.enQ(msg);
                    }
                    if (msg.Command == CommMessage.MessageCommand.BuildRequest || msg.Command == CommMessage.MessageCommand.CloseChildBuilder)//both the message of build request and the 
                    {                                                                                                                         //message of close child builder enque into 
                        repocommQue.enQ(msg);                                                                                                 //the same queue, so we will never stop a
                    }                                                                                                                         //child builder which is working
                }
            }
        }
        static void SendMessage(string buildrequest, string str)//send message to the child process
        {
            Sender sender = new Sender();
            sender.CreateChannel("http://localhost:" + str);
            CommMessage senderMsg = new CommMessage();
            senderMsg.Source = "Motherbuilder";
            senderMsg.Destinaiton = "childprocess"+str;
            senderMsg.Command = CommMessage.MessageCommand.BuildRequest;
            senderMsg.Body = buildrequest;
            sender.PostMessage(senderMsg);
        
        }
        static void SendFile(string filename)//send files to the child process
        {
            Sender sender = new Sender();
            sender.CreateChannel("http://localhost:1000");
            string filespec = @"..\..\..\ChildBuilderstorage";
            string fullpath = Path.GetFullPath(filespec);
            sender.postFile(filename, @"..\..\..\motherstorage", 1024,filespec);
            Console.WriteLine("\nsend file:{0},from:{1} to:{2},meet requirement 3",filename,Path.GetFullPath(@"..\..\..\motherstorage"),Path.GetFullPath(@"..\..\..\ChildBuilderstorage"));
        }
        static void Main(string[] args)
        {
            Console.WriteLine("\n============================this is the motherbuilder=================================");
            Console.WriteLine("\nthe mother builder can start a number of child builder, meet the requirement 5.");
            Thread thread = new Thread(ReceiveMessage);
            thread.Start();
            while (true)
            {
                if (childcommQue.size() != 0 && repocommQue.size() != 0)
                {
                    CommMessage msgchild = childcommQue.deQ();
                    CommMessage msgrepo = repocommQue.deQ();
                    try
                    {
                        if (msgrepo.Body != "quit")
                        {
                            Console.WriteLine("=========================send message and file to the child process======================");
                            Console.WriteLine("\n get file from the repository, file name is:{0}",msgrepo.Body);
                            Console.WriteLine("\nsend file to the childrprocess,file name is:{0}", msgrepo.Body);
                            Console.WriteLine("\nsend message to the childprocess,the port of the childprocess is:http://localhost:" + msgchild.Source);
                            Console.WriteLine("\nmessage is:" + msgrepo.Body);
                            Console.WriteLine("==========================================================================================");
                            SendMessage(msgrepo.Body, msgchild.Source);              //tell the child builder which build request to build
                            SendFile(msgrepo.Body);                                  //send the build request to the child builder
                        }
                        else if (msgrepo.Body == "quit")                                           //the command to close the child builder
                        {
                            Sender sender = new Sender();
                            sender.CreateChannel("http://localhost:" + msgchild.Source);
                            CommMessage senderMsg = new CommMessage();
                            senderMsg.Source = "Motherbuilder";
                            senderMsg.Destinaiton = "childprocess" + msgchild.Source;
                            senderMsg.Command = CommMessage.MessageCommand.BuildRequest;
                            senderMsg.Body = "quit";
                            sender.PostMessage(senderMsg);
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }
    }
}
