using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class StackElement
    {

        //栈的每个元素，除非是state（表示状态），否则都是一棵树
        public int type_code;
        public bool is_Tree = false;
        public List<object> branches = new List<object>();
        public int layers = 1;

        public override string ToString()
        {
            string text = "";
            if(type_code == 1)
            {
                IdentifierStackElement e = (IdentifierStackElement)this;
                text += "第" + e.linenum + "行标识符";
                text += e.content;
            }
            else if (type_code == 2)
            {
                IntStackElement e = (IntStackElement)this;
                text += "第" + e.linenum + "行整数";
                text += e.content;
            }
            else if (type_code == 3)
            {
                NonterminalStackElement e = (NonterminalStackElement)this;
                text += "非终结符";
                text += e.name;
                //递归调用
                foreach(StackElement s in e.branches)
                {
                    text += Environment.NewLine;
                    for(int i = 1; i < s.layers; i++)
                    {
                        text += "  ";
                    }
                    text += s.ToString();
                }
            }
            else if(type_code == 4)
            {
                OtherTerminalStackElement e = (OtherTerminalStackElement)this;
                text += "第" + e.linenum + "行终结符";
                text += e.content;
            }
            else if (type_code == 5)
            {
                RealStackElement e = (RealStackElement)this;
                text += "第" + e.linenum + "行实数";
                text += e.content;
            }
            else if (type_code == 7)
            {
                CharStackElement e = (CharStackElement)this;
                text += "第" + e.linenum + "行字符";
                text += e.content;
            }
            else if (type_code == 8)
            {
                StringStackElement e = (StringStackElement)this;
                text += "第" + e.linenum + "行字符串";
                text += e.content;
            }
            else
            {
                StateStackElement e = (StateStackElement)this;
                text += "状态" + e.state;
            }
            return text;
        }

        public void layersIncreaseRecursively()
        {
            layers += 1;
            foreach(StackElement s in branches)
            {
                s.layersIncreaseRecursively();
            }
        }
    }
}
