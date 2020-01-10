using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Frame
    {
        public static Frame curr_frame;
        public static Frame global_frame;
        public Frame parent;
        public Dictionary<string, Value> local_bindings;
        public Value return_val;

        public Frame(Frame parent)
        {
            this.parent = parent;
            local_bindings = new Dictionary<string, Value>();
        }

        public Frame(Frame parent, Dictionary<string, Value> bindings)
        {
            this.parent = parent;
            local_bindings = new Dictionary<string, Value>();
            foreach(string k in bindings.Keys)
            {
                local_bindings.Add(k, bindings[k]);
            }
        }

        public void define(string identifier, Value v)
        {
            local_bindings[identifier] = v;
        }

        public Value lookUp(string id)
        {
            if (local_bindings.Keys.Contains(id))
            {
                return local_bindings[id];
            }
            else
            {
                throw new ExecutorException(id + "不是当前栈帧的局部变量");
            }
        }

        public Frame makeChildFrame(Dictionary<string, Value> bindings)
        {
            Frame childFrame = new Frame(this, bindings);
            return childFrame;
        }

    }
}
