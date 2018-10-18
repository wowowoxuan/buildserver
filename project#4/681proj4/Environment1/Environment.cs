///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Environment.cs:define the environment properties for proj#4                                                                //
//Author:Chai,Weiheng,wchai01@syr.edu                                                                                        //
//Application:CSE681 Proj4-Environment                                                                                       //
//Environment:c# console                                                                                                     //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/* 
*
*
* Public Interface
* ==================================================================
* public struct Environment1----- set environment properties
* public struct GuiEnvironment-----stored the properties needed by the client
* public struct ServerEnvironment-----stored the properties needed by the repository
* ublic struct TestharnessEnvironment-----stored the properties needed by the testharness
* public struct RepoEnvironment-----stored the path needed by the testharness
* 
* 
*  * Maintenance History: 
* -------------------- 
* ver 1.0 : 07/12/2017 
* - first release 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator
{
    public struct Environment1
    {
        public static string root { get; set; }
        public static string endPoint { get; set; }
        public static string address { get; set; }
        public static int port { get; set; }
        public static bool verbose { get; set; }
    }

    public struct ServerEnvironment
    {
        public static string root { get; set; } = "../../../repostorage/";
        public static string endPoint { get; set; } = "http://localhost:8080/IMessagePassingComm";
        public static string address { get; set; } = "http://localhost";
        public static int port { get; set; } = 8080;
        public static bool verbose { get; set; } = false;
    }
    public struct TestharnessEnvironment
    {
        public static string testlogpath { get; set; } = @"../../../Tester/testlogs";
        public static int port { get; set; } = 3000;
    }
    public struct RepoEnvironment
    {
        public static string testlogpath { get; set; } = @"../../../repostorage/testlog";
        public static string buildlogpath { get; set; } = @"../../../repostorage/buildlog";
        public static string dllstopath { get; set; } = @"../../../repostorage/dll";
        public static string repostopath { get; set; } = @"../../../repostorage";
    }
    public struct GuiEnvironment
    {
        public static string guistorage { get; set; } = @"..\..\..\guistorage";
    }
}
