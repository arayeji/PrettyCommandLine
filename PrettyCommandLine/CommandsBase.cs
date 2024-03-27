 
using System.Collections.Generic;
using System.Linq; 

namespace PrettyCommandLine
{
    public abstract class CommandsBase
    {
        public Commands Parent { get; set; }
        public bool IsCaseSensetive;
        public bool IsExecutable;
        string command;
        public string Command
        {
            get
            {
                if (IsCaseSensetive)
                    return command;
                else
                    return command.ToLower();
            }

            set
            {
                command = value.Trim();
            }
        }
        internal Dictionary<string, Commands> SubCommands = new Dictionary<string, Commands>();
        public List<Variable> Variables = new List<Variable>();

        public string CommandDescription { get; set; }
        internal CommandTypes CommandType;
        public delegate void ExecuteHandler(object sender, CommandExecuteEventArgs e);
        public event ExecuteHandler Executable;
        public virtual void CallEvent(object sender, CommandExecuteEventArgs e)
        {
            if(Validation())
            Executable?.Invoke(sender, e);
        }

        public abstract void AddToMode(Modes mode);
        public abstract void RemoveMode(Modes mode);
        public CommandsBase(CommandTypes ContainSubCommands)
        {
            CommandType = ContainSubCommands;
        }
        public bool IsSubCommandMatched(string SubCommand)
        {
            return SubCommands.ContainsKey(SubCommand);
        }
        public List<string> GetFullPath()
        {
            List<string> Result = new List<string>();

             if (SubCommands.Count == 0)
                Result.Add(Command);
             else
            if (SubCommands.Count > 0)
                Result.AddRange(SubCommands.SelectMany(x => x.Value.GetFullPath()).ToList().Select(c => Command + " " + c).ToList());

            return Result;
        }

        public List<string> GetPath()
        {
            List<string> Path = new List<string>();
            if (Parent != null)
            {
                Path.AddRange(Parent.GetPath().SelectMany(x=>x.Split(' ')));
                Path.Add(Command);
                return Path;
            }
            else
            {
                Path.AddRange(Command.Split(' '));
                return Path;
            }
              
        }
        public abstract bool Validation();

        public List<CommandsBase> GetAllCommands()
        {
            List<CommandsBase> Result = new List<CommandsBase>();

            if (IsExecutable || ( Parent != null && SubCommands.Count == 0))
            Result.Add(this);

            Result.AddRange(SubCommands.Values.SelectMany(x => x.GetAllCommands()));

            return Result;
        }
         

        public List<string> GetFullPathAndVariables()
        {
            List<string> Result = new List<string>();

            if (Variables.Count > 0)
                Result.AddRange(new List<string> { Command + "\r\n" + string.Join(", ", Variables.Select(x => "(" + x.Name + " : " + x.DataType.Name + ")").ToList()) });
            else if (SubCommands.Count == 0)
                Result.Add(  Command);
             
            if (SubCommands.Count > 0)
                Result.AddRange(SubCommands.SelectMany(x => x.Value.GetFullPathAndVariables()).ToList().Select(c => Command + " " + c).ToList());

            return Result;
        }

        public bool IsCommandMatched(string[] CommandArgs)
        {
            return GetFullPath().Contains(string.Join(" ", CommandArgs).ToLower());
        }
         

        public abstract bool CheckMode(Modes CurrentMode);
        public abstract bool CheckAndExecute(string[] CommandArgs, Modes CurrentMode);
        public abstract bool CheckAndExecute(InputData inputData, Modes CurrentMode);
        public abstract void AddSubCommand(Commands Command);
        public abstract void RemoveSubCommand(Commands Command);
         
        public abstract void Execute(string[] args, bool Silent);
        public abstract void Execute(InputData inputData, bool Silent);

        public enum CommandTypes
        {
            Command,
            SubCommand,
            Mode
        }

    }
}
