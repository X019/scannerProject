using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace promiseProject
{
    class thePromise
    {
            public static IPAddress? theIP;
            public static HashSet<int> openPorts = new HashSet<int>();
        public static void Main(String[] args)
        {
            Console.WriteLine("Type 'end' to end the program."); //just useful info
            beginScan();
        }
      public static void beginScan()
        {
            Console.Write("Input link or IP to be scanned: ");
            
            #region openingInputStuff
                var userInput = Console.ReadLine();
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

            for(int i = 1; i <= 65355; i++)
            {
                new Thread(new ThreadStart(() =>
                {
                    portCanBeConnected(i);
                })).Start();
            }
            
            Console.WriteLine("OPEN PORTS: ");
            foreach(int x in openPorts)
            {
                Console.Write("{0} is open, ",x);
            }


        }      
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
      
      // this method (connectToPort) is unused as of yet.
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

}