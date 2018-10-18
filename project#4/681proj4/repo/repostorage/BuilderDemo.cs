/////////////////////////////////////////////////////////////////////////////////
// BuilderDemo.cs : builds projects using .csproj or .xml config files         //
// ver 1.0                                                                     //
//                                                                             //
// Platform     : Acer Aspire R15, Windows 10 Pro x64, Visual Studio 2017      //
// Application  : CSE-681 - Builder Demonstration                              //
// Author       : Ammar Salman, EECS Department, Syracuse University           //
//                (313)-788-4694, hoplite.90@hotmail.com                       //
/////////////////////////////////////////////////////////////////////////////////
/*
 * Description: this package demonstrates building .csproj and .xml files.
 *              The attached .csproj and .xml files are identical except
 *              for the selected default build configuration. The default
 *              build config in the .csproj file is Debug/AnyCPU while 
 *              the default config in the .xml file is Release/x64
 *              
 *              In both files, Debug/AnyCPU builds x86 DLL file and 
 *              Release/x64 builds x64 EXE file. This is just to show
 *              that using the XML description file can be used for
 *              everything which leaves nothing for the build server
 *              to worry about except .csproj/.xml file and MSBuild
 *              does everything as specified in the file.
 *              
 */

using System;
using System.Collections.Generic;

using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;


namespace Builder
{
  class BuilderDemo
  {

    /* 
     * This method uses MSBuild to build a .csproj file.
     * The csproj file is configured to build as Debug/AnyCPU
     * Therefore, there is no need to specify the parameters here.
     * This is useful for the build server because it should be as
     * general as it can get. The build server shouldn't have to
     * specify different build parameters for each project. 
     * Instead, the csproj/xml file sets the configuration settings.
     * 
     * In the csproj file, the OutputPath is set to "csproj_Debug" 
     * for the Debug configuration, and "csproj_Release" for the
     * Release configuration. Moreover, if Debug was selected, the
     * project will be build into an x86 library (DLL), while if Release
     * was selected, the project will build into an x64 executable (EXE)
     * 
     * To change the default configuration, the first PropertyGroup
     * in the ..\..\..\files\Builder.csproj must be modified.
     */
    static void BuildCsproj()
    {
      string projectFileName = @"..\..\..\files\Builder.csproj";

      ConsoleLogger logger = new ConsoleLogger();

      Dictionary<string, string> GlobalProperty = new Dictionary<string, string>();
      BuildRequestData BuildRequest = new BuildRequestData(projectFileName, GlobalProperty, null, new string[] { "Rebuild" }, null);
      BuildParameters bp = new BuildParameters();
      bp.Loggers = new List<ILogger> { logger };

      BuildResult buildResult = BuildManager.DefaultBuildManager.Build(bp, BuildRequest);

      Console.WriteLine();
    }

    /* 
     * This method uses MSBuild to build a .xml file.
     * The xml file is configured to build as Release/x64
     * 
     * In the xml file, the OutputPath is set to "xml_Debug" 
     * for the Debug configuration, and "xml_Release" for the
     * Release configuration. Moreover, if Debug was selected, the
     * project will be build into an x86 library (DLL), while if Release
     * was selected, the project will build into an x64 executable (EXE)
     * 
     * To change the default configuration, the first PropertyGroup
     * in the ..\..\..\files\project.xml must be modified.
     */
    static void BuildXml()
    {
      string projectFileName = @"..\..\..\files\project.xml";

      ConsoleLogger logger = new ConsoleLogger();

      Dictionary<string, string> GlobalProperty = new Dictionary<string, string>();
      BuildRequestData BuildRequest = new BuildRequestData(projectFileName, GlobalProperty, null, new string[] { "Rebuild" }, null);
      BuildParameters bp = new BuildParameters();
      bp.Loggers = new List<ILogger> { logger };

      BuildResult buildResult = BuildManager.DefaultBuildManager.Build(bp, BuildRequest);

      Console.WriteLine();
    }

    static void Main(string[] args)
    {
      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.BackgroundColor = ConsoleColor.White;
      Console.Write("\n  Building Builder.csproj ");
      Console.Write("\n =========================\n\n");
      Console.ResetColor();
      try
      {
        BuildCsproj();
      } catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.BackgroundColor = ConsoleColor.White;
        Console.Write("\n\n  An error occured while trying to build the csproj file.\n  Details: {0}\n\n", ex.Message);
        Console.ResetColor();
      }

      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.BackgroundColor = ConsoleColor.White;
      Console.Write("\n  Building project.xml ");
      Console.Write("\n ======================\n\n");
      Console.ResetColor();
      try
      {
        BuildXml();
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.BackgroundColor = ConsoleColor.White;
        Console.Write("\n\n  An error occured while trying to build the xml file.\n  Details: {0}\n\n", ex.Message);
        Console.ResetColor();
      }

      Console.ReadLine();
    }
  }
}
