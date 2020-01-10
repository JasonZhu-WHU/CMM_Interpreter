using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.Configuration;
using System.Xml;
using System.Windows;

namespace CMM_Interpreter
{
    class Parser
    {
        public static Stack<object> stack_to_parse = new Stack<object>();
        public static List<Token> all_tokens = new List<Token>();

        public static bool parse_tokens()
        {
            try
            {
                all_tokens.Clear();
                stack_to_parse = new Stack<object>();
                StateStackElement initial_state = new StateStackElement(1);
                stack_to_parse.Push(initial_state);
                foreach(Token t in MainWindow.allTokens)
                {
                    all_tokens.Add(t);
                }
                while(all_tokens.Count != 0)
                {
                    //获取分析栈的状态（第一个状态元素）
                    StateStackElement first = (StateStackElement)(stack_to_parse.Peek());
                    //获取剩余tokens的第一个
                    Token t = all_tokens[0];
                    //根据第一个token，构建一个栈元素（type_code自动赋予，用来判断token是哪一种stackelement）
                    StackElement ele = (StackElement)Tools.transformTokenToSymbol(t);
                    //结合分析表，获取应该进行的语法动作
                    Action a = getActionByTransformedToken(first.state, ele.type_code, t.content);
                    //进行该语法动作
                    conductAction(a, t, ele, first);
                }
                //下面这部分就与tokens无关了，如果上面tokens没有出错，就会进入下面的收尾部分：
                while (true)
                {
                    //这部分的意义主要是在于，tokens一旦为空，上面的while循环自动结束，但是这并不意味着语法分析的结束
                    //程序只是进行了最后一次真正token的移入，但是还有很多规约，甚至是报错的工作要进行
                    //这一部分每次都会接收到一个我们固定的empty，如果程序认为它不能在empty下规约，那么要么是到了最后的E，要么是报错
                    StateStackElement last_state = (StateStackElement)stack_to_parse.Peek();
                    Action last_action = GrammerConfig.analysis_table[last_state.state]["empty"];
                    //执行语法动作，应该是一个规约，否则就是已经规约到头err，那就是结束err
                    if (last_action.action == "reduce")
                    {
                        //规约产生的肯定都是非终结符
                        NonterminalStackElement e = new NonterminalStackElement(-1, last_action.left);
                        int count = last_action.right.Count;
                        for (int i = 0; i < count * 2; i++)
                        {
                            if (i % 2 == 1)
                            {
                                StackElement s = (StackElement)stack_to_parse.Pop();
                                s.layersIncreaseRecursively();
                                e.branches.Insert(0, s);
                            }
                            else
                            {
                                stack_to_parse.Pop();
                            }
                        }
                        StateStackElement first_now = (StateStackElement)(stack_to_parse.Peek());
                        Console.WriteLine("last_action.left:" + last_action.left);
                        Action aa = getActionByTransformedToken(first_now.state, e.type_code, last_action.left);
                        if (aa.action == "shift")
                        {
                            stack_to_parse.Push(e);
                            stack_to_parse.Push(new StateStackElement(aa.new_state));
                        }
                        else if (aa.action == "error")
                        {
                            stack_to_parse.Push(e);
                            stack_to_parse.Push(new StateStackElement(0));
                            break;
                        }
                        else if (aa.action == "acc")
                        {
                            stack_to_parse.Push(e);
                            stack_to_parse.Push(new StateStackElement(0));
                            break;
                        }
                        else
                        {
                            throw new ParserException("规约之后难道不是移进吗");
                        }
                    }
                    else if (last_action.action == "error")
                    {
                        throw new ParserException("程序定义不完整或者出现错误");
                    }
                    else if (last_action.action == "acc")
                    {
                        stack_to_parse.Push(new StateStackElement(0));
                        break;
                    }
                    else if (last_action.action == "special action")
                    {
                        throw new ParserException("程序定义不完整或者出现错误");
                    }
                    else
                    {
                        MessageBox.Show(last_action.action);
                        throw new ParserException("不可能的情况");
                    }
                }
                printStack();
                AbstractSyntaxTree ast_window = new AbstractSyntaxTree();
                if (MainWindow.grammer_output)
                {
                    ast_window.Show();
                }
                return true;
            }
            catch(ParserException pe)
            {
                MessageBox.Show(pe.getExceptionMsg());
                return false;
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.ToString());
                return false;
            }
        }

