
using FluzzBot.Commands;
using FluzzBot.Markov;
using FluzzBotCore;
using MySql.Data.MySqlClient;
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



        private Dictionary<string, string> _markovTextDict;
        public Dictionary<string, string> MarkovText { get =>  _markovTextDict; set { } }

        private Dictionary<string, JustDanceSetlist> _justDanceDict;
        public Dictionary<string, JustDanceSetlist> JustDanceDict { get => _justDanceDict; set { } }
        TcpClient IRCSocket;
        String _serverAddress;
        int _serverPort;
        private Thread writeThread;
        private Thread readThread;
        private Queue<String> _messagesToSend;


        string[] _bannedList;
        private StreamReader _chatReader;
        private StreamWriter _chatWriter;
        private NetworkStream _networkStream;
        private  bool _isRunning;
        public  bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public Setlist SongList { get; private set; }



        private List<ICommand> ValidCommands;
        public List<ICommand> Commands { get => ValidCommands; set { } }


        public string CurrentMessage;
        private Credentials _channelCredentials;


        //User, List of commands
        private Dictionary<string, List<string>> _removedCommandsDict;
        public Dictionary<string, List<string>> RemovedCommands { get => _removedCommandsDict; set { } }

        //User, List of commands
        private Dictionary<string, List<string>> _joinedUsersDict;
        public Dictionary<string, List<string>> JoinedUsersDict { get => _joinedUsersDict; set { } }

        //User, List of commands
        private Dictionary<string, List<string>> _timedOutUsersDict;
        public Dictionary<string, List<string>> TimedOutUsersDict { get => _timedOutUsersDict; set { } }

        public FluzzBot(Credentials c)
        {
            _channelCredentials = c;
            _markovTextDict = new Dictionary<string, string>();
            _justDanceDict = new Dictionary<string, JustDanceSetlist>();
            _removedCommandsDict = new Dictionary<string, List<string>>();
            _joinedUsersDict = new Dictionary<string, List<string>>();
            _timedOutUsersDict = new Dictionary<string, List<string>>();
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
                var file = File.ReadAllLines("./users.txt").ToList();
                foreach (var channel in file)
                {
                    StartMarkov(channel);
                LoadBannedCommands(channel);
                _timedOutUsersDict[channel] = new List<string>();
                _joinedUsersDict[channel] = new List<string>();
                    JustDanceDict[channel] = new JustDanceSetlist(this);
                    JustDanceDict[channel].LoadSetlistFromDatabase(channel);
                }

            

            _bannedList = File.ReadAllLines("bannedWords.txt");
            _serverAddress = "irc.chat.twitch.tv";
            _serverPort = 6667;
            _messagesToSend = new Queue<string>();
            SongList = new Setlist(this);
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

        private void LoadBannedCommands(string channel)
        {
            _removedCommandsDict[channel] = new List<string>();
            string queury = "SELECT * FROM banned_commands WHERE user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username)";

            MySqlDataReader dataReader = MySQLHelper.GetSQLDataFromDatabase(queury, new Dictionary<string, string>() { { "@username", channel } }, out MySqlConnection conn);
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    _removedCommandsDict[channel].Add((string)dataReader["command"]);
                    Console.WriteLine("Adding {0} to banned commands for user {1}", (string)dataReader["command"],channel);
                }
                dataReader.Close();
            }
        }

        private void LoginToTwitch()
        {

            try
            {
                 _networkStream = IRCSocket.GetStream();
                var utf8withoutBOM = new UTF8Encoding(false);
                    _chatReader = new StreamReader(_networkStream,utf8withoutBOM);
                    _chatWriter = new StreamWriter(_networkStream, utf8withoutBOM);


                    _chatWriter.Flush();
                    
                    String loginMessage = "PASS " + _channelCredentials.Password;
                    loginMessage += Environment.NewLine + "NICK " + _channelCredentials.Username.ToLower();

                    EnqueueMessage(loginMessage);


                    _isRunning = true;


                //SongList.LoadSetlistFromDatabase();



                

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

#if DEBUG
                global::System.Console.WriteLine(buffer);
#endif
                if (buffer.Split(' ')[1] == "001")
                {
                    var file = File.ReadAllLines("./users.txt").ToList();

                    foreach (var channel in file)
                    {
                        WriteToStream("JOIN #" + channel.ToLower());
                    }

                }

                if(buffer.Contains(".tmi.twitch.tv JOIN #"))
                {
                    string channelName = buffer.Split('#')[1];
                    string username = buffer.Split('!')[0].Substring(1);
                    _joinedUsersDict[channelName].Add(username);
                }
                else if (buffer.Contains(".tmi.twitch.tv PART #"))
                {
                    string channelName = buffer.Split('#')[1];
                    string username = buffer.Split('!')[0].Substring(1);
                    _joinedUsersDict[channelName].RemoveAll((x) => x ==username);
                }
                else if (buffer.Contains(":tmi.twitch.tv CLEARCHAT #"))
                {
                    string channelName = buffer.Split('#')[1].Split(' ')[0];
                    string username = buffer.Split(':')[2];
                    _timedOutUsersDict[channelName].Add(username);

                    if (buffer.Contains("@ban-duration="))
                    {
                        int startIndex = buffer.IndexOf('=') + 1;
                        int endIndex = buffer.IndexOf(';');
                        string time = buffer.Substring(startIndex, endIndex - startIndex);

                        if (int.TryParse(time, out int parsedTime))
                        {
                            Thread t = new Thread(() => { Thread.Sleep(3600 * 1000); _timedOutUsersDict[channelName].Remove(username); });
                            t.Start();
                        }
                    }
                }

                if (buffer.Contains("PING :"))
                {
                    
                    WriteToStream("PONG :tmi.twitch.tv");

                }
                else if(buffer.Contains("PRIVMSG #"))
                {


                    string username = buffer.Substring(buffer.IndexOf("PRIVMSG #"));
                    username = username.Substring(9, username.IndexOf(':') - 10);
                    string substringKey = "#" + username + " :";
                    string twitchInfo = buffer.Substring(0, buffer.IndexOf(substringKey));
                    string userStrippedMsg = buffer.Substring(buffer.IndexOf(substringKey) + substringKey.Length);


                    CurrentMessage = buffer;

                    foreach (ICommand command in ValidCommands)
                    {

                        if (command.CommandName == null)
                            continue;
                        if (userStrippedMsg.Split(' ')[0].ToLower() == (command.CommandName).ToLower())
                        {
                            bool isSuperUser = false;

                            if (command.RequireMod)
                            {
                                if (!(twitchInfo.Contains("@badges=broadcaster") || twitchInfo.Contains(";mod=1")))
                                {
                                    if (!twitchInfo.Contains(";display-name=Fluzzarn;"))
                                        continue;
                                    else
                                        isSuperUser = true;
                                }
                                else
                                {
                                    isSuperUser = true;
                                }

                            }

                            if (twitchInfo.Contains(";display-name=Fluzzarn;"))
                            {
                                isSuperUser = true;
                            }

                            try
                            {
                                Command c = command as Command;
                                if (isSuperUser)
                                {
                                    command.Execute(this, userStrippedMsg, username);
                                }
                                else
                                {
                                    if (command.AutoFire)
                                    {
                                        continue;
                                    }
                                    else if (c.PreExecute(this, username))
                                    {
                                        command.Execute(this, userStrippedMsg, username);

                                    }

                                }

                            }
                            catch (Exception ex)
                            {

                                Console.WriteLine(ex.Message);
                                Console.WriteLine(ex.StackTrace);
                                ConstructAndEnqueueMessage("Go and literally @ me in discord because the bot threw an exception trying to do a command", username);
                            }

                        }
                    }

                    if (twitchInfo.Contains(";bits="))
                    {
                        BitsCheerCommand b = new BitsCheerCommand();
                        b.Execute(this, userStrippedMsg, username);
                    }

                    Thread markovThread = new Thread(() => AddToMarkovText(buffer, username, userStrippedMsg));
                    markovThread.Start();

                }
                else if(buffer.Contains("USERNOTICE"))
                {

                    //string username = buffer.Substring(buffer.IndexOf("USERNOTICE #"));
                    //username = username.Substring("USERNOTICE #".Length, username.IndexOf(':') - "USERNOTICE #".Length + 1);
                    //if (username == "misskaddykins")
                    //{
                    //
                    //    if (buffer.Contains("msg-param-sub-plan=2000"))
                    //        ConstructAndEnqueueMessage("kadWotInTarnation WELCOME TO THE KADDY SALOON kadWotInTarnation",username);
                    //    else
                    //    {
                    //        ConstructAndEnqueueMessage("kadHype WELCOME TO THE KADDY SHACK kadHype",username);
                    //
                    //    }
                    //    ConstructAndEnqueueMessage("kadHype kadHype kadHype kadHype kadHype kadHype kadHype kadHype kadHype kadHype ",username);
                    //}
                }


            }
        }

        private void AddToMarkovText(string buffer, string username, string userStrippedMsg)
        {
            Thread.Sleep(10000);
            lock (_markovTextDict)
            {



                if (!_removedCommandsDict[username].Contains("!markov"))
                {
                    if (!buffer.Contains(";display-name=nightbot;") && !buffer.Contains(";display-name=theroflbotr;") && !buffer.Contains(";bits="))
                    {
                        if (!buffer.Contains(";display-name=fluzzbot;"))
                        {
                            int startIndex = buffer.IndexOf(";display-name=");
                            if (!(startIndex < 0))
                            {
                                startIndex = startIndex + ";display-name=".Length;

                                int endIndex = buffer.IndexOf(';', startIndex);
                                string bannedUser = buffer.Substring(startIndex, endIndex - startIndex).ToLower();

                                if (!_timedOutUsersDict[username].Contains(bannedUser))
                                {

                                    string message = StripBannedWords(userStrippedMsg);
                                    _markovTextDict[username] += message + " ";
                                    {
                                        File.AppendAllText("./markov/" + username.ToLower() + ".txt", message + Environment.NewLine);

                                    }
                                }
                            }

                
                        }
                    }
                }

            }
        }

        private void StartMarkov(string channel)
        {
            Console.WriteLine("CREATING MARKOV DICT FOR {0}", channel);
            _markovTextDict.Add(channel.ToLower(), "");

            string filePath = "./markov/" + channel.ToLower() + ".txt";

            if (File.Exists("./markov/" + channel.ToLower() + ".json"))
            {
                ((MarkovChatCommand)Commands.Find((x) => x.CommandName == "!markov")).MakeTDict(this, channel);
            }
            else if (File.Exists(filePath))
            {
                int lines = File.ReadAllLines(filePath).ToList().Count;
                int counter = 0;
                foreach (var line in File.ReadAllLines(filePath).ToList())
                {
                    
                    _markovTextDict[channel.ToLower()] = line;
                    ((MarkovChatCommand)Commands.Find((x) => x.CommandName == "!markov")).MakeTDict(this, channel);
                    counter++;
                    Console.WriteLine("Line " + counter + " of " + lines );
                }
            }


            this.MarkovText[channel] = "";
            Console.WriteLine("DONE CREATING MARKOV DICT FOR CHANNEL {0}", channel);
        }

        private string StripBannedWords(string userStrippedMsg)
        {
            
            foreach (var word in _bannedList)
            {
                userStrippedMsg = userStrippedMsg.Replace(word, "CENSORED");
            }
            return userStrippedMsg;
        }

        private void ChatMessageSendThread(StreamWriter chatWriter)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            if(_messagesToSend == null)
                _messagesToSend = new Queue<string>();

            EnqueueMessage("CAP REQ :twitch.tv/commands");
            EnqueueMessage("CAP REQ :twitch.tv/tags");
            EnqueueMessage("CAP REQ :twitch.tv/membership");
            //ConstructAndEnqueueMessage("/mods");
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

        public void EnqueuMessage(String message, String username)
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

        public void ConstructAndEnqueueMessage(String message, String username)
        {
            message = message.Insert(0, "PRIVMSG #" + username.ToLower() + " :");
            message = message.Replace('@', ' ');

            byte[] utf8_bytes = Encoding.Default.GetBytes(message);
            //message = Encoding.UTF8.GetString(utf8_bytes);
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
