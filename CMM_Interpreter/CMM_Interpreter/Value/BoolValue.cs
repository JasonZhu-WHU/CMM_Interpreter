using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class BoolValue : Value
    {
        public bool value;

        //面临的问题包括，0000011、000这样的数，以及- - - - - 1这样的情况
        public BoolValue(string type, bool initialized, bool value, int line_num)
        {
            this.type = type;
            this.initialized = initialized;
            this.value = value;
            this.line_num = line_num;
        }

        public override string ToString()
        {
            string text = "";
            text += " Value{ type:" + type + ", value:" + value + "}";
            return text;
        }
    }
}
