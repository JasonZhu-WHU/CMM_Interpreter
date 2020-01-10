using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMM_Interpreter
{
    class IntValue : Value
    {
        public int value;

        //面临的问题包括，0000011、000这样的数，以及- - - - - 1这样的情况
        public IntValue(string type, bool initialized, string value, int line_num)
        {
            this.type = type;
            this.initialized = initialized;
            this.value = int.Parse(value);
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