        private static void printStack()
        {
            Stack<StackElement> temp = new Stack<StackElement>();
            foreach(object o in stack_to_parse)
            {
                StackElement s_ele = (StackElement)o;
                temp.Push(s_ele);
            }
            foreach (StackElement s in temp)
            {
                Console.WriteLine(s.ToString());
            }
        }

        //执行五种语法动作之一   语法动作 - 目前的第一个token - 根据token构建的栈元素
        private static void conductAction(Action a, Token t, StackElement ele, StateStackElement first)
        {
            if (a.action == "error")
            {
                throw new ParserException("第" + t.lineNum + "行：" + "遇到无法解析的语法成分:" + Token.getALineOfTokens(t.lineNum));
            }
            else if (a.action == "shift")
            {
                stack_to_parse.Push(ele);
                stack_to_parse.Push(new StateStackElement(a.new_state));
                all_tokens.RemoveAt(0);
            }
            else if (a.action == "reduce")
            {
                //规约产生的肯定都是非终结符
                NonterminalStackElement e = new NonterminalStackElement(-1, a.left);
                int count = a.right.Count;
                for (int i = 0; i < count*2; i++)
                {
                    //第偶数个pop出去的元素不是代表状态的元素，是需要规约的元素
                    if (i % 2 == 1)
                    {
                        //保证是子结点是顺序的
                        StackElement s = (StackElement)stack_to_parse.Pop();
                        s.layersIncreaseRecursively();
                        e.branches.Insert(0, s);
                    }
                    else
                    {
                        stack_to_parse.Pop();
                    }
                }
                StateStackElement first_now = (StateStackElement)(stack_to_parse.Peek());
                Action aa = getActionByTransformedToken(first_now.state, e.type_code, a.left);
                if(aa.action == "shift")
                {
                    stack_to_parse.Push(e);
                    stack_to_parse.Push(new StateStackElement(aa.new_state));
                }
                else
                {
                    throw new ParserException("规约之后难道不是移进吗");
                }
            }
            else if(a.action == "special action")
            {
                //根据当前状态，将自动移入的nullable非终结符，获取分析表要我们执行的动作
                Action next_action = GrammerConfig.analysis_table[first.state][a.auto_shifted];
                if (next_action.action == "shift")
                {
                    stack_to_parse.Push(new NonterminalStackElement(-1, a.auto_shifted));
                    stack_to_parse.Push(new StateStackElement(next_action.new_state));
                }
                else
                {
                    throw new ParserException("special action自动移入才对呀");
                }
            }
            else
            {
                throw new ParserException("还未实现acc");
            }
        }

        //根据语法分析表，利用当前状态，准备入栈的StackElement的种类，以及content生成动作
        public static Action getActionByTransformedToken(int state, int type_code, string content)
        {
            if (type_code == 1)
            {
                return GrammerConfig.analysis_table[state]["identifier"];
            }
            else if (type_code == 2)
            {
                return GrammerConfig.analysis_table[state]["integer"];
            }
            //其他终结符
            else if (type_code == 4)
            {
                return GrammerConfig.analysis_table[state][content];
            }
            else if (type_code == 5)
            {
                return GrammerConfig.analysis_table[state]["real_number"];
            }
            else if (type_code == 7)
            {
                return GrammerConfig.analysis_table[state]["_char_content"];
            }
            else if (type_code == 8)
            {
                return GrammerConfig.analysis_table[state]["_string_content"];
            }
            //非终结符
            else if (type_code == 3)
            {
                return GrammerConfig.analysis_table[state][content];
            }
            else
            {
                throw new ParserException("Parser.cs中出现不可能的情况");
            }
        }
    }
}
