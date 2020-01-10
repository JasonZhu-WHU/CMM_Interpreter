using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class NonterminalStackElement : StackElement
    {
        public int linenum;
        public string name;

        public NonterminalStackElement(int line_num, string name)
        {
            type_code = 3;
            linenum = line_num;
            this.name = name;
        }
    }
}
