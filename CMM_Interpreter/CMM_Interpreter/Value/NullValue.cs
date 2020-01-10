using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMM_Interpreter
{
    class NullValue : Value
    {
        public NullValue(int linenum)
        {
            type = "null";
            is_null = true;
            line_num = linenum;
        }
    }
}
