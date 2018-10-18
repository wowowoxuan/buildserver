
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//MainWindows.xaml.cs:Client Server for proj#4                                                                               //
//Author:Chai,Weiheng,wchai01@syr.edu                                                                                        //
//Application:CSE681 Proj4-proj3gui                                                                                          //
//Environment:c# console                                                                                                     //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/* 
* Package Operations: 
* =================== 
*1.create GUI   2.show the files in the Repository   3.Create Build Request    4.send cmessage to the Mother Builder to start and close Child 
* Builder   5.send message to the Repository ask the Repository to send the Build Request to the Mother Builder.
*
* Public Interface
* ==================================================================
* public MainWindow()-----runs the test for the whole proj4, initialize the buttons, initialize the message dispatcher, and start a receiving thread.
* 
* All Other Functions used in this package
* ==================================================================
* void initializeMessageDispatcher()-----Define how to process each message command.
* void rcvThreadProc()-----Define Processing for the GUI's receive thread.
* void initializebutton()-----initialize the buttons that will be used.
* private void repotop_Click(object sender, RoutedEventArgs e)-----show all the files in the repofiles list box and show all the dirs in the repodirs
* list box.
* private void repodirs_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)-----show the files in the dir.
* private void start_Click(object sender, RoutedEventArgs e)-----send a start message command to the Mother Builder.
* private void Button_Click_4(object sender, RoutedEventArgs e)-----send file to the childprocess.
* private void send_Click(object sender, RoutedEventArgs e)-----send a message command to the Repository, ask the Repository to send the Build Request
* to the Mother Builder.
* private void build_Click(object sender, RoutedEventArgs e)-----create a Build Request according to the selected items,and send it to the Repository
* private void refreshrequest_Click(object sender, RoutedEventArgs e)-----refresh the List Box which shows the Build Requests.
* private void killprocess_Click(object sender, RoutedEventArgs e)-----send a message command to the Mother Builder to close the Child Builder.
* 
* 
* Required Files:
* =============================================
* Environment1
* requestbuilder
* wcfcom1
* 
* 
* Build command:
* =============================================
* csc{App.config,App,App.xaml.cs,MainWindow,MainWindow.xaml.cs}
* dnvenv proj4client.csproj
* 
* * Maintenance History: 
* -------------------- 
* ver 1.0 : 07/12/2017 
* - first release 

*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using HelpSession;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading;
using wcfcom1;
using Navigator;

namespace proj4gui
{
   

