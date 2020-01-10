using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class OtherTerminalStackElement : StackElement
    {
        public int linenum;
        public string content;

        public OtherTerminalStackElement(int line_num, string content)
        {
            type_code = 4;
            linenum = line_num;
            this.content = content;
        }
    }
}
