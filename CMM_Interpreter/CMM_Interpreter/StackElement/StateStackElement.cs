using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class StateStackElement : StackElement
    {
        public int state;

        public StateStackElement(int state)
        {
            type_code = 6;
            this.state = state;
        }
    }
}
