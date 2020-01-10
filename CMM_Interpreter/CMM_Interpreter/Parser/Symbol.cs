using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Symbol
    {
        public bool is_terminal;
        public string name;
        public bool is_nullable = false;

        public static HashSet<string> all_symbols = new HashSet<string>();
    }
}
