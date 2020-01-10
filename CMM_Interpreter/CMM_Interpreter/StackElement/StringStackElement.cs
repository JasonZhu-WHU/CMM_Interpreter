using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class StringStackElement : StackElement
    {
        public int linenum;
        public string content;

        public StringStackElement(int line_num, string content)
        {
            type_code = 8;
            linenum = line_num;
            this.content = content;
        }
    }
}
