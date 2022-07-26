using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.Json;

namespace promiseProject
{
    class thePromise
    {
            public static IPAddress? theIP;
            public static HashSet<int> openPorts = new HashSet<int>();
            public static thePorts? lastScanThing;

            public static string? userInput;

        public static void Main(String[] args)
        {
            lastScanThing = readScan("lastScan");

            Console.WriteLine("Leave prompt blank to end the program."); //just useful info
            beginScan();

            Console.Write("\n Type in file name to save data. Leave blank to skip saving the data: ");
            var newFileName = Console.ReadLine();
            if(!String.IsNullOrWhiteSpace(newFileName) && !String.IsNullOrEmpty(userInput)) writeInfo(userInput, newFileName);
            
            userFeatures();
            
        }
      public static void beginScan()
        {
            Console.Write("Input link or IP to be scanned: ");
            
            #region openingInputStuff
                userInput = Console.ReadLine();
                if(userInput==null)
                    return;
                if(userInput.ToLower() == "end" || String.IsNullOrWhiteSpace(userInput)) //just end the program
                    return;
            #endregion 
            
            theIP= getIPAddress(userInput);
            if(theIP == null) 
            {
                Console.WriteLine("Your IP or URL has produced an error. Please use a valid URL or IP Address. \n");
                beginScan();
                return;
            } 
            //we now know that the IP Address provided is valid...
            Console.WriteLine("IP: {0} \n", theIP.ToString());

            Parallel.For(1, 2000, i => // 65355 is the magic number... to test set to 2000
            {
                new Thread(new ThreadStart(() =>
                {
                    portCanBeConnected(i);
                })).Start();
            });

            writeInfo(userInput, "lastScan"); //write the last scan information to the file
            

        }      
      
      //getIpAddress handles transforming URL to IP.
      public static IPAddress? getIPAddress(string input) 
      {     
        //returns the IP address of the given string.
         try
         {
            Uri accessedURI = new Uri(input);
            var ip = Dns.GetHostAddresses(accessedURI.Host)[0];
           return ip;
            //this attempts to convert a URL to an IP.
         }
         catch
         {
           IPAddress? IP;
           bool isIP = IPAddress.TryParse(input, out IP); 
           if(isIP)
            return IP; //it is an IP, and we can just return it!
        
            return null;
                
         }
      }      
      
