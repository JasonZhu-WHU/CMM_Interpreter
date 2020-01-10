using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMM_Interpreter
{
    class ItemSet
    {
        //单例 状态集
        public static List<ItemSet> states_lst = new List<ItemSet>();

        //第几号状态集
        public int num;
        public List<Item> itemset = new List<Item>();

        public ItemSet(int number)
        {
            num = number;
        }

        public ItemSet(int number, List<Item> itemSet)
        {
            num = number;
            itemset = itemSet;
        }

        public override string ToString()
        {
            string text = "第" + num + "号项目集所有项目： ";
            foreach (Item i in itemset)
            {
                text += i.ToString();
            }
            return text;
        }

        //利用当前项目集（状态），递归产生可以跳转到的新的状态，这个函数是产生状态机的主要业务函数
        public void addStateToStateLst()    
        {
            this.getCompleteClosure();
            //右部第一个symbol - 新状态项目
            Dictionary<string, List<Item>> transit_accordingToFirstEleInRight = new Dictionary<string, List<Item>>();
            //存规约的产生式
            List<Item> reducable_items = new List<Item>();
            //存规约的条件（followset）
            List<string> reducable_symbols = new List<string>();
            //存一种特殊情况，也就是遇到了产生空产生式的项目，为什么空产生式很特殊，因为.empty这种表达形式不兼容，所以单独处理，直接移进
            Dictionary<string, Action> direct_shift = new Dictionary<string, Action>();

            foreach (Item ii in this.itemset)
            {
                if(ii.index_of_point < ii.right.Count)
                {
                    //空产生式我们认为可以直接多执行一个特别动作，也就是直接移入那个空产生式左部，然后立刻规约（相当于 移进+规约）
                    if(ii.right[0].name == "empty")
                    {
                        foreach (string s in GrammerConfig.followSet[ii.left])
                        {
                            direct_shift.Add(s, new Action("special action", ii.left, true));
                        }
                    }
                    else
                    {
                        if (transit_accordingToFirstEleInRight.Keys.Contains(ii.right[ii.index_of_point].name))
                        {
                            transit_accordingToFirstEleInRight[ii.right[ii.index_of_point].name].Add(new Item(ii.left, ii.right, ii.index_of_point + 1));
                        }
                        else
                        {
                            transit_accordingToFirstEleInRight[ii.right[ii.index_of_point].name] = new List<Item>();
                            transit_accordingToFirstEleInRight[ii.right[ii.index_of_point].name].Add(new Item(ii.left, ii.right, ii.index_of_point + 1));
                        }
                    }
                }
                //有一个项目的点到了最后，也就是可以规约的项目，但是能不能规约，取决于其左部的Follow集是否包含/交叉
                else
                {
                    //那么Follow集如果没有冲突，就都应该是遇到就规约的
                    foreach(string s in GrammerConfig.followSet[ii.left])
                    {
                        if (transit_accordingToFirstEleInRight.Keys.Contains(s))
                        {
                            MessageBox.Show("发生了SLR无法解决的移进规约冲突");
                            throw new ParserException("发生了SLR无法解决的移进规约冲突");
                        }
                        reducable_symbols.Add(s);
                    }
                    reducable_items.Add(ii);
                }
            }
            //现在有两个要处理的方向，一，我们要把可以产生新状态（项目集）的产生，并且给新状态编号，并且把可以转到新状态的符号的语法动作填入那个分析表
            //二、对于可规约的项目，我们要确定哪些符号（根据该产生式左部的Follow集）可以使项目规约，同样把这些符号语法动作填入分析表
            foreach(string s in transit_accordingToFirstEleInRight.Keys)
            {
                bool has_same = false;
                int num = ItemSet.states_lst.Count + 1;
                int transit_to = num;
                ItemSet new_set = new ItemSet(num);
                foreach (Item i in transit_accordingToFirstEleInRight[s])
                {
                    new_set.itemset.Add(i);
                }
                new_set.getCompleteClosure();
                foreach(ItemSet state in states_lst)
                {
                    if (state.Equals(new_set))
                    {
                        has_same = true;
                        transit_to = state.num;
                        break;
                    }
                }
                GrammerConfig.analysis_table[this.num][s] = new Action("shift", transit_to);
                if (!has_same)
                {
                    Console.WriteLine(new_set.ToString());
                    ItemSet.states_lst.Add(new_set);
                    new_set.addStateToStateLst();
                }
            }
            foreach(Item i in reducable_items)
            {
                //把规约根据的产生式找出来
                foreach(string s in GrammerConfig.grammer.Keys)
                {
                    //左部相同
                    if(s == i.left)
                    {
                        foreach(List<Symbol> lst in GrammerConfig.grammer[s])
                        {
                            //右部相同，也就是找到了那个规约时候要遵守的语法产生式
                            if (i.rightIsSameToLst(lst))
                            {
                                foreach(string ss in reducable_symbols)
                                {
                                    GrammerConfig.analysis_table[this.num][ss] = new Action("reduce", i.left, lst);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            foreach(string empty_left in direct_shift.Keys)
            {
                GrammerConfig.analysis_table[this.num][empty_left] = new Action("special action", direct_shift[empty_left].auto_shifted, true);
            }
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
            ItemSet temp = (ItemSet)obj;
            if (this.itemset.Count != temp.itemset.Count)
            {
                return false;
            }
            for (int i = 0; i < itemset.Count; i++)
            {
                if (!itemset[i].Equals(temp.itemset[i]))
                {
                    return false;
                }
            }
            return true;
        }

        //重写GetHashCode方法（重写Equals方法必须重写GetHashCode方法，否则发生警告
        public override int GetHashCode()
        {
            return states_lst.GetHashCode() + num.GetHashCode() + itemset.GetHashCode();
        }

        //itemset的itemset求完整闭包
        public void getCompleteClosure()
        {
            bool changed = false;
            if (itemset.Count == 0)
            {
                throw new ParserException(num + "项目集中没有项目");
            }
            else
            {
                int length = itemset.Count;
                for (int j = 0; j < length; j++)
                {
                    Item i = itemset[j];
                    //如果是不可规约的项目（indexPoint不在最后）（确保indexPoint处有元素，防止越界）
                    if (i.index_of_point < i.right.Count)
                    {
                        //如果下一个Symbol是非终结符（要扩充闭包），终结符就不用管了
                        if (!i.right[i.index_of_point].is_terminal)
                        {
                            //准备可能要扩充的项目的左部
                            string new_left = i.right[i.index_of_point].name;
                            //每个可能的产生式与已有项目对比
                            foreach (List<Symbol> l in GrammerConfig.grammer[new_left])
                            {
                                //检查项目集所有项目，检查是否有相同的
                                bool none_is_the_same = true;
                                foreach (Item ii in itemset)
                                {
                                    if (new Item(new_left, l, 0).Equals(ii))
                                    {
                                        none_is_the_same = false;
                                    }
                                }
                                //这个项目不在我们的项目集里，那就添加
                                if (none_is_the_same)
                                {
                                    itemset.Add(new Item(new_left, l, 0));
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
            //如果改变了（也就是添加了新项目），就递归调用，直到没有变化，那就自然停止递归调用了（tail recursion资源消耗小）
            if (changed)
            {
                getCompleteClosure();
            }
        }
    }
}
