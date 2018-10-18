///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//childprocess.cs:child builder for proj#4                                                                                   //
//Author:Chai,Weiheng,wchai01@syr.edu                                                                                        //
//Application:CSE681 Proj4-childproccess                                                                                     //
//Environment:c# console                                                                                                     //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
*build and create log, send the log to the repository, if build success, send the dll to the testharness and the repository
*
* Public Interface
* ==================================================================
* No public interface in this package
* 
* All Other Functions used in this package
* ==================================================================
* static bool ProcessCommandLineArgs(string[] args)-----get the commandline arguement and set the address of host
* static void BuildCsproj(string filename)-----build the files into dll,create and send logs after build.
* static string getfiles(string buildrequest)-----parse the build request and send message to  get the needed files from the repository, return the name of csproj file which
* is needed by the static void BuildCsproj(string filename)
* static void SendMessage()-----send the ready message to the mother builder
* static void senddllfiles(string filename)-----send the dll files to the repository and the testharness
* 
* Required Files:
* =============================================
* Environment1
* wcfcom1
* 
* 
* Build command:
* =============================================
* csc{App.config,childprocess.cs}
* dnvenv Childbuilder.csproj
* 
*  * Maintenance History: 
* -------------------- 
* ver 1.0 : 07/12/2017 
* - first release 
*/
using System;
using System.Collections.Generic;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;
using wcfcom1;
using System.Linq;
using System.IO;
using HelpSession;
using System.Threading;
using Navigator;


namespace Buildserver1
{
    class ChildBuilder
    {
        public string storagePath { get; set; } = @"..\..\..\ChildBuilderstorage";
        
