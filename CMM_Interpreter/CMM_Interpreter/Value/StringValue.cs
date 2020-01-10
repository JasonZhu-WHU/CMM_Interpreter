using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class StringValue : Value
    {
        public string value;

        //面临的问题包括，0000011、000这样的数，以及- - - - - 1这样的情况
        public StringValue(string type, bool initialized, string value, int line_num)
        {
            this.type = type;
            this.initialized = initialized;
            this.line_num = line_num;
            this.value = value;
        }

        public override string ToString()
        {
            string text = "";
            text += " Value{ type:" + type + ", value:" + value + ", line_num:" + line_num + "}";
            return text;
        }
    }
}
