using System;
using System.Collections.Generic; 
using System.Linq; 

namespace PrettyCommandLine
{
   
    public class Commands : CommandsBase
    { 
        Dictionary<string,Modes> ActiveModes = new Dictionary<string,Modes>();
        public Commands(CommandTypes ContainSubCommands) : base(ContainSubCommands)
        {
        }

        public override bool Validation()
        {
            return true;
        }
        public override void AddToMode(Modes mode)
        {
            if(!ActiveModes.ContainsKey(mode.Name))
            ActiveModes.Add(mode.Name, mode);
        }
        public override void RemoveMode(Modes mode)
        {
            ActiveModes.Remove(mode.Name);
        }

        public override void Execute(string[] args, bool Silent)
        {
            switch (CommandType)
            {
                case CommandTypes.Mode:
                    {
                        CallEvent(this, new CommandExecuteEventArgs());
                    }
                    break;
                case CommandTypes.Command:
                case CommandTypes.SubCommand:
                    { 
                        if (args.Length - GetPath().Count >= Variables.Where(x=>x.Required).Count())
                        {
                            if (args.Length - GetPath().Count > Variables.Count())
                            {
                                if (!Silent)
                                    throw new Exception("This command expected '" + Variables.Count() + "' variable(s)" + "\r\n\r\n" + CommandDescription);
                            }
                            else
                            {
                                try
                                {
                                    List<Variable> variablelist = new List<Variable>();

                                    for (int i = 0; i < args.Length - GetPath().Count; i++)
                                    {
                                        Variable var = new Variable(Variables[i].DataType);
                                        var.Name = Variables[i].Name;
                                        var.Value = args[i + args.Length - Variables.Count];
                                        variablelist.Add(var);
                                    }
                                    CallEvent(this, new CommandExecuteEventArgs { Variables = variablelist });
                                }
                                catch (Exception ex)
                                {
                                    if (!Silent)
                                        throw new Exception(ex.Message + "\r\n\r\n" + CommandDescription);
                                }
                            }
                        }
                        else
                            if(!Silent)
                            throw new Exception("This command expected '" + Variables.Count() + "' variable(s)" + "\r\n\r\n" + CommandDescription);
                    }
                    break;
                default:
                    break;
            }
        }
        public override void Execute(InputData inputData, bool Silent)
        {
            switch (CommandType)
            {
                case CommandTypes.Mode:
                    {
                        CallEvent(this, new CommandExecuteEventArgs());
                    }
                    break;
                case CommandTypes.Command:
                case CommandTypes.SubCommand:
                    {
                        List<Variable> variablelist = new List<Variable>();
                        if (inputData.Variables.Count > 0)
                        {
                            if (inputData.Variables.Count >= Variables.Where(x => x.Required).Count())
                            {
                                try
                                {
                                    foreach (var variable in inputData.Variables)
                                    {
                                        var var = Variables.Where(x => x.Name.ToLower() == variable.Name);
                                        if (var.Count()>0)
                                        {
                                            Variable newvar = new Variable(var.First().DataType);
                                            newvar.Name = var.First().Name;
                                            newvar.Value = variable.Value;
                                            variablelist.Add(newvar);
                                        }
                                    }
                                    foreach(var variable in Variables)
                                    {
                                        if(variable.Required && variablelist.Where(x=>x.Name.ToLower() == variable.Name.ToLower()).Count()==0)
                                            if (!Silent)
                                                throw new Exception("This command expected '" + variable.Name+ "' variable" + "\r\n\r\n" + CommandDescription);
                                    }
                                    CallEvent(this, new CommandExecuteEventArgs { Variables = variablelist });
                                }
                                catch (Exception ex)
                                {
                                    if (!Silent)
                                        throw new Exception(ex.Message + "\r\n\r\n" + CommandDescription);
                                }
                            }
                            else
                            if (!Silent)
                                throw new Exception("This command expected '" + Variables.Count() + "' variable(s)" + "\r\n\r\n" + CommandDescription);
                        }
                        else
                            if (inputData.Commands.Count - GetPath().Count >= Variables.Where(x => x.Required).Count())
                        {
                            try
                            {
                                for (int i = 0; i < inputData.Commands.Count - GetPath().Count; i++)
                                {
                                    Variable var = new Variable(Variables[i].DataType);
                                    var.Name = Variables[i].Name;
                                    var.Value = inputData.Commands[i + inputData.Commands.Count - Variables.Count];
                                    variablelist.Add(var);
                                }
                                CallEvent(this, new CommandExecuteEventArgs { Variables = variablelist });
                            }
                            catch (Exception ex)
                            {
                                if (!Silent)
                                    throw new Exception(ex.Message + "\r\n\r\n" + CommandDescription);
                            }
                        }
                        else
                            if (!Silent)
                            throw new Exception("This command expected '" + Variables.Count() + "' variable(s)" + "\r\n\r\n" + CommandDescription);
                    }
                    break;
                default:
                    break;
            }
        }



        public override bool CheckMode(Modes CurrentMode)
        {
            if (ActiveModes.Count == 0 || ActiveModes.ContainsKey(CurrentMode.Name))
                return true;
            else
                return false;
        }
        public override bool CheckAndExecute(InputData inputData, Modes CurrentMode)
        {
            if (CheckMode(CurrentMode))
            {
                if (SubCommands.Count > 0)
                {
                    var Commands = GetAllCommands().Where(c => string.Join(" ", inputData.Commands).ToLower().StartsWith(string.Join(" ", c.GetPath()))).ToList();

                    if (Commands != null && Commands.Count() > 0)
                    {
                        bool Silent = Commands.Count() > 1;

                        foreach (Commands command in Commands)
                        {
                            command.Execute(inputData, Silent);
                        }
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (GetFullPath().Where(x => string.Join(" ", inputData.Commands).ToLower().StartsWith(x)).Count() > 0)
                    {
                        Execute(inputData, false);
                        return true;
                    }
                }
            }
            return false;
        }
        public override bool CheckAndExecute(string[] CommandArgs,Modes CurrentMode)
        {
            if (CheckMode(CurrentMode))
            {
                if (SubCommands.Count > 0)
                { 
                    var Commands = GetAllCommands().Where(c => string.Join(" ", CommandArgs).ToLower().StartsWith(string.Join(" ", c.GetPath()))).ToList();
                    
                    if (Commands!=null && Commands.Count() > 0)
                    {
                        bool Silent = Commands.Count() > 1;

                        foreach (Commands command in Commands)
                        { 
                                command.Execute(CommandArgs, Silent);
                        }
                        return true;
                    }
                    else
                        return false; 
                }
                else
                {
                    if (GetFullPath().Where(x => string.Join(" ", CommandArgs).ToLower().Contains(x)).Count() > 0)
                    {
                        string[] Inputs = new string[Variables.Count];
                        for (int i = 0; i < Variables.Count; i++)
                            Inputs[i] = CommandArgs[i + CommandArgs.Length - Variables.Count];

                        Execute(CommandArgs,false);
                        return true;
                    }
                }
            }
            return false;
        }
        public override void AddSubCommand(Commands Command)
        {
            Command.CommandType = CommandTypes.SubCommand;
            Command.Parent = this;
            SubCommands.Add(Command.Command, Command);
        }

        public override void RemoveSubCommand(Commands Command)
        {
            Command.CommandType = CommandTypes.Command;
            Command.Parent = null;
            SubCommands.Remove(Command.Command);
        }

    }

}
