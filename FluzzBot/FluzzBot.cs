
using FluzzBotCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
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
        private NetworkStream _networkStream;
        private  bool _isRunning;
        private string _messagePrefix;
        public  bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public Setlist SongList { get; private set; }
        private JustDanceSetlist _jdSetlist;

        internal JustDanceSetlist JustDanceSetlist
        {
            get { return _jdSetlist; }
            set { _jdSetlist = value; }
        }


        private List<ICommand> ValidCommands;
        public List<ICommand> Commands { get => ValidCommands; set { } }


        public string CurrentMessage;
        private Credentials _channelCredentials;
        public Credentials Credentials { get => _channelCredentials; set => throw new NotImplementedException(); }


        public FluzzBot(Credentials c)
        {
            _channelCredentials = c;
        }


        internal void Start()
        {
            ValidCommands = new List<ICommand>();
            var type = typeof(ICommand);
            foreach (var command in (AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(s => s.GetTypes())
    .Where(p => type.IsAssignableFrom(p) && p.IsClass)))
            {
                Console.WriteLine("Adding {0} to commands",command);
                ValidCommands.Add(Activator.CreateInstance(command) as ICommand);
            }

            _serverAddress = "irc.chat.twitch.tv";
            _serverPort = 6667;
            _messagesToSend = new Queue<string>();
            JustDanceSetlist = new JustDanceSetlist(this);
            SongList = new Setlist(this);
            JustDanceSetlist.LoadSetlistFromDatabase();
            try
            {
                Console.WriteLine("Starting FluzzBot");
                ConnectToTwitch();
                LoginToTwitch();




                writeThread = new Thread(() => ChatMessageSendThread(_chatWriter));
                readThread = new Thread(() => ChatMessageRecievedThread(_chatReader, _networkStream));
                writeThread.Start();
                readThread.Start();
                writeThread.Join();
                readThread.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bot Threw An Exception And Cannot Recovery");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                throw ex;
            }

        }

        private void LoginToTwitch()
        {

            try
            {
                 _networkStream = IRCSocket.GetStream();             
                    _chatReader = new StreamReader(_networkStream);
                    _chatWriter = new StreamWriter(_networkStream);

                    String loginMessage = "PASS " + _channelCredentials.Password;
                    loginMessage += Environment.NewLine + "NICK " + _channelCredentials.Username.ToLower();

                    EnqueueMessage(loginMessage);


                    _isRunning = true;


                SongList.LoadSetlistFromDatabase();



                

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
                {
                    Thread.Sleep(1);
                    continue;
                }

                buffer = chatReader.ReadLine();
                if(buffer.Split(' ')[1] == "001")
                {
                    WriteToStream("JOIN #" + _channelCredentials.ChannelName.ToLower());
                    _messagePrefix = "PRIVMSG #" + _channelCredentials.ChannelName.ToLower() + " :";
                }

                if (buffer.Contains("PING :"))
                {
                    
                    WriteToStream("PONG :tmi.twitch.tv");
                }

                //Actual Chat Messages
                if(buffer.Contains("PRIVMSG #" + _channelCredentials.ChannelName.ToLower() + " :"))
                {
                    string substringKey = "#" + _channelCredentials.ChannelName.ToLower() + " :";
                    string twitchInfo = buffer.Substring(0, buffer.IndexOf(substringKey));
                    string userStrippedMsg = buffer.Substring(buffer.IndexOf(substringKey) + substringKey.Length);

#if DEBUG
            global::System.Console.WriteLine(buffer);
#endif
                    CurrentMessage = buffer;

                    foreach (ICommand command in ValidCommands)
                    {

                        if (command.CommandName == null)
                            continue;
                        if(userStrippedMsg.StartsWith(command.CommandName))
                        {

                            if(!(twitchInfo.Contains("@badges=broadcaster") || twitchInfo.Contains(";mod=1")) && command.RequireMod)
                            { 
                                        continue;
                            }

                            command.Execute(this, userStrippedMsg);
                        }
                    }

                    if (twitchInfo.Contains(";bits="))
                    {
                        BitsCheerCommand b = new BitsCheerCommand();
                        b.Execute(this, userStrippedMsg);
                    }

                }

            }
        }

        private void ChatMessageSendThread(StreamWriter chatWriter)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            if(_messagesToSend == null)
                _messagesToSend = new Queue<string>();

            //EnqueueMessage("PRIVMSG #" + _channelCredentials.ChannelName.ToLower() + " :FluzzBot Online! kadWave");
            EnqueueMessage("CAP REQ :twitch.tv/commands");
            EnqueueMessage("CAP REQ :twitch.tv/tags");


            ConstructAndEnqueueMessage("/mods");
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

                            if (command == "PART #" + _channelCredentials.ChannelName)
                            {
                                _isRunning = false;
                            }

                            timer.Stop();
                            timer.Reset();
                            timer.Start();
                        }
                    }

                }
                Thread.Sleep(10);
            }
            Console.WriteLine("MessageThread Ended");
        }

        private void ConnectToTwitch()
        {
            IRCSocket = new TcpClient();
            try
            {
                var task = IRCSocket.ConnectAsync(_serverAddress, _serverPort);
                task.Wait();
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

        public void ConstructAndEnqueueMessage(String message)
        {
           message = message.Insert(0, "PRIVMSG #" + _channelCredentials.ChannelName.ToLower() + " :");
            EnqueueMessage(message);
        }



        public static class ReflectiveEnumerator
        {
            static ReflectiveEnumerator() { }

            public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
            {
                List<T> objects = new List<T>();
                foreach (Type type in
                    Assembly.GetAssembly(typeof(T)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    objects.Add((T)Activator.CreateInstance(type, constructorArgs));
                }
                objects.Sort();
                return objects;
            }
        }
    }
}
