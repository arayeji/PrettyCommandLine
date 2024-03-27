using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrettyCommandLine
{
    public  class CommandLineBind
    {
        public delegate void CommandLineHandler(object sender, CommandLineEventArgs e);
        public   event CommandLineHandler OnCommandLineInput;

        Modes CurrentMode;
        List<Commands> Commands = new List<Commands>();
        Dictionary<string, Modes> Modes = new Dictionary<string, Modes>();

        public void AddMode(Modes Mode)
        {
            if (Modes.Values.Where(x => x.isDefault).Any()) {
                Mode.isDefault = false;
            }
            if(!Modes.ContainsKey(Mode.Name))
            Modes.Add(Mode.Name, Mode);
            Commands ExitCommand = new Commands(CommandsBase.CommandTypes.Mode);
            ExitCommand.Command = Mode.Name;
            ExitCommand.Executable += delegate { CurrentMode = Mode; Console.ForegroundColor = CurrentMode.Color; };
            Commands.Add(ExitCommand);
        }
        public void AddCommand(Commands Command)
        {  
          Commands.Add(Command);
        }

        static bool Exiting = false;
        public void Start()
        {
            if (Modes.Count > 1)
            {
                var defaultmode = Modes.Values.Where(x => x.isDefault).First();
                if (defaultmode != null)
                {
                    CurrentMode = defaultmode;
                    Console.ForegroundColor = CurrentMode.Color;
                }
            }
            else if (Modes.Count > 0) { CurrentMode = Modes.Values.Where(x => x.isDefault).First(); }
            else CurrentMode = new Modes { Name = "", isDefault = true };

            Commands ExitCommand = new Commands(CommandsBase.CommandTypes.Command);
            ExitCommand.Command = "Exit";
            ExitCommand.Executable += delegate { Exiting = true; };

            Commands.Add(ExitCommand);

            OnCommandLineInput += Start_OnCommandLineInput;
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                if (!Exiting)
                {
                    e.Cancel = true;
                    Read();
                }
            };
            Console.Write(CurrentMode);
            Read();
        }
        private InputData ParseInput(string[] Inputs)
        {
            InputData inputData = new InputData();
            bool ParsingVariablesFinished = false;
            for(int i = 0; i < Inputs.Length;)
            {
                if (Inputs[i].StartsWith("--"))
                {
                    try
                    {
                        inputData.Variables.Add(new InputVariable { Name = Inputs[i], Value = Inputs[i + 1] });
                        i += 2;
                        ParsingVariablesFinished = true;
                    }
                    catch 
                    {
                         
                    }
                }
                else
                {
                    if(ParsingVariablesFinished)
                    {
                        break;
                    }
                    inputData.Commands.Add(Inputs[i]);
                    i++;
                }
            }
            return inputData;
        }
        private IEnumerable<string> ParseArguments(string commandLine)
        {
            var regex = new Regex(@"[^\s""]+|""[^""]*""");
            foreach (Match match in regex.Matches(commandLine))
            {
                yield return match.Value.Trim('"');
            }
        }
        
        private void Start_OnCommandLineInput(object sender, CommandLineEventArgs e)
        {
            if (e.Command != null) 
            {
                var variables = ParseArguments(e.Command).ToArray();
                var inputData = ParseInput(variables);
                Commands.ForEach(x => x.CheckAndExecute(inputData, CurrentMode));

                //Commands.ForEach(x => x.CheckAndExecute(variables, CurrentMode));
            }

            if (!Exiting)
            Read();
        }

        StringBuilder cmd = new StringBuilder();
        bool LastKeyEnter = false;
        List<string> CommandsHistory = new List< string>();
        int? historyindex ;
        int CursorLeftOffset = 0;
        public void Read()
        {
            try
            {


                if (LastKeyEnter)
                {
                    Console.Write(CurrentMode);
                }
                Console.CursorLeft = CurrentMode.ToString().Length + CursorLeftOffset;
                var key = Console.ReadKey();
                LastKeyEnter = false;

                switch (key.Key)
                {
                    case ConsoleKey.Delete:
                        if (cmd.Length > 0 && CursorLeftOffset < cmd.Length)
                        {
                            Console.Write(" ");
                            Console.CursorLeft = 0;
                            cmd.Remove(CursorLeftOffset, 1);
                            string newcmd = CurrentMode + cmd.ToString();
                            Console.Write(CurrentMode + cmd.ToString() + new string(' ', Console.WindowWidth - newcmd.Length));
                            Console.CursorLeft = newcmd.Length;
                        }

                        break;
                    case ConsoleKey.Backspace:


                        if (cmd.Length > 0 && CursorLeftOffset > 0)
                        {
                            Console.Write(" ");
                            Console.CursorLeft = 0;
                            cmd.Remove(CursorLeftOffset - 1, 1);
                            string newcmd = CurrentMode + cmd.ToString();
                            Console.Write(CurrentMode + cmd.ToString() + new string(' ', Console.WindowWidth - newcmd.Length));
                            Console.CursorLeft = newcmd.Length;
                            CursorLeftOffset--;
                        }

                        break;
                    case ConsoleKey.Enter:

                        Console.WriteLine();
                        string final = cmd.ToString();
                        cmd = new StringBuilder();
                        LastKeyEnter = true;
                        CursorLeftOffset = 0;
                        if (final.Length > 0)
                        {
                            if (!CommandsHistory.Contains(final))
                                CommandsHistory.Add( final);
                            else
                            {
                                CommandsHistory.Remove(final);
                                CommandsHistory.Add( final);
                            }
                            historyindex = null;
                        }
                        OnCommandLineInput?.Invoke("Start", new CommandLineEventArgs { Command = final });

                        break;
                
                    case ConsoleKey.Tab: 
                        var similar = Commands.Where(c => c.CheckMode(CurrentMode)).SelectMany(x => x.GetFullPathAndVariables().Where(c => c.TrimStart().ToLower().StartsWith(cmd.ToString())).ToList());
                        if (similar.Count() > 0)
                        {
                            if (similar.Count() == 1)
                                cmd = new StringBuilder(similar.First());
                            else
                            {
                                StringBuilder AddedChar = new StringBuilder();
                                int offset = 0;
                                bool Ok = true;
                                foreach (char ch in similar.First())
                                {
                                    foreach (string s in similar)
                                    {
                                        if (ch != s[offset])
                                        {
                                            Ok = false;
                                            break;
                                        }

                                    }
                                    if (Ok)
                                    {
                                        offset++;
                                        AddedChar.Append(ch);
                                    }
                                    else
                                        break;
                                }
                                if (AddedChar.Length > cmd.Length)
                                    cmd = AddedChar;

                            }
                        }
                        CursorLeftOffset = cmd.Length;
                        Console.CursorLeft = 0;
                        Console.Write(CurrentMode + cmd.ToString() + new string(' ', Console.WindowWidth - (CurrentMode.ToString() + cmd.ToString()).Length));
                        Console.CursorLeft = CurrentMode.ToString().Length + cmd.Length;
                        
                        break;
                    case ConsoleKey.LeftArrow:
                        if (Console.CursorLeft > CurrentMode.ToString().Length)
                        {
                            Console.CursorLeft = Console.CursorLeft - 1;
                            CursorLeftOffset--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (Console.CursorLeft < CurrentMode.ToString().Length + cmd.Length)
                        {
                            Console.CursorLeft = Console.CursorLeft + 1;
                            CursorLeftOffset++;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (CommandsHistory.Count > 0)
                        {
                            if (Console.CursorTop != 0)
                                Console.CursorTop--;
                            else
                                Console.Clear();
                            Console.CursorLeft = 0;
                            if (historyindex is null)
                                historyindex = 0;
                            cmd = new StringBuilder(CommandsHistory[historyindex.Value]);
                            string History = CurrentMode + cmd.ToString();
                            Console.WriteLine();
                            Console.Write(CurrentMode + cmd.ToString() + new string(' ', Console.WindowWidth - History.Length));
                            Console.CursorLeft = History.Length;
                            CursorLeftOffset = cmd.Length;
                            historyindex += 1;
                            if (historyindex > CommandsHistory.Count - 1)
                                historyindex = 0;

                        }
                        break;
                    case ConsoleKey.UpArrow:
                        if (CommandsHistory.Count > 0)
                        {
                            if (Console.CursorTop != 0)
                                Console.CursorTop--;
                            else
                                Console.Clear();
                            Console.CursorLeft = 0;
                            if (historyindex is null)
                                historyindex = CommandsHistory.Count - 1;
                            cmd = new StringBuilder(CommandsHistory[historyindex.Value]);
                            string History = CurrentMode + cmd.ToString();
                            Console.WriteLine();
                            Console.Write(CurrentMode + cmd.ToString() + new string(' ', Console.WindowWidth - History.Length));
                            Console.CursorLeft = History.Length;
                            CursorLeftOffset = cmd.Length;
                            historyindex -= 1;
                            if (historyindex < 0)
                                historyindex = CommandsHistory.Count - 1;
                        }
                        break;
                    default:

                        if (key.KeyChar == '?')
                        {
                            bool Found = false;
                            foreach (string command in Commands.Where(c=>c.CheckMode(CurrentMode)).SelectMany(x => x.GetFullPathAndVariables().Where(c => c.ToLower().StartsWith(cmd.ToString())).ToList()))
                            {
                                Console.WriteLine();
                                Console.WriteLine(command);
                                Found = true;
                            }
                            if(!Found)
                                Console.WriteLine();
                            Console.Write(CurrentMode + cmd.ToString());
                        }
                        else
                        {
                            if (key.KeyChar != 0)
                            {
                                cmd.Insert(CursorLeftOffset, key.KeyChar);
                                Console.CursorLeft = 0;
                                Console.Write(CurrentMode.ToString() + cmd.ToString() + new string(' ', Console.WindowWidth - (cmd.ToString().Length) - CurrentMode.ToString().Length ));
                                Console.CursorLeft = CurrentMode.Name.Length + 1 + cmd.Length;
                            }
                            CursorLeftOffset++;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Beep();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = CurrentMode.Color;
            }
            if (!Exiting)
                Read();
        }
    }
}
