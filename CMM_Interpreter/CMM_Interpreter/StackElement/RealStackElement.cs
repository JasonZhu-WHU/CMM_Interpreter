using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class RealStackElement : StackElement
    {
        public int linenum;
        public string content;

        public RealStackElement(int line_num, string content)
        {
            type_code = 5;
            linenum = line_num;
            this.content = content;
        }
    }
}
