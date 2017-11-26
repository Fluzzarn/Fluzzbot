
using FluzzBot.Commands;
using FluzzBot.Markov;
using FluzzBotCore;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FluzzBot
{
    public class FluzzBot
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();


        private Dictionary<string, string> _markovTextDict;
        public Dictionary<string, string> MarkovText { get =>  _markovTextDict; set { } }

        private Dictionary<string, int> _spookDict;
        public Dictionary<string, int> Spookdict { get => _spookDict; set { } }

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


        List<Regex> filteredRegex;

        public FluzzBot(Credentials c)
        {
            _channelCredentials = c;
            _markovTextDict = new Dictionary<string, string>();
            _justDanceDict = new Dictionary<string, JustDanceSetlist>();
            _removedCommandsDict = new Dictionary<string, List<string>>();
            _joinedUsersDict = new Dictionary<string, List<string>>();
            _timedOutUsersDict = new Dictionary<string, List<string>>();
            _spookDict = new Dictionary<string, int>();
        }


        internal void Start()
        {
            ValidCommands = new List<ICommand>();
            var type = typeof(ICommand);
            foreach (var command in (AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(s => s.GetTypes())
    .Where(p => type.IsAssignableFrom(p) && p.IsClass)))
            {
                logger.Info("Adding {0} to commands",command);
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


            filteredRegex = new List<Regex>();
            if (File.Exists("regex.txt"))
            {
                foreach (var line in File.ReadAllLines("regex.txt"))
                {
                    logger.Info("Adding \"{0}\" for markov ignoring", line);
                    filteredRegex.Add(new Regex(line));
                }
            }
            _bannedList = File.ReadAllLines("bannedWords.txt");
            _serverAddress = "irc.chat.twitch.tv";
            _serverPort = 6667;
            _messagesToSend = new Queue<string>();
            SongList = new Setlist(this);
            try
            {
                logger.Info("Starting FluzzBot");
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
                logger.Error("Bot Threw An Exception And Cannot Recovery");
                logger.Error(ex.Message);
                logger.Error(ex.InnerException);
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
                    logger.Info("Adding {0} to banned commands for user {1}", (string)dataReader["command"],channel);
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

            }
            catch (Exception ex)
            {
                logger.Error("Exception Thrown Authenticating With Twitch");

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
                logger.Trace(buffer);
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

                        if (command.AutoFire)
                        {
                            Command c = command as Command;
                            if (c.PreExecute(this, username,command.AutoFire))
                            {
                                command.Execute(this, userStrippedMsg, username);
                            }
                            
                        }
                        else
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
                                        else if (c.PreExecute(this, username, command.AutoFire))
                                        {
                                            command.Execute(this, userStrippedMsg, username);

                                        }

                                    }

                                }
                                catch (Exception ex)
                                {

                                    logger.Error(ex.Message);
                                    logger.Error(ex.StackTrace);
                                    ConstructAndEnqueueMessage("Go and literally @Fluzzarn#4287 in Discord because the bot threw an exception trying to do a command", username);
                                }

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



                string loweredBuffer = buffer.ToLower();
                foreach (var reg in filteredRegex)
                {
                    if (reg.Match(loweredBuffer).Success)
                    {
                        logger.Info("{0} matched regex {1}, ignoring message", buffer, reg);
                        return;
                    }
                }
                if (!_removedCommandsDict[username].Contains("!markov"))
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
                            _markovTextDict[username] += message + '\n';
                            {
                                File.AppendAllText("./markov/" + username.ToLower() + ".txt", message + Environment.NewLine);

                            }
                        }
                    } 
                }

            }
        }

        private void StartMarkov(string channel)
        {
            logger.Info("CREATING MARKOV DICT FOR {0}", channel);
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
                    logger.Info("Line " + counter + " of " + lines );
                }
                ((MarkovChatCommand)Commands.Find((x) => x.CommandName == "!markov")).SaveTDict(channel);
            }


            this.MarkovText[channel] = "";
            logger.Info("DONE CREATING MARKOV DICT FOR CHANNEL {0}", channel);
        }

        private string StripBannedWords(string userStrippedMsg)
        {
            
            foreach (var word in _bannedList)
            {
                userStrippedMsg = userStrippedMsg.Replace(word, "CENSORED");
            }
            return userStrippedMsg;
        }

        internal void ReloadSettings()
        {
            throw new NotImplementedException();
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
                logger.Error("An exception was thrown while trying to connect to Twitch");
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw;
            }
        }



        private void EnqueueMessage(String message)
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
#if DEBUG
                Console.WriteLine("Enqueuing " + message);
#endif
                _messagesToSend.Enqueue(message);
            }
        }

        private void WriteToStream(String message)
        {
            logger.Trace("Writing {0} to twitch", message); 

            _chatWriter.WriteLine(message);
            _chatWriter.Flush();
        }


        public void ConstructAndEnqueueMessage(String message, String username)
        {
            message = message.Insert(0, "PRIVMSG #" + username.ToLower() + " :");
            message = message.Replace('@', ' ');

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
