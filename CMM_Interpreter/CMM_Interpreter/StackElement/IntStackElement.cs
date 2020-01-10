using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class IntStackElement : StackElement
    {
        public int linenum;
        public string content;

        public IntStackElement(int line_num, string content)
        {
            type_code = 2;
            linenum = line_num;
            this.content = content;
        }
    }
}
