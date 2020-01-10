using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Item
    {
        public string left;
        public List<Symbol> right;
        public int index_of_point;

        public Item(string left, List<Symbol> right, int index_of_point)
        {
            this.left = left;
            this.right = right;
            this.index_of_point = index_of_point;
        }

        public override string ToString()
        {
            string text = "左部:" + left + " 右部:";
            foreach(Symbol s in right)
            {
                text += s.name;
                text += " ";
            }
            text += "点的位置:" + index_of_point + "   ";
            return text;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj.GetType().Equals(this.GetType())) == false)
            {
                return false;
            }
            Item temp = (Item)obj;
            if(left != temp.left)
            {
                return false;
            }
            if(right.Count != temp.right.Count)
            {
                return false;
            }
            if(index_of_point != temp.index_of_point)
            {
                return false;
            }
            for(int i = 0; i < right.Count; i++)
            {
                if(right[i].name != temp.right[i].name)
                {
                    return false;
                }
            }
            return true;
        }

        //重写GetHashCode方法（重写Equals方法必须重写GetHashCode方法，否则发生警告
        public override int GetHashCode()
        {
            return left.GetHashCode() + right.GetHashCode() + index_of_point.GetHashCode();
        }

        public bool rightIsSameToLst(List<Symbol> lst)
        {
            if(lst.Count != this.right.Count)
            {
                return false;
            }
            for(int j = 0; j < lst.Count; j++)
            {
                if (lst[j].name != this.right[j].name)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
