using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class RealArrayValue : Value
    {
        //realArray
        public double[] array_elements;
        private int array_len = 0;

        //面临的问题包括，0000011、000这样的数，以及- - - - - 1这样的情况
        public RealArrayValue(string type, bool initialized, int array_len, double[] array_elements, int line_num)
        {
            this.type = type;
            this.initialized = initialized;
            this.array_len = array_len;
            this.array_elements = array_elements;
            this.line_num = line_num;
        }

        public override string ToString()
        {
            string text = "";
            text += " Value{ type:" + type + ", length:" + array_len + ", line_num:" + line_num + "}";
            return text;
        }

        public int getLen()
        {
            return array_len;
        }

        public double getArrayElement(int index)
        {
            return array_elements[index];
        }
    }
}
