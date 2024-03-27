using System; 

namespace PrettyCommandLine
{
    public class Variable
    {
        public string Name { get; set; }
        dynamic _value;
        public bool Required { get; set; }
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                Validation(value);
            }
        }

        public Type DataType { get; set; }
        public Variable(Type DataType)
        {
            this.DataType = DataType;
        }
        void Validation(object value)
        {
            try
            {
                if (Convert.ChangeType(value, DataType) == null)
                    throw new Exception("Conversion of Value '" + value + "' to type '" + DataType.FullName + "' failed");
            }
            catch (Exception ex)
            {
                throw new Exception("Conversion of Value '" + value + "' to type '" + DataType.FullName + "' failed", ex);
            }
            _value = value;
        }
    }
}
