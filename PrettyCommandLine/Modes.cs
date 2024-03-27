using System; 

namespace PrettyCommandLine
{
    public class Modes
    {
        public string Name;
        public char Sign = '#';
        public bool isDefault;
        public ConsoleColor Color = ConsoleColor.Yellow;
        
        public override string ToString()
        {
            return Name+Sign;
        }
    }
}
