/////////////////////////////////////////////////////////////////////
// Tester.cs - Mock Testharness                                    //
// Environment:c#                                                  //
// Application:CSE681 proj4-Tester                                 //
// Author:      Weiheng, Syracuse University                       //
// Source:      Jim Fawcett                                        //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides operations to Create a child Application Domain,
 * load libraries into it, and run tests on all loaded libraries that
 * support the ITest interface. 
 * 
 * In order to load libraries without requiring the Tester to bind to
 * the types they declare, a Loader library is defined that is loaded
 * into the child domain, and loads each of the test libraries from
 * within the child. 
 * 
 * 
 * Public Interface:
 * =================
 * Tester tstr = new Tester();
 * Thread t = tstr.SelectConfigAndRun("TestLibraries");
 * tstr.ShowTestResults();
 * tstr.UnloadTestDomain();
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   Tester.cs, Loader.cs, 
 * Compiler Command:
 *   csc /t:Library Loader.cs
 *   csc /t:exe     Tester.cs
 * Deployment:
 *   Loader.dll --> TesterDir/Loader
 *   where TesterDir is the folder containing Tester.exe
 *  
 * Maintence History:
 * ==================
 * ver 1.1 : 17 Oct 05
 *   - uses new version of Loader.cs.  Otherwise unchanged.
 * ver 1.0 : 09 Oct 05
 *   - first release
 * 
 */
//
using System;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Collections;
using System.Security.Policy;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using wcfcom1;
using System.Text;
using System.Linq;
using Navigator;

namespace TestHarness
{
    public class Tester
    {
        string pathToTestLibs_ = "TestLibraries";  // ITest.dll and test libs
        string filename_ = "Test.dll";
        AppDomain ad;
        string results_;

        //----< Constructor placeholder >--------------------------------

        public Tester()
        {
        }
        //----< Create AppDomain in which to run tests >-----------------

        void CreateAppDomain()
        {
            AppDomainSetup domainInfo = new AppDomainSetup();
            domainInfo.ApplicationName = "TestDomain";
            Evidence evidence = AppDomain.CurrentDomain.Evidence;
            ad = AppDomain.CreateDomain("TestDomain", evidence, domainInfo);
        }
        //----< Load Loader and tests, run tests, unload AppDomain >-----

        void LoadAndRun()
        {
            Console.Write("\n\n  Loading and instantiating Loader in TestDomain");
            Console.Write("\n ------------------------------------------------");
            ad.Load("Loader");
            ObjectHandle oh = ad.CreateInstance("loader", "TestHarness.Loader");
            Loader ldr = oh.Unwrap() as Loader;
            ldr.SetFile(filename_);
            ldr.SetPath(pathToTestLibs_);
            ldr.LoadTests();
            results_ = ldr.RunTests();
        }
        //
        //----< Run tests in configDir >---------------------------------

        void runTests()
        {
            try
            {
                CreateAppDomain();
                LoadAndRun();
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
            }
            Console.Write("\n");
        }
        //----< unload Child AppDomain >---------------------------------

        void UnloadTestDomain()
        {
            AppDomain.Unload(ad);
        }
        //
        //----< show test results >--------------------------------------

        void ShowTestResults()
        {
            Console.Write("\n  Test Results returned to Tester");
            Console.Write("\n ---------------------------------\n");

            Console.Write("\n  {0}\n", results_);
            StringReader tr = new StringReader(results_);
            XmlTextReader xtr = new XmlTextReader(tr);
            xtr.MoveToContent();
            if (xtr.Name != "TestResults")
                throw new Exception("invalid test results: " + results_);
            int count = 0;
            string name = "", text = "";
            while (xtr.Read())
            {
                if (xtr.NodeType == XmlNodeType.Element)
                    name = xtr.Name;
                if (xtr.NodeType == XmlNodeType.Text)
                {
                    if (xtr.Value == "True")
                        text = "passed";
                    
                    else
                        text = "failed";
                    ++count;
                    Console.Write("\n  Test #{0}: {1} - {2}", count, text, name);
                }
            }
            Console.Write("\n\n");
        }
        //
        //----< run configuration on its own thread >--------------------

        Thread SelectConfigAndRun(string configDir, string filename)
        {
            filename_ = filename;
            Console.WriteLine("file name is"+filename_);
            pathToTestLibs_ = configDir;
            Thread t = new Thread(new ThreadStart(this.runTests));
            t.Start();
            return t;
        }
        //----< demonstrate Test Harness Prototype >---------------------

        static void Main(string[] args)
        {
            Console.WriteLine("============================this is the testharness====================");
            Console.WriteLine("\nthe testharness can load and test the libraries, meet the requirement 9");
            Receiver testreceiver = new Receiver();
            testreceiver.CreateHost("http://localhost:3000");
            while (true)
           {
                try
                {
                    CommMessage msg = testreceiver.GetMessage();
                    Console.WriteLine("\n=================start new test========================");
                    Console.WriteLine("\nreceive test request from the child builder");
                    Console.WriteLine("\nthe library need to be test is:{0}", msg.Body);     
                    Tester tstr = new Tester();
                    Thread t = tstr.SelectConfigAndRun("TestLibraries", msg.Body);
                    Console.WriteLine();
                    t.Join();
                    Sender logsender = new Sender();
                    logsender.CreateChannel("http://localhost:2017");
                    logsender.postFile(msg.Body + "testlog.txt", TestharnessEnvironment.testlogpath, 1024, RepoEnvironment.testlogpath);//send log to the repository
                    tstr.ShowTestResults();
                    tstr.UnloadTestDomain();
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
