///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//repository.cs:Repository Server for proj#4                                                                                 //
//Author:Chai,Weiheng,wchai01@syr.edu                                                                                        //
//Application:CSE681 Proj4-repo                                                                                              //
//Environment:c# console                                                                                                     //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
*1.send list of files and dirs to the Client 2.when receive  the send build request command from the Client, send the build request to the mother builder
*
* Public Interface
* ==================================================================
* No public interface in this package
* 
* All Other Functions used in this package
* ==================================================================
* void initializeEnvironment()-----set Environment properties needed by server
* repository()-----(constructor)initialize server processing
* void initializeDispatche()-----define how each message will be processed
* 
* 
* Required Files:
* =============================================
* Environment1
* FileMgr1
* wcfcom1
* 
* 
* Build command:
* =============================================
* csc{App.config,repository.cs}
* dnvenv repo.csproj
* 
*  * Maintenance History: 
* -------------------- 
* ver 1.0 : 07/12/2017 
* - first release 
*/
using Navigator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wcfcom1;

namespace repo
{
    class repository
  {
        IFileMgr localFileMgr { get; set; } = null;
        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
    new Dictionary<string, Func<CommMessage, CommMessage>>();
        void initializeEnvironment()                   //set Environment properties needed by server
        {
            Environment1.root = ServerEnvironment.root;
            Environment1.address = ServerEnvironment.address;
            Environment1.port = ServerEnvironment.port;
            Environment1.endPoint = ServerEnvironment.endPoint;
        }
         repository()                                  //initialize repository processing
        {
            initializeEnvironment();
            localFileMgr= FileMgrFactory.create(FileMgrType.Local);
        }
        void initializeDispatcher()                    //define how each message will be processed
        {
            CommMessage reply = new CommMessage();
            reply.Source = "repo";
            reply.Destinaiton = "gui";
            Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                reply.command = "getTopFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopFiles"] = getTopFiles;
            Func<CommMessage, CommMessage> getBuildRequest = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                reply.command = "getBuildRequest";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["getBuildRequest"] = getBuildRequest;
            Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                reply.command = "getTopDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopDirs"] = getTopDirs;
            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {
                 if (msg.arguments.Count() == 1)localFileMgr.currentPath = msg.arguments[0];
                reply.command = "moveIntoFolderFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;
            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)localFileMgr.currentPath = msg.arguments[0];
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("===========================this is the Repository============================");
            Receiver receiver = new Receiver();
            receiver.CreateHost("http://localhost:2017");
            repository repo = new repository();
            repo.initializeDispatcher();          
            while (true)                                                              //the repository will receive three kinds of message, with each kind of message, the repository
            {                                                                         //will do different things.      
                        CommMessage msg = receiver.GetMessage();
                        if (msg.Command == CommMessage.MessageCommand.getfiles)       //command=getfiles, send the list of files'name to the Client
                        {
                            CommMessage reply = repo.messageDispatcher[msg.command](msg);
                            Sender sender = new Sender();
                            sender.CreateChannel("http://localhost:1234");
                            Console.WriteLine("\nsend list of files to the client!");
                            sender.PostMessage(reply);
                        }
                        else if (msg.Command == CommMessage.MessageCommand.Sendbuildrequest) //command=Sendbuildrequest, send the buildrequest to the mother builder and send a message to
                        {                                                                    //it to let it work(the work of the system is drived by message)
                            Console.WriteLine("\nreceive command to send build request to the mother builder,the build request is:{0}", msg.Body);
                            Sender sender1 = new Sender();
                            sender1.CreateChannel("http://localhost:9999");
                            Console.WriteLine("\nbuild request has send to the motherbuilder, the path is:{0}",Path.GetFullPath(@"..\..\..\motherstorage"));
                            sender1.postFile(msg.Body, RepoEnvironment.repostopath, 1024, @"..\..\..\motherstorage");
                            CommMessage msg1 = new CommMessage();
                            msg1.Destinaiton = "motherbuilder";
                            msg1.Source = "repo";
                            msg1.Body = msg.Body;
                            msg1.Command = CommMessage.MessageCommand.BuildRequest;
                            sender1.PostMessage(msg1);
                        }
                        else if (msg.Command == CommMessage.MessageCommand.requestfiles)      //command=requestfiles,send the files that are needed to build to the child builder
                        {
                            Console.WriteLine("\nreceive file request from the child builder,the port is:{0}", msg.Source);
                            Console.WriteLine("\nsend the file to the child builder,the file is:{0},the path is:{1}",msg.Body,Path.GetFullPath(@"..\..\..\ChildBuilderstorage"));
                            Sender sender2 = new Sender();
                            sender2.CreateChannel("http://localhost:" + msg.Source);
                            sender2.postFile(msg.Body, RepoEnvironment.repostopath, 1024, @"..\..\..\ChildBuilderstorage");
                        }                        
                    }        
                }
            }
        }
    






