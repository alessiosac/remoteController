using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace Client
{
    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public int BufferSize = 4;
        //number of bytes received
        public int received = 0;
        // Receive buffer.
        public byte[] buffer;

        public StateObject()
        {
            buffer = new byte[BufferSize];
        }
    }

    public delegate void Handler (JsonObject jsObj, Socket sock);
    public delegate void Handl(Socket sock);
    public class SocketHelper
    {
        public event Handler Response;
        public static event Handl End;
        // The port number for the remote device.
        private const int port = 27016;

        // The response from the remote device.
        //private static String response = String.Empty;
        private static List<IPAddress> serverConnected = new List<IPAddress>();

        public static Socket Connect(string str)
        {
            IPAddress ipAddr = null;
            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission 
                    TransportType.Tcp,        // Defines transport types 
                    "",                       // Gets the IP addresses 
                    SocketPermission.AllPorts // All ports 
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance           
                //IPHostEntry ipHost = Dns.GetHostEntry("");
                IPHostEntry ipHost = Dns.GetHostEntry(str);  //o Dns.Resolve("host.contoso.com");
                //IPHostEntry ipHost = Dns.GetHostEntry("localhost");  //o Dns.Resolve("host.contoso.com");

                // Gets first IP address associated with a localhost 
                ipAddr = ipHost.AddressList[0];

                //check if alredy connected
                if (serverConnected.Contains(ipAddr))
                {
                    throw new Exception("Impossible to connect to server already connected");
                }
                else
                {
                    serverConnected.Add(ipAddr);
                }

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

                // Create one Socket object to setup Tcp connection 
                Socket senderSock = new Socket(
                    ipAddr.AddressFamily,// Specifies the addressing scheme 
                    SocketType.Stream,   // The type of socket  
                    ProtocolType.Tcp     // Specifies the protocols  
                    );

                senderSock.NoDelay = false;   // Using the Nagle algorithm,serve??? 

                // Establishes a connection to a remote host 
                senderSock.Connect(ipEndPoint);

                //MessageBox.Show("Client connected to : " + ipEndPoint, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                return senderSock;
            }
            catch (Exception ex)
            {
                if (ex.Message != "Impossible to connect to server already connected" && ipAddr != null)
                {
                    removeFromList(ipAddr);
                }
                throw;
            }    
        }

        public static T ReceiveImmediatly<T>(Socket client)
        {
            //read int 
            byte[] size = ReceiveExactly(client, 4);
            int bufferSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(size, 0));

            byte[] bytes = ReceiveExactly(client, bufferSize);
            string data = Encoding.UTF8.GetString(bytes, 0, bufferSize);
            System.Console.WriteLine("Data: " + data);

            return JsonHelper.JSONDeserialize<T>(data);
        }

        private static byte[] ReceiveExactly(Socket handler, int length)
        {
            byte[] buffer = new byte[length];
            var receivedLength = 0;
            while (receivedLength < length)
            {
                //If the remote host shuts down the Socket connection with the Shutdown method, and all available data has been received, the Receive method will complete immediately and return zero bytes
                //oppure lancia eccezione???
                //ObjectDisposedException--->The Socket has been closed.
                var nextLength = handler.Receive(buffer, receivedLength, length - receivedLength, SocketFlags.None);
                if (nextLength == 0)
                {
                    //The socket's never going to receive more data. Server disconnected    non va bene gestisco in altro modo
                    Disconnect(handler);      
                    //call event, especially removeServer
                    End(handler);
                }
                receivedLength += nextLength;
            }
            return buffer;
        }

        public void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, state.BufferSize, 0, new AsyncCallback(MessageLengthReceived), state);
            }
            catch (Exception e)
            {
                Console.WriteLine("ciao"+e.ToString());
            }
        }       
        
        private void MessageLengthReceived(IAsyncResult ar)
        {
            // Retrieve the state object and the client socket 
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            try
            {
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);      //se socket chiuso me ne accorgo qui
                if (bytesRead > 0)
                {
                    if (bytesRead + state.received < state.BufferSize)
                    {
                        //get the rest of length
                        state.received += bytesRead;
                        client.BeginReceive(state.buffer, state.received, state.BufferSize - state.received, 0, new AsyncCallback(MessageLengthReceived), state);
                    }
                    else
                    {
                        //Read length, now get the rest of the data.
                        state.received = 0;

                        state.BufferSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(state.buffer, 0));

                        //state.BufferSize = Int32.Parse(Encoding.UTF8.GetString(state.buffer, 0, state.BufferSize));       questo se dovessi leggere una stringa
                        state.buffer = new byte[state.BufferSize];

                        client.BeginReceive(state.buffer, 0, state.BufferSize, 0, new AsyncCallback(MessageReceived), state);
                    }
                }
                else
                {
                    // All the data has arrived; put it in response. Server disconnected
                    //Disconnect(client);
                    removeFromList(client);
                    //call event, especially removeServer                    
                    End(client);
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("SokcetException in MessageLengthReceived");
                removeFromList(client);
                //call event, especially removeServer
                End(client);
            }
            catch (ObjectDisposedException)
            {
                MessageBox.Show("ObjectDisposedException in MessageLengthReceived");
                removeFromList(client);
            }
            catch (Exception ex)
            {
                //possibile dato perso, posso non fare niente
                MessageBox.Show("eccomi:" + ex.ToString());
            }
        }

        private void MessageReceived(IAsyncResult ar)
        {
            // Retrieve the state object and the client socket 
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            try
            {
                                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);      //se socket chiuso me ne accorgo qui
                if (bytesRead > 0)
                {
                    if (bytesRead + state.received < state.BufferSize)
                    {
                        //get the rest of message
                        state.received += bytesRead;
                        client.BeginReceive(state.buffer, state.received, state.BufferSize - state.received, 0, new AsyncCallback(MessageReceived), state);
                    }
                    else
                    {
                        string str = Encoding.UTF8.GetString(state.buffer, 0, state.BufferSize);

                        JsonObject obj = JsonHelper.JSONDeserialize<JsonObject>(str);

                        //call event, esepccialy updateInfo in TaskManager
                        Response(obj, state.workSocket);

                        //always ready to receive notifications
                        state.received = 0;
                        state.BufferSize = 4;
                        state.buffer = new byte[state.BufferSize];

                        client.BeginReceive(state.buffer, 0, state.BufferSize, 0, new AsyncCallback(MessageLengthReceived), state);
                    }
                }
                else
                {
                    //socket chiuso
                    //Disconnect(client);
                    removeFromList(client);
                    //call event, especially removeServer
                    End(client);
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("SokcetException in MessageReceived");
                removeFromList(client);
                //call event, especially removeServer               
                End(client);
            }
            catch (ObjectDisposedException)
            {
                MessageBox.Show("ObjectDisposedException in MessageReceived");
                removeFromList(client);
            }
            catch (Exception ex)
            {
                //possibile dato perso, posso non fare niente
                MessageBox.Show("Sono qui"+ex.ToString());
            }
        }
        
        public static void Send(Socket client, CommandObject data)
        {
            string str = JsonHelper.JSONSerilaize<CommandObject>(data);
            //send also number of byte of the json string, it must be 4 byte-long
            int numberBytes = Encoding.UTF8.GetByteCount(str);
            byte[] intBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(numberBytes));
            
            int byteSend = client.Send(intBytes);

            byte[] msg = Encoding.UTF8.GetBytes(str);
            // Sends data to a connected Socket. 
            //Send will block until all of the bytes in the buffer are sent
            //In nonblocking mode, Send may complete successfully even if it sends less than the number of bytes in the buffer.
            int bytesSend = client.Send(msg);
            
        }

        private static void removeFromList(Socket s)
        {
            IPEndPoint remoteIpEndPoint = s.RemoteEndPoint as IPEndPoint;
            serverConnected.Remove(remoteIpEndPoint.Address);
        }

        private static void removeFromList(IPAddress ip)
        {
            serverConnected.Remove(ip);
        }

        public static void Disconnect(Socket client)
        {
            client.Shutdown(SocketShutdown.Both);
            removeFromList(client);
            client.Close();
        }
    }
}
