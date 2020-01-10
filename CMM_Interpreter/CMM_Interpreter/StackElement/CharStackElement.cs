using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class CharStackElement : StackElement
    {
        public int linenum;
        public string content;

        public CharStackElement(int line_num, string content)
        {
            type_code = 7;
            linenum = line_num;
            this.content = content;
        }
    }
}
