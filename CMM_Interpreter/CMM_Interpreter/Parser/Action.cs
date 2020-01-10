using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Action
    {
        public string action;
        public int new_state;
        public string left;
        public List<Symbol> right;
        public string err_msg;
        public string auto_shifted;
        
        //移进
        public Action(string action, int new_state)
        {
            this.action = action;
            this.new_state = new_state;
        }

        //为了处理空产生式，移进那个产生empty的产生式的左部，并根据

        //规约
        public Action(string action, string left, List<Symbol> right)
        {
            this.action = action;
            this.left = left;
            this.right = right;
        }

        //报错
        public Action(string action, string err)
        {
            this.action = action;
            this.err_msg = err;
        }

        //接受
        public Action(string action)
        {
            this.action = action;
        }

        //特殊动作
        public Action(string special_action, string auto_shifted, bool do_nothing)
        {
            action = special_action;
            this.auto_shifted = auto_shifted;
        }
    }
}
