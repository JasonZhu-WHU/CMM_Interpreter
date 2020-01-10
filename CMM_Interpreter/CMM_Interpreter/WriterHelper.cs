using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMM_Interpreter
{
    class WriterHelper
    {

        internal static void write_value(Value v, int linenum)
        {
            if(v.type == "int")
            {
                IntValue value = (IntValue)v;
                MessageBox.Show(value.value.ToString());
            }
            else if (v.type == "real")
            {
                RealValue value = (RealValue)v;
                MessageBox.Show(value.value.ToString());
            }
            else if (v.type == "number")
            {
                NumberValue value = (NumberValue)v;
                MessageBox.Show(value.value.ToString());
            }
            else if (v.type == "char")
            {
                CharValue value = (CharValue)v;
                MessageBox.Show(value.value);
            }
            else if (v.type == "string")
            {
                StringValue value = (StringValue)v;
                MessageBox.Show(value.value);
            }
            else if (v.type == "bool")
            {
                BoolValue value = (BoolValue)v;
                MessageBox.Show(value.value.ToString());
            }
            else if (v.type == "intArray")
            {
                IntArrayValue value = (IntArrayValue)v;
                string text = "";
                for(int i = 0; i < value.array_elements.Length; i++)
                {
                    text += value.array_elements[i];
                    text += "|";
                }
                MessageBox.Show(text);
            }
            else if (v.type == "realArray")
            {
                RealArrayValue value = (RealArrayValue)v;
                string text = "";
                for (int i = 0; i < value.array_elements.Length; i++)
                {
                    text += value.array_elements[i];
                    text += "|";
                }
                MessageBox.Show(text);
            }
            else if (v.type == "charArray")
            {
                CharArrayValue value = (CharArrayValue)v;
                string text = "";
                for (int i = 0; i < value.array_elements.Length; i++)
                {
                    text += value.array_elements[i];
                    text += "|";
                }
                MessageBox.Show(text);
            }
            else if (v.type == "stringArray")
            {
                StringArrayValue value = (StringArrayValue)v;
                string text = "";
                for (int i = 0; i < value.array_elements.Length; i++)
                {
                    text += value.array_elements[i];
                    text += "|";
                }
                MessageBox.Show(text);
            }
            else
            {
                throw new ExecutorException("出现了没有考虑到的新类型", linenum);
            }
        }
    }
}
