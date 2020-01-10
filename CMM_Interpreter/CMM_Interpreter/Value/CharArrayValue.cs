using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class CharArrayValue : Value
    {
        public string[] array_elements;
        private int array_len = 0;

        //面临的问题包括，0000011、000这样的数，以及- - - - - 1这样的情况
        public CharArrayValue(string type, bool initialized, int array_len, string[] array_elements, int line_num)
        {
            foreach(string e in array_elements)
            {
                if(e.Length > 1 && !e.Contains("\\"))
                {
                    throw new ExecutorException("输入的数组中元素" + e + "与类型" + type + "不符合", line_num);
                }
            }
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

        public string getArrayElement(int index)
        {
            return array_elements[index];
        }
    }
}
