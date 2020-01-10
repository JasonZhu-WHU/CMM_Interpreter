using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class IdentifierStackElement : StackElement
    {
        public int linenum;
        public string content;

        public IdentifierStackElement(int line_num, string content)
        {
            type_code = 1;
            linenum = line_num;
            this.content = content;
        }
    }
}
