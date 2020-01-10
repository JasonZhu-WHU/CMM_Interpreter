using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMM_Interpreter
{
    class GrammerConfig
    {
        // key就是每个产生式的左部，也就是所有的非终结符，value代表了该非终结符的所有产生式右部的List，每个产生式右部由symbol（终结符/非终结符）的List构成
        // 比如grammer["prog"][0]代表prog的第一个产生式右部所有符号的List
        public static Dictionary<string, List<List<Symbol>>> grammer = new Dictionary<string, List<List<Symbol>>>();

        //每个非终结符，应该都对应一个集合（这个集合里面可以包含比如empty之类的终结符元素）
        public static Dictionary<string, HashSet<string>> firstSet = new Dictionary<string, HashSet<string>>();
        public static Dictionary<string, HashSet<string>> followSet = new Dictionary<string, HashSet<string>>();

        //最最重要的分析表 状态 - (识别到符号， 语法动作）
        public static Dictionary<int, Dictionary<string, Action>> analysis_table = new Dictionary<int, Dictionary<string, Action>>();

        public static int states_num = 250;

        public GrammerConfig()
        {

        }

        //程序启动时就调用了
        public static void init()
        {
            constructGrammer();
            showGrammer();
            initFirstSet();
            initFollowSet();
            initAnalysisTable();
            showFirstSetAndFollowSet();

            initItemset();
            showAnalysisTable();
        }

        private static void initAnalysisTable()
        {
            addTerminalsToSymbols();
            for (int i = 1; i < states_num + 1; i++)
            {
                analysis_table.Add(i, new Dictionary<string, Action>());
                foreach(string symbol in Symbol.all_symbols)
                {
                    analysis_table[i].Add(symbol, new Action("error", "err"));
                }
            }
        }

        private static void showAnalysisTable()
        {
            foreach(int state in analysis_table.Keys)
            {
                Console.WriteLine("状态" + state);
                foreach(string symbol in analysis_table[state].Keys)
                {
                    string text = "识别到" + symbol + "就执行";
                    if(analysis_table[state][symbol].action == "shift")
                    {
                        text += "shift，跳转到" + analysis_table[state][symbol].new_state + "状态";
                        Console.WriteLine(text);
                    }
                    else if(analysis_table[state][symbol].action == "reduce")
                    {
                        text += "reduce，利用" + analysis_table[state][symbol].left + "->";
                        foreach(Symbol s in analysis_table[state][symbol].right)
                        {
                            text += s.name;
                        }
                        text += "产生式进行规约";
                        Console.WriteLine(text);
                    }
                    else if (analysis_table[state][symbol].action == "special action")
                    {
                        text += "special action，将自动移入" + analysis_table[state][symbol].auto_shifted + "，并跳转到对应状态";
                        Console.WriteLine(text);
                    }
                }
            }
        }
            
        private static void showFirstSetAndFollowSet()
        {
            foreach (string nonterminal in firstSet.Keys)
            {
                string text = nonterminal + "的FirstSet为:";
                foreach (string s in firstSet[nonterminal])
                {
                    text += s;
                    text += " ";
                }
                text += "   ";
                text += "FollowSet为:";
                foreach (string s in followSet[nonterminal])
                {
                    text += s;
                    text += " ";
                }
                Console.WriteLine(text);
            }
        }

        //创建First集
        private static void initFirstSet()
        {
            //非终结符才有FirstSet
            foreach (string nonterminal in grammer.Keys)
            {
                ItemSet i = new ItemSet(1);
                foreach (List<Symbol> lst in grammer[nonterminal])
                {
                    i.itemset.Add(new Item(nonterminal, lst, 0));
                    Console.WriteLine(i.ToString());
                }
                i.getCompleteClosure();
                HashSet<string> hs = new HashSet<string>();
                foreach (Item ii in i.itemset)
                {
                    // *************
                    //完整闭包里面每个项目右部第一个是终结符的，因为是set直接加入就行
                    //但是单单这样做还不够，因为如果第一个非终结符是可空的，那么就应该还来拿第二个进行判断，也即是下一个才能决定，除非所有的都有可能为空，否则就不能把empty加进去，而应当添加至最后一个不可能为空的
                    if (ii.right[0].is_terminal)
                    {
                        if(ii.right[0].name != "empty")
                        {
                            hs.Add(ii.right[0].name);
                        }
                    }
                    //如果第一个不是终结符，那我们要检查这个非终结符可不可能为空（如果不可能为空，那么不用管，因为完整闭包会帮我们加入First）
                    else
                    {
                        //不是nullable就不需要管
                        if (ii.right[0].is_nullable)
                        {
                            if(ii.right.Count > 1)
                            {
                                //两个nullable非终结符就一个
                                if (ii.right[1].is_nullable)
                                {
                                    hs.Add("empty");
                                }
                                //一个nullable非终结符开头的，只有spec_declare
                                if (ii.right[1].is_terminal)
                                {
                                    hs.Add(ii.right[1].name);
                                }
                            }
                        }
                    }
                }
                foreach(List<Symbol> l in grammer[nonterminal])
                {
                    if (l[0].is_terminal)
                    {
                        hs.Add(l[0].name);
                    }
                    //我定义的文法中，每个产生式右部第一个非终结符不可能是nullable的
                }
                firstSet.Add(nonterminal, hs);
            }
        }

        //创建Follow集
        private static void initFollowSet()
        {
            //某个在产生式右部末尾的非终结符 - 对应的左部
            Dictionary<string, HashSet<string>> last_deal_set = new Dictionary<string, HashSet<string>>();
            //根据每个非终结符进行分析
            foreach (string nonterminal in grammer.Keys)
            {
                HashSet<string> hs = new HashSet<string>();
                foreach (string left in grammer.Keys)
                {
                    List<List<Symbol>> ll = grammer[left];
                    //遍历每一个文法产生式
                    foreach (List<Symbol> l in ll)
                    {
                        //找到这个产生式右部中所有与该非终结符相同的index
                        List<int> indexes = new List<int>();
                        for(int i = 0; i < l.Count; i++)
                        {
                            //若这个产生式有非终结符与我们在找的非终结符相同
                            if (!l[i].is_terminal && l[i].name == nonterminal)
                            {
                                indexes.Add(i);
                            }
                        }
                        //找完所有index，对那个非终结符后面的那个元素进行分析Firstset，加到Followset
                        foreach(int index in indexes)
                        {
                            if(index < l.Count - 1)
                            {
                                //若下一个是终结符
                                if (l[index + 1].is_terminal)
                                {
                                    hs.Add(l[index + 1].name);
                                }
                                else
                                {
                                    //如果是非终结符，那还要看是否是nullable的
                                    if (l[index + 1].is_nullable)
                                    {
                                        //我的文法里面，所有nullable的非终结符都是在产生式右部最后一个，也就是说，empty可以加入，且不需要管后面的
                                        foreach (string s in firstSet[l[index + 1].name])
                                        {
                                            hs.Add(s);
                                        }
                                    }
                                    else
                                    {
                                        foreach(string s in firstSet[l[index + 1].name])
                                        {
                                            hs.Add(s);
                                        }
                                    }
                                }
                            }
                            //说明是最后一个，加入empty?
                            //错，比如A->B;  B->C，这里C的Follow集应该包括;分号，所以情况是很复杂的
                            //更复杂的情况：最后一个是nullable的非终结符，那么还要看它前面最近的非终结符，让它的Follow集也进行扩展
                            //这件事情告诉我们，坑是填不完的，所以要充分测试
                            else
                            {
                                //hs.Add("empty");
                                if (!last_deal_set.Keys.Contains(l[index].name))
                                {
                                    last_deal_set[l[index].name] = new HashSet<string>();
                                }
                                if (l[index].is_nullable)
                                {
                                    int j = index-1;
                                    while(j >= 0)
                                    {
                                        if (!l[j].is_terminal)
                                        {
                                            if (!last_deal_set.Keys.Contains(l[j].name))
                                            {
                                                last_deal_set[l[j].name] = new HashSet<string>();
                                            }
                                            last_deal_set[l[j].name].Add(left);
                                            if (!l[j].is_nullable)
                                            {
                                                break;
                                            }
                                        }
                                        j--;
                                    }
                                }
                                last_deal_set[l[index].name].Add(left);
                                
                            }
                        }
                    }
                }
                followSet[nonterminal] = hs;
            }
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach(string nonterminal in last_deal_set.Keys)
                {
                    foreach(string left in last_deal_set[nonterminal])
                    {
                        foreach(string follow_ele in followSet[left])
                        {
                            if (!followSet[nonterminal].Contains(follow_ele))
                            {
                                followSet[nonterminal].Add(follow_ele);
                                changed = true;
                            }
                        }
                    }
                }
            }
            followSet["E"].Add("empty");
            followSet["prog"].Add("empty");
            followSet["extern_declaration"].Add("empty");
            followSet["declare_stmt"].Add("empty");
            followSet["func_definition"].Add("empty");
        }

        public static void showGrammer()
        {
            Console.WriteLine(grammer.ToString());
            foreach (string s in grammer.Keys)
            {
                foreach (List<Symbol> l in grammer[s])
                {
                    string production = "";
                    foreach (Symbol s1 in l)
                    {
                        production = production + " " + s1.name;
                    }
                    Console.WriteLine(production);
                }
            }
        }

        private static void initItemset()
        {
            try
            {
                //测试1
                ItemSet i = new ItemSet(1);
                List<Symbol> l1 = new List<Symbol>();
                l1.Add(new Nonterminal(-100, "prog"));
                i.itemset.Add(new Item("E", l1, 0));
                i.getCompleteClosure();
                Console.WriteLine(i.ToString());
                ItemSet.states_lst.Add(i);
                i.addStateToStateLst();
            }
            catch (ParserException pe)
            {
                MessageBox.Show(pe.getExceptionMsg());
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
            List<Symbol> temp = new List<Symbol>();
            temp.Add(new Nonterminal(-100, "prog", false));
            analysis_table[2]["empty"] = new Action("reduce", "E", temp);
        }

        public static void constructGrammer()
        {
            E_Grammer();
            prog_Grammer();
            extern_declaration_Grammer();
            func_definition_Grammer();
            func_declaratee_Grammer();
            param_lst_Grammer();
            param_declaration_Grammer();
            stmt_lst_Grammer();
            stmt_Grammer();
            compound_stmt_Grammer();
            declare_stmt_Grammer();
            declaratee_lst_Grammer();
            declaratee_Grammer();
            type_Grammer();
            initializer_Grammer();
            initializer_lst_Grammer();
            if_stmt_Grammer();
            more_ifelse_Grammer();
            else_stmt_Grammer();
            while_stmt_Grammer();
            while_compound_stmt_Grammer();
            while_stmt_lst_Grammer();
            while_substmt_Grammer();
            while_if_stmt_Grammer();
            while_more_ifelse_Grammer();
            while_else_stmt_Grammer();
            read_stmt_Grammer();
            write_stmt_Grammer();
            assign_stmt_Grammer();
            other_assign_Grammer();
            return_stmt_Grammer();
            expr_Grammer();
            logical_expr_Grammer();
            logical_expr_more_Grammer();
            logical_op_Grammer();
            relational_expr_Grammer();
            relational_expr_more_Grammer();
            comparison_op_Grammer();
            simple_expr_Grammer();
            more_term_Grammer();
            add_op_Grammer();
            mul_op_Grammer();
            term_Grammer();
            more_factor_Grammer();
            factor_Grammer();
            more_identifier_Grammer();
            param_values_Grammer();
            number_Grammer();
            expr_stmt_Grammer();
            break_stmt_Grammer();
            continue_stmt_Grammer();
        }

        private static void logical_op_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "||"));
            production2.Add(new Terminal(100, "&&"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["logical_op"] = production_lst;
        }

        private static void logical_expr_more_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "empty"));
            production2.Add(new Nonterminal(-100, "logical_op"));
            production2.Add(new Nonterminal(-100, "relational_expr"));
            production2.Add(new Nonterminal(-100, "logical_expr_more", true));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["logical_expr_more"] = production_lst;
        }

        private static void logical_expr_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "relational_expr"));
            production1.Add(new Nonterminal(-100, "logical_expr_more", true));
            production_lst.Add(production1);
            grammer["logical_expr"] = production_lst;
        }

        private static void continue_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Terminal(100, "continue"));
            production1.Add(new Terminal(100, ";"));
            production_lst.Add(production1);
            grammer["continue_stmt"] = production_lst;
        }

        private static void break_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Terminal(100, "break"));
            production1.Add(new Terminal(100, ";"));
            production_lst.Add(production1);
            grammer["break_stmt"] = production_lst;
        }

        private static void while_more_ifelse_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "empty"));
            production2.Add(new Terminal(-100, "else"));
            production2.Add(new Nonterminal(-100, "while_else_stmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["while_more_ifelse"] = production_lst;
        }

        private static void while_stmt_lst_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "while_substmt"));
            production1.Add(new Nonterminal(-100, "while_stmt_lst"));
            production2.Add(new Nonterminal(-100, "while_substmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["while_stmt_lst"] = production_lst;
        }

        private static void while_else_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "while_if_stmt"));
            production2.Add(new Nonterminal(-100, "while_compound_stmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["while_else_stmt"] = production_lst;
        }

        private static void while_if_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Terminal(100, "if"));
            production1.Add(new Terminal(100, "("));
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ")"));
            production1.Add(new Nonterminal(-100, "while_compound_stmt"));
            production1.Add(new Nonterminal(-100, "while_more_ifelse", true));
            production_lst.Add(production1);
            grammer["while_if_stmt"] = production_lst;
        }

        private static void while_substmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            List<Symbol> production5 = new List<Symbol>();
            List<Symbol> production6 = new List<Symbol>();
            List<Symbol> production7 = new List<Symbol>();
            List<Symbol> production8 = new List<Symbol>();
            List<Symbol> production9 = new List<Symbol>();
            List<Symbol> production10 = new List<Symbol>();
            List<Symbol> production11 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "while_if_stmt"));
            production2.Add(new Nonterminal(-100, "while_stmt"));
            production3.Add(new Nonterminal(-100, "read_stmt"));
            production4.Add(new Nonterminal(-100, "write_stmt"));
            production5.Add(new Nonterminal(-100, "assign_stmt"));
            production6.Add(new Nonterminal(-100, "declare_stmt"));
            production7.Add(new Nonterminal(-100, "while_compound_stmt"));
            production8.Add(new Nonterminal(-100, "return_stmt"));
            production9.Add(new Nonterminal(-100, "expr_stmt"));
            production10.Add(new Nonterminal(-100, "break_stmt"));
            production11.Add(new Nonterminal(-100, "continue_stmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            production_lst.Add(production5);
            production_lst.Add(production6);
            production_lst.Add(production7);
            production_lst.Add(production8);
            production_lst.Add(production9);
            production_lst.Add(production10);
            production_lst.Add(production11);
            grammer["while_substmt"] = production_lst;
        }

        private static void while_compound_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "{"));
            production1.Add(new Nonterminal(-100, "while_stmt_lst"));
            production1.Add(new Terminal(100, "}"));
            production2.Add(new Terminal(100, "{"));
            production2.Add(new Terminal(100, "}"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["while_compound_stmt"] = production_lst;
        }

        public static void E_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production = new List<Symbol>();
            production.Add(new Nonterminal(-100, "prog"));
            production_lst.Add(production);
            grammer["E"] = production_lst;
        }

        public static void prog_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "extern_declaration"));
            production2.Add(new Nonterminal(-100, "extern_declaration"));
            production2.Add(new Nonterminal(-100, "prog"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["prog"] = production_lst;
        }

        public static void extern_declaration_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "declare_stmt"));
            production2.Add(new Nonterminal(-100, "func_definition"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["extern_declaration"] = production_lst;
        }

        public static void func_definition_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production.Add(new Nonterminal(-100, "type"));
            production.Add(new Nonterminal(-100, "func_declaratee"));
            production.Add(new Terminal(100, "{"));
            production.Add(new Nonterminal(-100, "stmt_lst"));
            production.Add(new Terminal(100, "}"));
            production2.Add(new Nonterminal(-100, "type"));
            production2.Add(new Nonterminal(-100, "func_declaratee"));
            production2.Add(new Terminal(100, "{"));
            production2.Add(new Terminal(100, "}"));
            production_lst.Add(production);
            production_lst.Add(production2);
            grammer["func_definition"] = production_lst;
        }

        public static void func_declaratee_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production.Add(new Terminal(100, "identifier"));
            production.Add(new Terminal(100, "("));
            production.Add(new Terminal(100, ")"));
            production2.Add(new Terminal(100, "identifier"));
            production2.Add(new Terminal(100, "("));
            production2.Add(new Nonterminal(-100, "param_lst"));
            production2.Add(new Terminal(100, ")"));
            production_lst.Add(production);
            production_lst.Add(production2);
            grammer["func_declaratee"] = production_lst;
        }

        public static void param_lst_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "param_declaration"));
            production1.Add(new Terminal(100, ","));
            production1.Add(new Nonterminal(-100, "param_lst"));
            production2.Add(new Nonterminal(-100, "param_declaration"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["param_lst"] = production_lst;
        }

        public static void param_declaration_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production.Add(new Nonterminal(-100, "type"));
            production.Add(new Terminal(100, "identifier"));
            production2.Add(new Nonterminal(-100, "type"));
            production2.Add(new Terminal(100, "identifier"));
            production2.Add(new Terminal(100, "["));
            production2.Add(new Nonterminal(-100, "simple_expr"));
            production2.Add(new Terminal(100, "]"));
            production_lst.Add(production);
            production_lst.Add(production2);
            grammer["param_declaration"] = production_lst;
        }

        public static void stmt_lst_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "stmt"));
            production1.Add(new Nonterminal(-100, "stmt_lst"));
            production2.Add(new Nonterminal(-100, "stmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["stmt_lst"] = production_lst;
        }

        public static void stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            List<Symbol> production5 = new List<Symbol>();
            List<Symbol> production6 = new List<Symbol>();
            List<Symbol> production7 = new List<Symbol>();
            List<Symbol> production8 = new List<Symbol>();
            List<Symbol> production9 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "if_stmt"));
            production2.Add(new Nonterminal(-100, "while_stmt"));
            production3.Add(new Nonterminal(-100, "read_stmt"));
            production4.Add(new Nonterminal(-100, "write_stmt"));
            production5.Add(new Nonterminal(-100, "assign_stmt"));
            production6.Add(new Nonterminal(-100, "declare_stmt"));
            production7.Add(new Nonterminal(-100, "compound_stmt"));
            production8.Add(new Nonterminal(-100, "return_stmt"));
            production9.Add(new Nonterminal(-100, "expr_stmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            production_lst.Add(production5);
            production_lst.Add(production6);
            production_lst.Add(production7);
            production_lst.Add(production8);
            production_lst.Add(production9);
            grammer["stmt"] = production_lst;
        }

        public static void compound_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "{"));
            production1.Add(new Nonterminal(-100, "stmt_lst"));
            production1.Add(new Terminal(100, "}"));
            production2.Add(new Terminal(100, "{"));
            production2.Add(new Terminal(100, "}"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["compound_stmt"] = production_lst;
        }

        public static void declare_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "type"));
            production1.Add(new Nonterminal(-100, "declaratee_lst"));
            production1.Add(new Terminal(-100, ";"));
            production_lst.Add(production1);
            grammer["declare_stmt"] = production_lst;
        }

        public static void declaratee_lst_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "declaratee"));
            production1.Add(new Terminal(-100, ","));
            production1.Add(new Nonterminal(-100, "declaratee_lst"));
            production2.Add(new Nonterminal(-100, "declaratee"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["declaratee_lst"] = production_lst;
        }

        public static void type_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            List<Symbol> production5 = new List<Symbol>();
            production1.Add(new Terminal(100, "int"));
            production2.Add(new Terminal(100, "real"));
            production3.Add(new Terminal(100, "void"));
            production4.Add(new Terminal(100, "char"));
            production5.Add(new Terminal(100, "string"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            production_lst.Add(production5);
            grammer["type"] = production_lst;
        }

        public static void declaratee_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            List<Symbol> production5 = new List<Symbol>();
            production1.Add(new Terminal(100, "identifier"));
            production2.Add(new Terminal(100, "identifier"));
            production2.Add(new Terminal(100, "["));
            production2.Add(new Nonterminal(-100, "simple_expr"));
            production2.Add(new Terminal(100, "]"));
            production3.Add(new Terminal(100, "identifier"));
            production3.Add(new Nonterminal(-100, "initializer"));
            production4.Add(new Terminal(100, "identifier"));
            production4.Add(new Terminal(100, "["));
            production4.Add(new Nonterminal(-100, "simple_expr"));
            production4.Add(new Terminal(100, "]"));
            production4.Add(new Nonterminal(-100, "initializer"));
            production5.Add(new Terminal(100, "identifier"));
            production5.Add(new Terminal(100, "["));
            production5.Add(new Terminal(100, "]"));
            production5.Add(new Nonterminal(-100, "initializer"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            production_lst.Add(production5);
            grammer["declaratee"] = production_lst;
        }

        public static void initializer_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "="));
            production1.Add(new Nonterminal(-100, "expr"));
            production2.Add(new Terminal(100, "="));
            production2.Add(new Terminal(100, "{"));
            production2.Add(new Nonterminal(-100, "initializer_lst"));
            production2.Add(new Terminal(100, "}"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["initializer"] = production_lst;
        }

        public static void initializer_lst_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "expr"));
            production2.Add(new Nonterminal(-100, "expr"));
            production2.Add(new Terminal(100, ","));
            production2.Add(new Nonterminal(-100, "initializer_lst"));
            production3.Add(new Nonterminal(-100, "expr"));
            production3.Add(new Terminal(100, ","));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            grammer["initializer_lst"] = production_lst;
        }

        public static void if_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "if"));
            production1.Add(new Terminal(100, "("));
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ")"));
            production1.Add(new Nonterminal(-100, "compound_stmt"));
            production1.Add(new Nonterminal(-100, "more_ifelse", true));
            //production2.Add(new Terminal(100, "if"));
            //production2.Add(new Terminal(100, "("));
            //production2.Add(new Nonterminal(-100, "expr"));
            //production2.Add(new Terminal(100, ")"));
            //production2.Add(new Nonterminal(-100, "stmt"));
            //production2.Add(new Nonterminal(-100, "more_ifelse", true));
            //production_lst.Add(production2);
            production_lst.Add(production1);
            grammer["if_stmt"] = production_lst;
        }

        public static void more_ifelse_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "empty"));
            production2.Add(new Terminal(-100, "else"));
            production2.Add(new Nonterminal(-100, "else_stmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["more_ifelse"] = production_lst;
        }

        public static void else_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "if_stmt"));
            production2.Add(new Nonterminal(-100, "compound_stmt"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["else_stmt"] = production_lst;
        }

        public static void while_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Terminal(100, "while"));
            production1.Add(new Terminal(100, "("));
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ")"));
            production1.Add(new Nonterminal(-100, "while_compound_stmt"));
            production_lst.Add(production1);
            grammer["while_stmt"] = production_lst;
        }

        public static void read_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "read"));
            production1.Add(new Terminal(100, "identifier"));
            production1.Add(new Terminal(100, ";"));
            production2.Add(new Terminal(100, "read"));
            production2.Add(new Terminal(100, "identifier"));
            production2.Add(new Terminal(100, "["));
            production2.Add(new Nonterminal(-100, "simple_expr"));
            production2.Add(new Terminal(100, "]"));
            production2.Add(new Terminal(100, ";"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["read_stmt"] = production_lst;
        }

        public static void write_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Terminal(100, "write"));
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ";"));
            production_lst.Add(production1);
            grammer["write_stmt"] = production_lst;
        }

        public static void expr_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ";"));
            production_lst.Add(production1);
            grammer["expr_stmt"] = production_lst;
        }

        public static void assign_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Terminal(100, "identifier"));
            production1.Add(new Nonterminal(-100, "other_assign"));
            production_lst.Add(production1);
            grammer["assign_stmt"] = production_lst;
        }

        public static void other_assign_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            List<Symbol> production5 = new List<Symbol>();
            List<Symbol> production6 = new List<Symbol>();
            List<Symbol> production7 = new List<Symbol>();
            List<Symbol> production8 = new List<Symbol>();
            List<Symbol> production9 = new List<Symbol>();
            List<Symbol> production10 = new List<Symbol>();
            production1.Add(new Terminal(100, "="));
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ";"));
            production2.Add(new Terminal(100, "+="));
            production2.Add(new Nonterminal(-100, "expr"));
            production2.Add(new Terminal(100, ";"));
            production3.Add(new Terminal(100, "-="));
            production3.Add(new Nonterminal(-100, "expr"));
            production3.Add(new Terminal(100, ";"));
            production4.Add(new Terminal(100, "*="));
            production4.Add(new Nonterminal(-100, "expr"));
            production4.Add(new Terminal(100, ";"));
            production5.Add(new Terminal(100, "/="));
            production5.Add(new Nonterminal(-100, "expr"));
            production5.Add(new Terminal(100, ";"));

            production6.Add(new Terminal(100, "["));
            production6.Add(new Nonterminal(-100, "simple_expr"));
            production6.Add(new Terminal(100, "]"));
            production6.Add(new Terminal(100, "="));
            production6.Add(new Nonterminal(-100, "expr"));
            production6.Add(new Terminal(100, ";"));
            production7.Add(new Terminal(100, "["));
            production7.Add(new Nonterminal(-100, "simple_expr"));
            production7.Add(new Terminal(100, "]"));
            production7.Add(new Terminal(100, "+="));
            production7.Add(new Nonterminal(-100, "expr"));
            production7.Add(new Terminal(100, ";"));
            production8.Add(new Terminal(100, "["));
            production8.Add(new Nonterminal(-100, "simple_expr"));
            production8.Add(new Terminal(100, "]"));
            production8.Add(new Terminal(100, "-="));
            production8.Add(new Nonterminal(-100, "expr"));
            production8.Add(new Terminal(100, ";"));
            production9.Add(new Terminal(100, "["));
            production9.Add(new Nonterminal(-100, "simple_expr"));
            production9.Add(new Terminal(100, "]"));
            production9.Add(new Terminal(100, "*="));
            production9.Add(new Nonterminal(-100, "expr"));
            production9.Add(new Terminal(100, ";"));
            production10.Add(new Terminal(100, "["));
            production10.Add(new Nonterminal(-100, "simple_expr"));
            production10.Add(new Terminal(100, "]"));
            production10.Add(new Terminal(100, "/="));
            production10.Add(new Nonterminal(-100, "expr"));
            production10.Add(new Terminal(100, ";"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            production_lst.Add(production5);
            production_lst.Add(production6);
            production_lst.Add(production7);
            production_lst.Add(production8);
            production_lst.Add(production9);
            production_lst.Add(production10);
            grammer["other_assign"] = production_lst;
        }

        public static void return_stmt_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "return"));
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ";"));
            production2.Add(new Terminal(100, "return"));
            production2.Add(new Terminal(100, ";"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["return_stmt"] = production_lst;
        }

        public static void expr_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "logical_expr"));
            production_lst.Add(production1);
            grammer["expr"] = production_lst;
        }

        public static void relational_expr_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "simple_expr"));
            production1.Add(new Nonterminal(-100, "relational_expr_more", true));
            production_lst.Add(production1);
            grammer["relational_expr"] = production_lst;
        }

        public static void relational_expr_more_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "empty"));
            production2.Add(new Nonterminal(-100, "comparison_op"));
            production2.Add(new Nonterminal(-100, "simple_expr"));
            production2.Add(new Nonterminal(-100, "relational_expr_more", true));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["relational_expr_more"] = production_lst;
        }

        public static void comparison_op_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            List<Symbol> production5 = new List<Symbol>();
            List<Symbol> production6 = new List<Symbol>();
            List<Symbol> production7 = new List<Symbol>();
            List<Symbol> production8 = new List<Symbol>();
            List<Symbol> production9 = new List<Symbol>();
            production1.Add(new Terminal(100, "<"));
            production2.Add(new Terminal(100, "=="));
            production3.Add(new Terminal(100, ">"));
            production4.Add(new Terminal(100, ">="));
            production5.Add(new Terminal(100, "<="));
            production6.Add(new Terminal(100, "<>"));
            production7.Add(new Terminal(100, "!="));
            production8.Add(new Terminal(100, "||"));
            production9.Add(new Terminal(100, "&&"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            production_lst.Add(production5);
            production_lst.Add(production6);
            production_lst.Add(production7);
            production_lst.Add(production8);
            production_lst.Add(production9);
            grammer["comparison_op"] = production_lst;
        }

        public static void simple_expr_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "term"));
            production1.Add(new Nonterminal(-100, "more_term", true));
            production_lst.Add(production1);
            grammer["simple_expr"] = production_lst;
        }

        public static void more_term_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "empty"));
            production2.Add(new Nonterminal(-100, "add_op"));
            production2.Add(new Nonterminal(-100, "term"));
            production2.Add(new Nonterminal(-100, "more_term", true));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["more_term"] = production_lst;
        }

        public static void add_op_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "+"));
            production2.Add(new Terminal(100, "-"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["add_op"] = production_lst;
        }

        public static void mul_op_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "*"));
            production2.Add(new Terminal(100, "/"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["mul_op"] = production_lst;
        }

        public static void term_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "factor"));
            production1.Add(new Nonterminal(-100, "more_factor", true));
            production_lst.Add(production1);
            grammer["term"] = production_lst;
        }

        public static void more_factor_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "empty"));
            production2.Add(new Nonterminal(-100, "mul_op"));
            production2.Add(new Nonterminal(-100, "factor"));
            production2.Add(new Nonterminal(-100, "more_factor", true));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["more_factor"] = production_lst;
        }

        public static void factor_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production0 = new List<Symbol>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            List<Symbol> production5 = new List<Symbol>();
            List<Symbol> production6 = new List<Symbol>();
            production0.Add(new Terminal(100, "+"));
            production0.Add(new Nonterminal(-100, "factor"));
            production1.Add(new Terminal(100, "-"));
            production1.Add(new Nonterminal(-100, "factor"));
            production2.Add(new Nonterminal(-100, "number"));
            production3.Add(new Terminal(100, "identifier"));
            production3.Add(new Nonterminal(-100, "more_identifier", true));
            production4.Add(new Terminal(100, "("));
            production4.Add(new Nonterminal(-100, "expr"));
            production4.Add(new Terminal(100, ")"));
            production5.Add(new Terminal(100, "_string_content"));
            production6.Add(new Terminal(100, "_char_content"));
            production_lst.Add(production0);
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            production_lst.Add(production5);
            production_lst.Add(production6);
            grammer["factor"] = production_lst;
        }

        public static void more_identifier_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            List<Symbol> production3 = new List<Symbol>();
            List<Symbol> production4 = new List<Symbol>();
            production1.Add(new Terminal(100, "empty"));
            production2.Add(new Terminal(100, "["));
            production2.Add(new Nonterminal(-100, "simple_expr"));
            production2.Add(new Terminal(100, "]"));
            production3.Add(new Terminal(100, "("));
            production3.Add(new Terminal(100, ")"));
            production4.Add(new Terminal(100, "("));
            production4.Add(new Nonterminal(-100, "param_values"));
            production4.Add(new Terminal(100, ")"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            production_lst.Add(production3);
            production_lst.Add(production4);
            grammer["more_identifier"] = production_lst;
        }

        public static void param_values_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Nonterminal(-100, "expr"));
            production1.Add(new Terminal(100, ","));
            production1.Add(new Nonterminal(-100, "param_values"));
            production2.Add(new Nonterminal(-100, "expr"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["param_values"] = production_lst;
        }

        public static void number_Grammer()
        {
            List<List<Symbol>> production_lst = new List<List<Symbol>>();
            List<Symbol> production1 = new List<Symbol>();
            List<Symbol> production2 = new List<Symbol>();
            production1.Add(new Terminal(100, "real_number"));
            production2.Add(new Terminal(100, "integer"));
            production_lst.Add(production1);
            production_lst.Add(production2);
            grammer["number"] = production_lst;
        }

        private static void addTerminalsToSymbols()
        {
            Symbol.all_symbols.Add("E");
            Symbol.all_symbols.Add("if");
            Symbol.all_symbols.Add("else");
            Symbol.all_symbols.Add("int");
            Symbol.all_symbols.Add("while");
            Symbol.all_symbols.Add("read");
            Symbol.all_symbols.Add("write");
            Symbol.all_symbols.Add("real");
            Symbol.all_symbols.Add("+");
            Symbol.all_symbols.Add("-");
            Symbol.all_symbols.Add("*");
            Symbol.all_symbols.Add("/");
            Symbol.all_symbols.Add("=");
            Symbol.all_symbols.Add("+=");
            Symbol.all_symbols.Add("-=");
            Symbol.all_symbols.Add("*=");
            Symbol.all_symbols.Add("/=");
            Symbol.all_symbols.Add("<");
            Symbol.all_symbols.Add(">");
            Symbol.all_symbols.Add("<=");
            Symbol.all_symbols.Add(">=");
            Symbol.all_symbols.Add("!=");
            Symbol.all_symbols.Add("!");
            Symbol.all_symbols.Add("&&");
            Symbol.all_symbols.Add("||");
            Symbol.all_symbols.Add("(");
            Symbol.all_symbols.Add(")");
            Symbol.all_symbols.Add("{");
            Symbol.all_symbols.Add("}");
            Symbol.all_symbols.Add("[");
            Symbol.all_symbols.Add("]");
            Symbol.all_symbols.Add("char");
            Symbol.all_symbols.Add("string");
            Symbol.all_symbols.Add("_char_content");
            Symbol.all_symbols.Add("_string_content");
            Symbol.all_symbols.Add(",");
            Symbol.all_symbols.Add(";");
            Symbol.all_symbols.Add("\"");
            Symbol.all_symbols.Add("'");
            Symbol.all_symbols.Add("void");
        }
    }
}