      //portCanBeConnected takes in an integer and if that port ID can be connected, it adds the int to a hash set.
      static void portCanBeConnected(int portToConnectTo)
      {
        using(TcpClient client = new TcpClient())
        {
            if(theIP == null) return;
            var r = client.BeginConnect(theIP, portToConnectTo, null ,null);
            bool didConnect = r.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1)); //we dont want to wait forever to try to connect to a port! so just wait 1 second...
            if(didConnect)
                openPorts.Add(portToConnectTo); //add port number to openPorts set
            
        }
      }
      
      //writeInfo saves data gathered to the specified file.
      public static void writeInfo(string userIn, string fileName)
      {
        if(theIP == null) return;
        var path = Directory.GetCurrentDirectory() + "/" + fileName + ".json";
        using(StreamWriter write = new StreamWriter(path))
        {
            var whoIsAPort = new thePorts
            {
                firstInput = userIn,
                ipThing = theIP.ToString(),
                portId = openPorts.ToArray()
            };

            string makeItGoToJson = JsonSerializer.Serialize(whoIsAPort); //pack the information into a .json so its nice and easy to grab
            write.Write(makeItGoToJson);

            write.Close();
        }
      }

    //readLastScan gathers saved data from the specified file.
      public static thePorts? readScan(string fileName)
      {
        if(!fileDoesExist(fileName)) return null;
        var path = Directory.GetCurrentDirectory() + "/" + fileName + ".json";
        string scannie;
        using(StreamReader read = new StreamReader(path))
        {
            scannie = read.ReadToEnd();
            read.Close();
        }
        var scanned = JsonSerializer.Deserialize<thePorts>(scannie); //unpack all the information from .json!!
        return scanned;
        
      }
      
    public static void userFeatures()
    {
        Console.Write("\n \n Type 1 to enter compare mode. Type 2 to enter conversion mode. Leave empty to end application: ");
        var newMode = Console.ReadLine();

        switch(newMode)
        {
            case "":
                break;
    
            case "1":
                compareTwoFiles();
                break;
            
            case "2":
                askForConversion();
                break;
        }
    }
    public static void askForConversion()
    {
        Console.WriteLine("Enter a file name to begin conversion. Leave blank to return to mode selection: ");
        string? fileName = Console.ReadLine();
        if(String.IsNullOrEmpty(fileName) || !fileDoesExist(fileName))
        {
            userFeatures(); return;
        }
        Console.WriteLine("Writing a new text file... ");

        string path = Directory.GetCurrentDirectory() + "/" + fileName + ".txt";
        thePorts? info = readScan(fileName);
        using(StreamWriter write = new StreamWriter(path))
        {
            write.WriteLine("URL/IP: " + info!.firstInput);
            write.WriteLine("IP: " + info.ipThing);
            write.Write("\nOpen ports: ");
            foreach(int x in info.portId!)
            {    
             if(x == info.portId[info.portId.Count() - 1])
             {
                write.Write(x);
             }
             else write.Write("{0}, ", x);
            
            }
        }

    }
    public static bool fileDoesExist(string fileName)
    {
        string path = Directory.GetCurrentDirectory() + "/" + fileName + ".json";
        if(File.Exists(path)) return true;
        return false;
    }
    public static void compareTwoFiles()
    {
        Console.Write("\nWould you like to compare your previous scan to your latest one? Type 'y' for yes. ");
        string? result = Console.ReadLine();
        if(result!.ToLower().Equals("y"))
        {
            compareAgainstPreviousScan("lastScan");
        }
        else
        {
            Console.Write("\nWould you like to compare your previous scan or latest scan to another file? Enter 'p' for previous scan, and 'l' for latest scan: ");
            result = Console.ReadLine();
            if(result!.ToLower().Equals("p"))
            {
                Console.Write("\nEnter in file name: ");
                result = Console.ReadLine();
                if(fileDoesExist(result!)) compareAgainstPreviousScan(result!);
                return;
            }
            if(result!.ToLower().Equals("l"))
            {
                Console.Write("\nEnter in file name: ");
                result = Console.ReadLine();
                if(fileDoesExist(result!)) comparison("lastScan", result!);
                return;
            }

            Console.Write("\nEnter in first file name: ");
            result = Console.ReadLine();
            if(!fileDoesExist(result!))
            {
                userFeatures(); return;
            }
            Console.Write("\nEnter in second file to compare: ");
            string? result2 = Console.ReadLine();
            if(fileDoesExist(result2!))
            { 
                comparison(result!, result2!);
                return;
            }
            userFeatures();
        }

    }
    public static void comparison(string fileName1, string fileName2)
    {
        if(String.IsNullOrEmpty(fileName1) || String.IsNullOrEmpty(fileName2)) return;
        thePorts? ports1 = readScan(fileName1);
        thePorts? ports2 = readScan(fileName2);
        writeOutComparisonResults(ports1!, ports2!, fileName1, fileName2);

        userFeatures(); //loop back to user features
    }  
    public static void compareAgainstPreviousScan(string fileName)
    {
        if(String.IsNullOrEmpty(fileName)) return;
        thePorts? scann = readScan(fileName);
        writeOutComparisonResults(lastScanThing!, scann!, "Previous Scan", fileName);
        userFeatures();

    }

    public static void writeOutComparisonResults(thePorts ports1, thePorts ports2, string fileName1, string fileName2)
    {
         int difference = ports1!.portId!.Count()-ports2!.portId!.Count();

        Console.WriteLine("\n Original input for {0}: {1} \n Original input for {2}: {3}", fileName1, ports1.firstInput, fileName2, ports2.firstInput);
        Console.WriteLine("IP for {0}: {1}, \n IP for {2}: {3}", fileName1, ports1.ipThing, fileName2, ports2.ipThing);
        
        #region compareOpenPorts
        if(ports1.portId!.Count() > ports2.portId!.Count()) 
            Console.WriteLine("{0} has {1} more ports open than {2} ({0} has {3} ports open)", fileName1, difference, fileName2, ports1.portId!.Count());
        else if(ports1.portId!.Count() == ports2.portId!.Count()) 
            Console.WriteLine("{0} and {1} have the same amount of open ports ({2} open ports)", fileName1, fileName2, ports1.portId!.Count());
        else 
            Console.WriteLine("{0} has {1} less ports open than {2} (has {3} ports open)", fileName1, Math.Abs(difference), fileName2, ports2.portId!.Count());
        #endregion
    }
      //connect to port is still unused.
      public static void connectToPort(int port)
      {
        if(theIP == null) return;
        try
        {
            Socket sock = new Socket(theIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(theIP, port);
            Console.WriteLine("yeah.");
        }
        catch{ Console.WriteLine("No");}
      
      }

}

    public class thePorts
    {
        public string? firstInput{get;set;}
        public string? ipThing{get;set;}
        public int[]? portId{get;set;}

    }
}