        public static string address { get; set; } //the port of the host
        static bool ProcessCommandLineArgs(string[] args)//get the command line arguments to set the address
        {
            if (args.Count() == 0) return false;
            address = args[0];
            return true;
        }
        static void BuildCsproj(string filename)//build the files
        {
            Console.WriteLine("\n============================build start================================");
            Console.WriteLine("\nmeet the requirement6,7,8!");
            string filepath = Path.Combine(@"..\..\..\ChildBuilderstorage", filename);
            string projectFileName = filepath;// using MSBuild to build
            FileStream fs = new FileStream(@"..\..\..\ChildBuilder\builderstorage\logs\"+filename+"buildlog.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);
            sw.Close();/*using FileLogger to write logs in txt*/
            FileLogger logger = new FileLogger(){ Parameters = @"logfile=..\..\..\ChildBuilder\builderstorage\logs\" + filename + "buildlog.txt", Verbosity = LoggerVerbosity.Detailed };
            Dictionary<string, string> GlobalProperty = new Dictionary<string, string>();
            BuildRequestData BuildRequest = new BuildRequestData(projectFileName, GlobalProperty, null, new string[] { "Rebuild" }, null);
            BuildParameters bp = new BuildParameters();
            bp.Loggers = new List<Microsoft.Build.Framework.ILogger>{ logger }.AsEnumerable();
            BuildResult buildResult = BuildManager.DefaultBuildManager.Build(bp, BuildRequest);
            string result = buildResult.OverallResult.ToString();            
            Console.WriteLine("\nbuild result is:{0}\n",result);        
            Console.WriteLine("\nbuild log has been stored in:{0}, the name of the log is:{1}",Path.GetFullPath(@"..\..\..\ChildBuilder\builderstorage\logs"), filename + "buildlog.txt");
            Sender logsender = new Sender();
            logsender.CreateChannel("http://localhost:2017");
            logsender.postFile(filename + "buildlog.txt", @"..\..\..\ChildBuilder\builderstorage\logs",1024, RepoEnvironment.buildlogpath);//send logs
            Console.WriteLine("\nbuild log had been send to:" + Path.GetFullPath(RepoEnvironment.buildlogpath));
            string dllname = Path.GetFileNameWithoutExtension(@"..\..\..\ChildBuilderstorage\" + filename );
            string libname = dllname + ".dll";
            Console.WriteLine("librarynameis:{0}",libname);
            if (result == "Success")
            {
                senddllfiles(libname);
            }
        }
        static void senddllfiles(string filename)
        {
            Sender sendtotest = new Sender();                                          
            sendtotest.CreateChannel("http://localhost:3000");
            sendtotest.postFile(filename, @"..\..\..\ChildBuilderstorage\dll", 1024, @"..\..\..\Tester\bin\Debug\TestLibraries");                     //send the library to the testharness
            Console.WriteLine("\nsend the dll to the testharness,path is:{0}", Path.GetFullPath(@"..\..\..\Tester\bin\Debug\TestLibraries"));
            CommMessage testmsg = new CommMessage();
            testmsg.Destinaiton = "testharness";
            testmsg.Source = "childbuilder";
            testmsg.Body = filename;
            sendtotest.PostMessage(testmsg);
            Console.WriteLine("\nsend test request to the testharness,the dll need to tbe test is:{0}",filename);
            Sender sendtorepo = new Sender();
            sendtorepo.CreateChannel("http://localhost:2017");
            sendtorepo.postFile(filename, @"..\..\..\ChildBuilderstorage\dll", 1024, RepoEnvironment.dllstopath);                                   //send the library to the repository
            Console.WriteLine("\nsend the dll to the reoisitory,path is{0}",Path.GetFullPath(RepoEnvironment.dllstopath));

        }
        static void SendMessage()//send message to the motherbuilder
        {
            Sender sender = new Sender();
            sender.CreateChannel("http://localhost:9999");
            CommMessage senderMsg = new CommMessage();
            senderMsg.Source = address;
            senderMsg.Destinaiton = "motherbuilder";
            senderMsg.Command = CommMessage.MessageCommand.Processdone;
            senderMsg.Body = "done";
            Console.WriteLine("\n==========================send message to motherbuilder==============================");
            Console.WriteLine("\nmessage.body:done");
            sender.PostMessage(senderMsg);                                
        }
        static string getfiles(string buildrequest)
        {
            requestbuilder rb = new requestbuilder();
            rb.loadXml(@"..\..\..\ChildBuilderstorage\" + buildrequest);//parse the build request, and request filers from the repository according to the information in the buildrequest
            rb.parse("author");
            rb.parse("csproj");           
            rb.parse("testDriver");
            rb.parseList("tested");
            Console.WriteLine("author is{0}", rb.author);
            Console.WriteLine("csproj is {0}", rb.csproj);
            Console.WriteLine("testdriver is {0}", rb.testDriver);
            foreach (var item in rb.testedFiles)
            {
                Console.WriteLine("tested file is {0}", item);
            }
            Sender senderrepo = new Sender();
            senderrepo.CreateChannel("http://localhost:2017");
            CommMessage msgcsproj = new CommMessage();
            msgcsproj.Command = CommMessage.MessageCommand.requestfiles;
            msgcsproj.Destinaiton = "repo";
            msgcsproj.Source = address;
            msgcsproj.Body = rb.csproj;
            senderrepo.PostMessage(msgcsproj);
            CommMessage msgtestdriver = new CommMessage();
            msgtestdriver.Command = CommMessage.MessageCommand.requestfiles;
            msgtestdriver.Destinaiton = "repo";
            msgtestdriver.Source = address;
            msgtestdriver.Body = rb.testDriver;
            senderrepo.PostMessage(msgtestdriver);
            foreach (var item in rb.testedFiles)
            {
                CommMessage msgtested = new CommMessage();
                msgtested.Command = CommMessage.MessageCommand.requestfiles;//get files
                msgtested.Destinaiton = "repo";
                msgtested.Source = address;
                msgtested.Body = item;
                senderrepo.PostMessage(msgtested);
            }
            return rb.csproj;
        }
        static void Main(string[] args)
        {
            ProcessCommandLineArgs(args);
            Console.WriteLine("=======================this is the child builder======================================");
            Receiver receiver = new Receiver();            
            receiver.CreateHost("http://localhost:" +address);
            Console.WriteLine("the uri is " + "http://localhost:"+address);
            while (true)
            {
                CommMessage msg = receiver.GetMessage();
                Thread.Sleep(2000);
                 if (msg.Body == "quit")
                {
                    break;//close the child builder
                }
                else if (msg.Body != "quit")
                {
                    string str = msg.Body;
                    string csprojname=getfiles(msg.Body);
                    Thread.Sleep(2000);                                      //I do not have enough time to write a code get the builder work after receive the files, so write a sleep.
                    bool flag = true;
                    while (flag)
                    {
                        try
                        {
                            BuildCsproj(csprojname);                           //build.
                            SendMessage();
                            flag = false;
                        }
                        catch (Exception ex)
                        {
                            Console.Write("\n\n  An error occured while trying to build the csproj file.\n  Details: {0}\n\n", ex.Message);
                            Console.ResetColor();
                        }
                    }                    
                }             
            }
        }      
    }
}