    public  partial class MainWindow : Window
    {
        Thread rcvThread = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        public MainWindow()
        {
            Console.WriteLine("=====================This is the Client============================");
            test();
            Console.WriteLine("\nwrite code in c#,meet requirement #1,all the send files and send,get messages using wcf,meet requirement#2");
            InitializeComponent();
            initializebutton();
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }
        void test()
        {
            Console.WriteLine("\n--------------------this is the test----------------------");
            Sender startsender = new Sender();                                       //here I run the test of the whole system
            startsender.CreateChannel("http://localhost:9999");
            string numchild = "3";                                                   //start child builder
            CommMessage startcommand = new CommMessage();               
            startcommand.Command = CommMessage.MessageCommand.startchild;
            startcommand.Source = "gui";
            startcommand.Destinaiton = "motherbuilder";
            startcommand.Body = numchild;
            startsender.PostMessage(startcommand);
            Console.WriteLine("\nstart 3 child builder");
            CommMessage testmsg = new CommMessage();                                  //send build request1
            testmsg.Command = CommMessage.MessageCommand.Sendbuildrequest;
            testmsg.Destinaiton = "repo";
            testmsg.Source = "gui";
            testmsg.Body = "test1.xml";
            Sender testsender = new Sender();
            testsender.CreateChannel("http://localhost:2017");
            testsender.PostMessage(testmsg);
            Console.WriteLine("\nsend the first BuildRequest for test");
            Thread.Sleep(500);
            testmsg.Command = CommMessage.MessageCommand.Sendbuildrequest;             //send build request2
            testmsg.Destinaiton = "repo";
            testmsg.Source = "gui";
            testmsg.Body = "test2.xml";
            testsender.PostMessage(testmsg);
            Console.WriteLine("\nsend the second BuildRequest for test");
            Thread.Sleep(500);
            testmsg.Command = CommMessage.MessageCommand.Sendbuildrequest;              //send build request3
            testmsg.Destinaiton = "repo";
            testmsg.Source = "gui";
            testmsg.Body = "test3.xml";
            testsender.PostMessage(testmsg);
            Console.WriteLine("\nsend the third BuildRequest for test");
            Thread.Sleep(5000);
            CommMessage stopcommand = new CommMessage();                                 //close child builder
            stopcommand.Command = CommMessage.MessageCommand.Sendbuildrequest;
            stopcommand.Source = "gui";
            stopcommand.Destinaiton = "motherbuilder";
            stopcommand.Body = "quit";
            stopcommand.Command = CommMessage.MessageCommand.CloseChildBuilder;
            testsender.CreateChannel("http://localhost:9999");
            testsender.PostMessage(stopcommand);
            testsender.PostMessage(stopcommand);
            testsender.PostMessage(stopcommand);
            Console.WriteLine("\n--------------------test end--------------------");//here the test end 
        }
        void initializeMessageDispatcher()
        {           
            messageDispatcher["getTopFiles"] = (CommMessage msg) =>// load repofiles listbox with name of the files from Repository
            {
                repofiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    repofiles.Items.Add(file);
                }
            };
            messageDispatcher["getBuildRequest"] = (CommMessage msg) =>//load buildrequestlist listbox with the name of the xml files from the Repository
            {
                buildrequestlist.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    if (file.Contains(".xml"))
                    {
                        buildrequestlist.Items.Add(file);
                    }
                }
            };           
            messageDispatcher["getTopDirs"] = (CommMessage msg) =>// load repodirs listbox with name of dirs from Repository
            {
                repodirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    
                    repodirs.Items.Add(dir);
                }
            };           
            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>// load repofiles listbox with name of files from Repository
            {
                repofiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    repofiles.Items.Add(file);
                }
            };
            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>  // load repodirs listbox with name of dirs from Repository
            {
                repodirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    repodirs.Items.Add(dir);
                }
            };
        }

        void rcvThreadProc()                  // pass the Dispatcher's action value to the main thread for execution
        {
            Console.Write("\n  starting client's receive thread");
            Receiver receiver = new Receiver();
            receiver.CreateHost("http://localhost:1234");
            while (true)
            {
                CommMessage msg = receiver.GetMessage();
               
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution
                Console.WriteLine("\nget list of files in the Repository");
                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }

        void initializebutton()
        {
            killprocess.IsEnabled = false;
            send.IsEnabled = false;
            
        }
        private void repotop_Click(object sender, RoutedEventArgs e)     //send message to Repository to get name of files and dirs in the Repository
        {                                                                //recv thread will create an Action<CommMessage> for the UI thread
            Sender sender1 = new Sender();                               //to invoke to load the repofiles list box
            sender1.CreateChannel("http://localhost:2017");
            CommMessage msg1=new CommMessage();
            msg1.Command = CommMessage.MessageCommand.getfiles;
            msg1.Source = "gui";
            msg1.Destinaiton = "repo";
            msg1.command= "getTopFiles";
            sender1.PostMessage(msg1);
            CommMessage msg2 = new CommMessage();
            msg2.Command = CommMessage.MessageCommand.getfiles;
            msg2.Source = "gui";
            msg2.Destinaiton = "repo";
            msg2.command = "getTopDirs";
            sender1.PostMessage(msg2);
        }
  

        private void repodirs_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {                                                                //send message to the Repository to get files  in the folder
            Sender sender2 = new Sender();                               
            sender2.CreateChannel("http://localhost:2017");
            CommMessage msg1 = new CommMessage();
            msg1.Command = CommMessage.MessageCommand.getfiles;
            msg1.Source = "gui";
            msg1.Destinaiton = "repo";
            msg1.command = "moveIntoFolderFiles";
            msg1.arguments.Add(repodirs.SelectedValue as string);
            sender2.PostMessage(msg1);
            CommMessage msg2 = new CommMessage();
            msg2.Command = CommMessage.MessageCommand.getfiles;
            msg2.Source = "gui";
            msg2.Destinaiton = "repo";
            msg2.command = "moveIntoFolderDirs";
            sender2.PostMessage(msg2);
        }

        private void start_Click(object sender, RoutedEventArgs e)//send a startchild command to the Mother Builder, the number of Child Builder
        {                                                         //that will be started is according to the number in the number Text Box.
            Sender startsender = new Sender();
            startsender.CreateChannel("http://localhost:9999");
            string numchild = number.Text;
            CommMessage msg = new CommMessage();
            msg.Command = CommMessage.MessageCommand.startchild;
            msg.Source = "gui";
            msg.Destinaiton = "motherbuilder";
            msg.Body = numchild;
            startsender.PostMessage(msg);                 
            start.IsEnabled = false;
            killprocess.IsEnabled = true;
            send.IsEnabled = true;
            repotop_Click(null, null);
        }
        private void build_Click(object sender, RoutedEventArgs e)// Create Build Request according to the repofiles' selected items.
        {
            string savePath = GuiEnvironment.guistorage;
            string fileName = xmlname.Text+".xml";
            if (!System.IO.Directory.Exists(savePath))
                System.IO.Directory.CreateDirectory(savePath);
            string fileSpec = System.IO.Path.Combine(savePath, fileName);
            fileSpec = System.IO.Path.GetFullPath(fileSpec);
            requestbuilder tr = new requestbuilder();
            tr.author = "Weiheng Chai";          
            foreach (var item in repofiles.SelectedItems)
            {
                string filename = item.ToString();
                if (filename.Contains("csproj"))
                {
                    tr.csproj = filename;
                }
                else if (filename.Contains("testdriver") && filename.Contains("cs"))
                {
                    tr.testDriver = filename;
                }
                else
                {
                    tr.testedFiles.Add(filename);
                }
                               
            }
            tr.makeRequest();
            tr.saveXml(fileSpec);
            string fullpath = System.IO.Path.GetFullPath(fileSpec);
            Sender sender3 = new Sender();
            sender3.CreateChannel("http://localhost:2017");
            sender3.postFile(fileName, savePath, 1024, RepoEnvironment.repostopath);
            refreshrequest_Click(null, null);
            string fullsavepath = Path.GetFullPath(savePath);
            string repofullpath = Path.GetFullPath(@"../../../repostorage");
            showpath.Text = "the Build Request:" + fileName + " has been stored at:" + fullsavepath + " and send to:" +repofullpath;
        }
        private void send_Click(object sender, RoutedEventArgs e)//send the Sendbuildrequest command to the Repsitory, when the Repository get
        {                                                        //this message, the Repository will send the Build Request to the Mother Builder
            foreach (var item in buildrequestlist.SelectedItems)
            {
                string brname = item.ToString();
                CommMessage msg = new CommMessage();
                msg.Command = CommMessage.MessageCommand.Sendbuildrequest;
                msg.Destinaiton = "repo";
                msg.Source = "gui";
                msg.Body = brname;
                Sender sender3 = new Sender();
                sender3.CreateChannel("http://localhost:2017");
                sender3.PostMessage(msg);
                Console.WriteLine("\nask the repository to send the build request to the mother builder to start build, the build request is:{0}",brname);
                Thread.Sleep(500);             //if do not sleep, the repository will crash and I do not have enough time to fix it......
            }
            
        }
        private void refreshrequest_Click(object sender, RoutedEventArgs e) //send message to the Repository to get .xml files in the Repository
        {
            Sender sender1 = new Sender();
            sender1.CreateChannel("http://localhost:2017");

            CommMessage msg1 = new CommMessage();
            msg1.Command = CommMessage.MessageCommand.getfiles;
            msg1.Source = "gui";
            msg1.Destinaiton = "repo";
            msg1.command = "getBuildRequest";
            sender1.PostMessage(msg1);
        }

        private void killprocess_Click(object sender, RoutedEventArgs e)//send a closechildbuilder command to the Mother Builder
        {
            Sender kilsebder = new Sender();
            kilsebder.CreateChannel("http://localhost:9999");
            CommMessage msg = new CommMessage();
            msg.Source = "gui";
            msg.Destinaiton = "motherbuilder";
            msg.Body = "quit";
            msg.Command = CommMessage.MessageCommand.CloseChildBuilder;
            int i = Int32.Parse(number.Text);
            for (int k = 0; k < i; k++)
            {
                kilsebder.PostMessage(msg);
            }
            start.IsEnabled = true;
            killprocess.IsEnabled = false;
            send.IsEnabled = false;
            Console.WriteLine("\n send message to the mother builder to close the child builder");
        }
    }
}

