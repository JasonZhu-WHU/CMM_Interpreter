using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Terminal : Symbol
    {
        public int code;

        public Terminal(int code, string name)
        {
            this.code = code;
            this.name = name;
            this.is_terminal = true;
            Symbol.all_symbols.Add(name);
        }
    }
}
