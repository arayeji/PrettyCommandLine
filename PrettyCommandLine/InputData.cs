using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrettyCommandLine
{
    public class InputVariable
    {
        string _name;
        public string Name { get { return _name.Remove(0, 2); } set { _name = value;  } }
        public string Value { get; set; }
    }
    public class InputData
    {
        public List<string> Commands { get; set; } = new List<string>();
        public List<InputVariable> Variables { get; set; } = new List<InputVariable>();
    }
}
