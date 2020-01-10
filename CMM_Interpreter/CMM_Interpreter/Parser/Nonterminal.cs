using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Nonterminal : Symbol
    {
        public int code;

        public Nonterminal(int code, string name)
        {
            this.code = code;
            this.name = name;
            is_terminal = false;
            Symbol.all_symbols.Add(name);
        }

        public Nonterminal(int code, string name, bool nullable)
        {
            this.code = code;
            this.name = name;
            is_terminal = false;
            is_nullable = nullable;
            Symbol.all_symbols.Add(name);
        }

    }
}
