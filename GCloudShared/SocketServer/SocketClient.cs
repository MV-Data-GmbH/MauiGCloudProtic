using GCloudShared.WebShopDto;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using GCloudShared.Shared;


namespace GCloudShared.SocketServer
{
    public class SocketClient : ISocketClient
    {
        public bool SocketConnectionOK { get; set; }

        public string SendMessage(string ServerIP, int ServerPort, string MessageText, int SendTO, int RecTO, OrderKuechenDisplay display)
        {
            string returndata = "ConnectionError";
            StateObject st = new StateObject();
            //if (ServerConnect(ServerIP, ServerPort) != "ServerOK")
            //{
            //    SocketConnectionOK = false;
            //    return returndata;
            //}
            //else
            //{
            //    SocketConnectionOK = true;
            //}
            try
            {
                var res = CheckConnectionToServer(ServerIP, ServerPort);
                if (res != "ServerOK")
                {
                    SocketConnectionOK = false;
                    return returndata;
                }
                else
                {
                    SocketConnectionOK = true;
                }

            }
            catch (SocketException ex)
            {
                var d = ex.Message;
                SocketConnectionOK = false;
                return "NoConnectionPossible";
            }
            catch (Exception ex)
            {
                var e = ex.Message;
                return "FunctionError";
            }
            try
            {

                // var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddr = IPAddress.Parse(ServerIP);
                var remoteEP = new IPEndPoint(ipAddr, ServerPort);
                using (var sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        sender.Connect(remoteEP);
                        sender.SendTimeout = SendTO;
                        sender.ReceiveTimeout = RecTO;

                        var msg = "";
                        var order = JsonConvert.SerializeObject(display);

                        var compressed = CompressString.Compress(order);
                        
                        var stringForsend = AESCrypt.EncryptString("b14ca5898a4e4133bbce2ea2315a1916", compressed);
                       

                        msg = $"InsertNewWebOrder;{stringForsend};<EOF>";

                        //var Commpresmessage = CompressString.Compress(msg);
                        var forSend = Encoding.UTF32.GetBytes(msg);
                        int bytesSent = sender.Send(forSend);

                        int bytesRec = 0;
                        returndata = "";
                        string ret_data = "";
                        Thread.Sleep(500);
                        while (!returndata.Contains("<EOF>"))
                        {
                            //Thread.Sleep(200);
                            bytesRec = sender.Receive(st.buffer);
                            ret_data = Encoding.UTF32.GetString(st.buffer, 0, bytesRec);
                            returndata = returndata + ret_data;
                        }

                        //returndata = ExtensionClass.GetData(returndata);
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                    }
                    catch (ArgumentNullException ane)
                    {
                        var s = ane.Message;
                        returndata = "WrongData";
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.TimedOut)
                        {
                            returndata = "ReadTimeOut";
                            SocketConnectionOK = false;
                        }
                        else if (se.SocketErrorCode == SocketError.ConnectionRefused | se.SocketErrorCode == SocketError.HostDown | se.SocketErrorCode == SocketError.HostNotFound | se.SocketErrorCode == SocketError.HostUnreachable)
                        {
                            returndata = "NoConnectionPossible";
                            SocketConnectionOK = false;
                        }
                        else
                        {
                            var m = se.Message;
                            returndata = "SocketError";
                            SocketConnectionOK = false;
                        }
                    }
                    catch (Exception e)
                    {
                        var s = e.Message;
                        returndata = "FunctionError";
                        SocketConnectionOK = false;
                    }
                    if (sender is not null)
                    {
                        sender.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var d = ex.Message;
                returndata = "ReadError";
            }

            return returndata;
        }
        public string CheckConnectionToServer(string ServerIP, int ServerPort)
        {
            var result = ServerConnect(ServerIP, ServerPort);
            if (result != "ServerOK")
            {

                return result;
            }
            return result;
        }
        public static string ServerConnect(string ServerIP, int ServerPort)
        {
            string returndata = "ConnectionError";

            var ipAddr = IPAddress.Parse(ServerIP);
            var remoteEP = new IPEndPoint(ipAddr, ServerPort);

            using (var sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                var connectCompleted = false;

                Thread connectThread = new Thread(() =>
                {
                    try
                    {
                        sender.Connect(remoteEP);
                        connectCompleted = true;
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.ConnectionRefused || se.SocketErrorCode == SocketError.HostDown || se.SocketErrorCode == SocketError.HostNotFound || se.SocketErrorCode == SocketError.HostUnreachable)
                        {
                            returndata = "NoConnectionPossible";
                        }
                        else
                        {
                            returndata = "SocketError";
                        }
                    }
                    catch (Exception)
                    {
                        returndata = "FunctionError";
                    }
                });

                connectThread.Start();
                connectThread.Join(1000);

                if (connectCompleted)
                {
                    // The connection task completed, check the result
                    if (sender.Connected)
                    {
                        returndata = "ServerOK";
                    }
                    else
                    {
                        returndata = "ConnectionFailed";
                    }
                }
                else
                {
                    // The connection task did not complete in time, indicating a timeout
                    returndata = "TimeoutError";
                }
            }

            return returndata;




        }
        //public static string ServerConnect(string ServerIP, int ServerPort)
        //{
        //    string returndata = "ConnectionError";
        //    var bytes = new byte[200000];


        //    try
        //    {

        //        //  var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        //        var ipAddr = IPAddress.Parse(ServerIP);
        //        var remoteEP = new IPEndPoint(ipAddr, ServerPort);
        //        using (var sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        //        {
        //            try
        //            {
        //                sender.SendTimeout = 1000;
        //                sender.ReceiveTimeout = 1000;
        //                sender.Connect(remoteEP);
        //                if (sender.Connected)
        //                {
        //                    returndata = "ServerOK";
        //                    sender.Shutdown(SocketShutdown.Both);
        //                    sender.Close();
        //                }
        //            }
        //            catch (ArgumentNullException ane)
        //            {
        //                var d = ane.Message;
        //                returndata = "WrongData";
        //            }
        //            catch (SocketException se)
        //            {
        //                var msges = se.Message;
        //                if (se.SocketErrorCode == SocketError.TimedOut)
        //                {
        //                    returndata = "ReadTimeOut";
        //                }
        //                else if (se.SocketErrorCode == SocketError.ConnectionRefused | se.SocketErrorCode == SocketError.HostDown | se.SocketErrorCode == SocketError.HostNotFound | se.SocketErrorCode == SocketError.HostUnreachable)
        //                {
        //                    returndata = "NoConnectionPossible";
        //                }
        //                else
        //                {
        //                    returndata = "SocketError";
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                var d=e.Message;
        //                returndata = "FunctionError";
        //            }
        //            if (sender is not null)
        //            {
        //                sender.Dispose();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var d = ex.Message;
        //        returndata = "ReadError";
        //    }


        //    return returndata;
        //}
    }
}
