using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class NumberValue : Value
    {
        public double value;

        public NumberValue(string type, bool initialized, string value, int line_num)
        {
            this.type = type;
            this.initialized = initialized;
            this.value = double.Parse(value);
            this.line_num = line_num;
        }

        public override string ToString()
        {
            string text = "";
            text += " Value{ type:" + type + ", value:" + value + ", line_num:" + line_num + "}";
            return text;
        }
    }
}
