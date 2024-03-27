using System;
using System.Collections.Generic; 

namespace PrettyCommandLine
{
    public class CommandExecuteEventArgs : EventArgs
    {
        public List<Variable> Variables;
    }
}
