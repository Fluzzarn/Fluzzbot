using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluzzBot
{
    public class FluzzBot
    {

        TcpClient IRCSocket;
        String _serverAddress;
        int _serverPort;
        private Thread writeThread;
        private Thread readThread;
        private Queue<String> _messagesToSend;


        private StreamReader _chatReader;
        private StreamWriter _chatWriter;
        private  bool _isRunning;
        private string _messagePrefix;
        public  bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }


        internal void Start()
        {
            _serverAddress = "irc.chat.twitch.tv";
            _serverPort = 6667;
            _messagesToSend = new Queue<string>();

            try
            {
                Console.WriteLine("Starting FluzzBot");
                ConnectToTwitch();
                LoginToTwitch();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bot Threw An Exception And Cannot Recovery");
                throw ex;
            }

        }

        private void LoginToTwitch()
        {

            try
            {
                var networkStream = IRCSocket.GetStream();             
                    _chatReader = new StreamReader(networkStream);
                    _chatWriter = new StreamWriter(networkStream);

                    String loginMessage = "PASS " + Credentials.Password;
                    loginMessage += Environment.NewLine + "NICK " + Credentials.Username.ToLower();

                    EnqueueMessage(loginMessage);


                    _isRunning = true;
                    
                    writeThread = new Thread(() => ChatMessageSendThread(_chatWriter));
                    readThread = new Thread(() => ChatMessageRecievedThread(_chatReader,networkStream));
                    writeThread.Start();
                    readThread.Start();

            }
            catch (Exception)
            {
                Console.WriteLine("Exception Thrown Authenticating With Twitch");

                throw;
            }
        }

        private void ChatMessageRecievedThread(StreamReader chatReader,NetworkStream ns)
        {
            String buffer;
            while (_isRunning)
            {
                if (!ns.DataAvailable)
                    continue;

                buffer = chatReader.ReadLine();
                if(buffer.Split(' ')[1] == "001")
                {
                    WriteToStream("JOIN #" + Credentials.ChannelName.ToLower());
                    _messagePrefix = "PRIVMSG #" + Credentials.ChannelName.ToLower() + " :";
                }

                if (buffer.Contains("PING :"))
                {
                    
                    WriteToStream("PONG :tmi.twitch.tv");
                }

                //Actual Chat Messages
                if(buffer.Contains("PRIVMSG #" + Credentials.ChannelName.ToLower() + " :"))
                {
                    string substringKey = "#" + Credentials.ChannelName.ToLower() + " :";
                    string userStrippedMsg = buffer.Substring(buffer.IndexOf(substringKey) + substringKey.Length);
                    Console.WriteLine(userStrippedMsg);
                    if(userStrippedMsg.StartsWith("!request"))
                    {
                        string song = userStrippedMsg.Substring(userStrippedMsg.IndexOf(' ') + 1);

                        EnqueueMessage(_messagePrefix + "Adding " + song + " to requests!");
                    }
                }

                Console.WriteLine(buffer);
            }
        }

        private void ChatMessageSendThread(StreamWriter chatWriter)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            if(_messagesToSend == null)
                _messagesToSend = new Queue<string>();

            EnqueueMessage("PRIVMSG #" + Credentials.ChannelName.ToLower() + " :FluzzBot Online!");
            while (_isRunning)
            {
                lock (_messagesToSend)
                {
                    if (_messagesToSend.Count > 0)
                    {
                        //1500 to make sure twitch doesn't time us out
                        if (timer.ElapsedMilliseconds > 1500)
                        {

                            string command = _messagesToSend.Dequeue();

                            //_chatWriter.Write(command);
                            //_chatWriter.Flush();
                            WriteToStream(command);

                            if (command == "PART #" + Credentials.ChannelName)
                            {
                                _isRunning = false;
                            }

                            timer.Stop();
                            timer.Reset();
                            timer.Start();
                        }
                    }

                }
            }
            Console.WriteLine("MessageThread Ended");
        }

        private void ConnectToTwitch()
        {
            IRCSocket = new TcpClient();
            try
            {
                IRCSocket.Connect(_serverAddress, _serverPort);
                if(!IRCSocket.Connected)
                {
                    throw new Exception("Tried to connect but couldnt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception was thrown while trying to connect to Twitch");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }



        public void EnqueueMessage(String message)
        {
            lock (_messagesToSend)
            {
                Console.WriteLine("Enqueuing " + message);
                _messagesToSend.Enqueue(message);
            }
        }


        private void WriteToStream(String message)
        {
            Console.WriteLine("Writing {0} to twitch",message);
            _chatWriter.WriteLine(message);
            _chatWriter.Flush();
        }
    }
}
