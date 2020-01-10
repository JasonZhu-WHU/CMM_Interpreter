using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class ExecutorTools
    {
        //传入代表数组长度的simple_expr，返回int类型数组长度
        //检查是否是正整数
        public static int get_array_len_from_simple_expr(NonterminalStackElement simple_expr, List<Dictionary<string, Value>> bindings_stack)
        {
            if(simple_expr.name != "simple_expr")
            {
                throw new ExecutorException("调用了ExecutorTools类的get_array_len_from_simple_expr方法，但是传入参数却并不是simple_expr");
            }
            Value array_len_v = Evaluator.eval_simple_expr(simple_expr, bindings_stack);
            if (array_len_v.type != "int")
            {
                throw new ExecutorException("数组长度应当是整数类型");
            }
            IntValue int_array_len = (IntValue)array_len_v;
            int array_len = int_array_len.value;
            if (array_len <= 0)
            {
                throw new ExecutorException("数组长度应当是正数");
            }
            return array_len;
        }

        //传入type和Value，尝试返回type类型的Value
        //
        public static Value adjustType(string type, Value v)
        {
            if(v.type == type)
            {
                return v;
            }
            else if(Value.computable_types.Contains(type) && Value.computable_types.Contains(v.type))
            {
                if(type == "int" && v.type == "real")
                {
                    return new IntValue("int", true, ((int)((RealValue)v).value).ToString(), v.line_num);
                }
                else if (type == "int" && v.type == "number")
                {
                    return new IntValue("int", true, ((int)((NumberValue)v).value).ToString(), v.line_num);
                }
                else if (type == "real" && v.type == "int")
                {
                    return new RealValue("real", true, ((double)((IntValue)v).value).ToString(), v.line_num);
                }
                else if (type == "real" && v.type == "int")
                {
                    return new RealValue("real", true, ((double)((IntValue)v).value).ToString(), v.line_num);
                }
                else if (type == "number" && v.type == "int")
                {
                    return new NumberValue("number", true, ((double)((IntValue)v).value).ToString(), v.line_num);
                }
                else if (type == "number" && v.type == "real")
                {
                    return new NumberValue("number", true, (((RealValue)v).value).ToString(), v.line_num);
                }
                else
                {
                    throw new ExecutorException("Impossible situation");
                }
            }
            else
            {
                throw new ExecutorException("类型错误", v.line_num);
            }
        }

        //执行函数（传入参数，返回类型等等）
        //注意创建一个新的bindings和Stack，且这个Stack目前只有globalBingdings->newBindings
        //可能return null-->（仅在void函数中可能）
        public static Value executeFunction(FuncValue func_value, List<Value> arguments_lst, int linenum)
        {
            //可以看到，所有的东西都是局部变量，因此函数调用结束这些Bindings都会自动free
            List<Dictionary<string, Value>> binding_stack = new List<Dictionary<string, Value>>();
            Dictionary<string, Value> local_bindings = new Dictionary<string, Value>();
            binding_stack.Add(Executor.globalSymbolTable);
            binding_stack.Add(local_bindings);
            //获得返回类型
            NonterminalStackElement return_type_node = (NonterminalStackElement)func_value.func_def_node.branches[0];
            OtherTerminalStackElement specific_return_type_node = (OtherTerminalStackElement)return_type_node.branches[0];
            string return_type = specific_return_type_node.content;
            //获得函数名
            NonterminalStackElement func_declaratee = (NonterminalStackElement)func_value.func_def_node.branches[1];
            IdentifierStackElement identifier_node = (IdentifierStackElement)func_declaratee.branches[0];
            string func_name = identifier_node.content;
            List<string> params_lst = new List<string>();
            //part1 传参

            //先把实参传进来（加到local_bindings顺便做一个类型检查，函数签名和传入参数）
            //func_declaratee →identifier()|identifier(param_lst)
            if (func_declaratee.branches.Count == 4)
            {
                int index = 0;
                NonterminalStackElement param_lst_node = (NonterminalStackElement)func_declaratee.branches[2];
                //param_lst→param_declaration,param_lst | param_declaration
                NonterminalStackElement param_declaration_node = (NonterminalStackElement)param_lst_node.branches[0];
                while (param_lst_node.branches.Count == 3)
                {
                    if(arguments_lst.Count <= index)
                    {
                        throw new ExecutorException("传入实参多于形参", linenum);
                    }
                    //param_declaration→type identifier|type identifier[simple_expr]
                    NonterminalStackElement type_node = (NonterminalStackElement)param_declaration_node.branches[0];
                    OtherTerminalStackElement specific_type_node = (OtherTerminalStackElement)type_node.branches[0];
                    IdentifierStackElement identifier_Node = (IdentifierStackElement)param_declaration_node.branches[1];
                    param_lst_node = (NonterminalStackElement)param_lst_node.branches[2];
                    param_declaration_node = (NonterminalStackElement)param_lst_node.branches[0];
                    string type = specific_type_node.content;
                    //传入数组会更特殊
                    if (param_declaration_node.branches.Count == 4)
                    {
                        type += "Array";
                        if (arguments_lst[index].type != type)
                        {
                            throw new ExecutorException("函数形参类型与实际传入参数类型不符", linenum);
                        }
                        NonterminalStackElement simple_expr_node = (NonterminalStackElement)param_declaration_node.branches[3];
                        if (arguments_lst[index].getArrayLen() > get_array_len_from_simple_expr(simple_expr_node, binding_stack))
                        {
                            throw new ExecutorException("传入的数组长度大于对应的形参数组长度", linenum);
                        }
                    }
                    //如果符合要求，那就在local_bindings增加定义
                    if (arguments_lst[index].type == type)
                    {
                        local_bindings.Add(identifier_Node.content, arguments_lst[index]);
                        index++;
                    }
                    else
                    {
                        throw new ExecutorException("函数形参类型与实际传入参数类型不符", linenum);
                    }
                }
                if((index + 1) > arguments_lst.Count)
                {
                    throw new ExecutorException("传入实参少于形参", linenum);
                }
                //最后一个参数，重复一遍上面的工作
                NonterminalStackElement _type_node = (NonterminalStackElement)param_declaration_node.branches[0];
                OtherTerminalStackElement _specific_type_node = (OtherTerminalStackElement)_type_node.branches[0];
                IdentifierStackElement _identifier_Node = (IdentifierStackElement)param_declaration_node.branches[1];
                string _type = _specific_type_node.content;
                //传入数组会更特殊
                if (param_declaration_node.branches.Count == 4)
                {
                    _type += "Array";
                    if (arguments_lst[index].type != _type)
                    {
                        throw new ExecutorException("函数形参类型与实际传入参数类型不符", linenum);
                    }
                    NonterminalStackElement simple_expr_node = (NonterminalStackElement)param_declaration_node.branches[3];
                    if (arguments_lst[index].getArrayLen() > get_array_len_from_simple_expr(simple_expr_node, binding_stack))
                    {
                        throw new ExecutorException("传入的数组长度大于对应的形参数组长度", linenum);
                    }
                }
                //如果符合要求，那就在local_bindings增加定义
                if (arguments_lst[index].type == _type)
                {
                    local_bindings.Add(_identifier_Node.content, arguments_lst[index]);
                    index++;
                }
                else
                {
                    throw new ExecutorException("函数形参类型与实际传入参数类型不符", linenum);
                }
            }
            //func_declaratee →identifier() 这样的话local_bindings就不需要任何东西了
            else
            {
                if(arguments_lst.Count != 0)
                {
                    throw new ExecutorException("函数" + identifier_node.content + "不需要接受任何参数", linenum);
                }
                //do nothing
            }

            //part2 实际执行
            //func_definition→type func_declaratee {stmt_lst} | type func_declaratee {}
            if (func_value.func_def_node.branches.Count == 4)
            {
                if(return_type == "void")
                {
                    return null;
                }
                else
                {
                    throw new ExecutorException("函数" + identifier_node.content + "没有返回任何值", linenum);
                }
            }
            //func_definition → type func_declaratee {stmt_lst}
            //stmt_lst → stmt stmt_lst | stmt
            NonterminalStackElement stmt_lst_node = (NonterminalStackElement)func_value.func_def_node.branches[3];
            Value return_value = executeStmtLst(stmt_lst_node, binding_stack);

            //如果返回一个NullValue或者Null，那要检查是否是void
            if (return_value == null || return_value.type == "null")
            {
                if (return_type == "void")
                {
                    return null;
                }
                else
                {
                    throw new ExecutorException("函数" + func_name + "的返回值必须是" + return_type + "类型", linenum);
                }
            }
            //返回不是Null，返回了一个实际的Value，那就要检查类型是否相符，检查返回类型不是void
            else
            {
                if(return_type == "void")
                {
                    throw new ExecutorException("函数" + func_name + "的返回值必须是null", linenum);
                }
                else
                {
                    try
                    {
                        Value v = adjustType(return_type, return_value);
                        return v;
                    }
                    //重新捕获调整再抛出
                    catch (ExecutorException ee)
                    {
                        throw new ExecutorException("函数" + func_name + "的返回值必须是" + return_type + "类型", linenum);
                    }
                    catch (Exception eee)
                    {
                        throw eee;
                    }
                }
            }
        }

        //执行stmt_lst，可能是一个函数的{stmt_lst}，也可能就是一个局部的函数块
        //有三种可能，return 3返回了一个值，return;没有返回值，或者真的没有return语句，分别对应Value，NullValue，Null
        private static Value executeStmtLst(NonterminalStackElement stmt_lst_node, List<Dictionary<string, Value>> binding_stack)
        {
            //func_definition → type func_declaratee {stmt_lst}
            //stmt_lst → stmt stmt_lst | stmt
            NonterminalStackElement stmt_node = (NonterminalStackElement)stmt_lst_node.branches[0];
            NonterminalStackElement specific_stmt_node = (NonterminalStackElement)stmt_node.branches[0];
            while (stmt_lst_node.branches.Count == 2)
            {
                stmt_node = (NonterminalStackElement)stmt_lst_node.branches[0];
                specific_stmt_node = (NonterminalStackElement)stmt_node.branches[0];
                stmt_lst_node = (NonterminalStackElement)stmt_lst_node.branches[1];
                Value result_value_ = executeStmt(specific_stmt_node, binding_stack);
                //执行指令，如果是return语句，就会返回一个Value，哪怕是return;，也会返回一个NullValue
                if (result_value_ != null)
                {
                    result_value_.line_num = specific_stmt_node.linenum;
                    return result_value_;
                }
                //返回Null，不是return语句，那就必然是要继续执行了，不做处理
            }
            stmt_node = (NonterminalStackElement)stmt_lst_node.branches[0];
            specific_stmt_node = (NonterminalStackElement)stmt_node.branches[0];
            Value result_value = executeStmt(specific_stmt_node, binding_stack);
            //有三种可能，return了一个值，return;，或者真的没有return语句，分别对应Value，NullValue，Null
            return result_value;
        }

        //执行某个具体的stmt，
        // stmt → if_stmt| while_stmt|read_stmt| write_stmt|assign_stmt|declare_stmt| →compound_stmt|return_stmt|expr_stmt
        // while_substmt →while_if_stmt| while_stmt|read_stmt| write_stmt|assign_stmt|declare_stmt| →while_compound_stmt|return_stmt|expr_stmt | break_stmt | continue_stmt
        // 有三种可能，return了一个值，return;，或者真的没有return语句，分别对应Value，NullValue，Null
        private static Value executeStmt(NonterminalStackElement specific_stmt_node, List<Dictionary<string, Value>> binding_stack)
        {
            string message = "";
            //if_stmt→if(expr)compound_stmt more_ifelse
            if (specific_stmt_node.name == "if_stmt")
            {
                NonterminalStackElement expr_node = (NonterminalStackElement)specific_stmt_node.branches[2];
                Value expr_value = Evaluator.eval_expr(expr_node, binding_stack);
                if (expr_value.getBoolean())
                {
                    NonterminalStackElement compound_stmt = (NonterminalStackElement)specific_stmt_node.branches[4];
                    return executeStmt(compound_stmt, binding_stack);
                }
                else
                {
                    //more_ifelse→ε|else else_stmt 
                    NonterminalStackElement more_if_else_node = (NonterminalStackElement)specific_stmt_node.branches[5];
                    if (more_if_else_node.branches.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        //else_stmt→if_stmt|compound_stmt
                        NonterminalStackElement else_stmt = (NonterminalStackElement)more_if_else_node.branches[1];
                        NonterminalStackElement specific_else_stmt = (NonterminalStackElement)else_stmt.branches[0];
                        return executeStmt(specific_else_stmt, binding_stack);
                    }
                }
                //emmm突然优雅起来了？？？
            }
            //while_stmt→while(expr)  while_compound_stmt
            else if (specific_stmt_node.name == "while_stmt")
            {
                NonterminalStackElement expr_node = (NonterminalStackElement)specific_stmt_node.branches[2];
                Value expr_value = Evaluator.eval_expr(expr_node, binding_stack);
                Dictionary<string, Value> new_bindings = new Dictionary<string, Value>();
                binding_stack.Add(new_bindings);
                Executor.while_indexes.Add(binding_stack.Count);
                //while_compound_stmt→{while_stmt_lst}|{}
                //while_stmt_lst →while_substmt while_stmt_lst | while_substmt
                NonterminalStackElement while_compound_stmt_node = (NonterminalStackElement)specific_stmt_node.branches[4];
                while (expr_value.getBoolean())
                {
                    if (while_compound_stmt_node.branches.Count == 2)
                    {
                        //do nothing
                    }
                    //while_compound_stmt→{while_stmt_lst}
                    else
                    {
                        NonterminalStackElement while_stmt_lst_node = (NonterminalStackElement)while_compound_stmt_node.branches[1];
                        Value v = executeStmtLst(while_stmt_lst_node, binding_stack);
                        //若提前执行return语句，直接结束while循环
                        if(v != null)
                        {
                            binding_stack.RemoveAt(binding_stack.Count - 1);
                            return v;
                        }
                    }
                    expr_value = Evaluator.eval_expr(expr_node, binding_stack);
                }
                binding_stack.RemoveAt(binding_stack.Count - 1);
                return null;
            }
            //read_stmt→read identifier;| read identifier [ simple_expr ] ;
            else if (specific_stmt_node.name == "read_stmt")
            {
                IdentifierStackElement identifier_node = (IdentifierStackElement)specific_stmt_node.branches[1];
                if(specific_stmt_node.branches.Count == 3)
                {
                    Dictionary<string, Value> bindings = findBindings(binding_stack, identifier_node.content);
                    if(bindings != null)
                    {
                        string type = bindings[identifier_node.content].type;
                        if (type.Contains("func") || type.Contains("bool"))
                        {
                            throw new ExecutorException("请检查您的读入目标是否有误，不可读入函数或者布尔类型值", specific_stmt_node.linenum);
                        }
                        else if (type.Contains("Array"))
                        {
                            int len = bindings[identifier_node.content].getArrayLen();
                            bindings[identifier_node.content] = ReaderHelper.read_array_value(type, identifier_node.linenum, len);
                        }
                        else
                        {
                            bindings[identifier_node.content] = ReaderHelper.read_single_value(type, identifier_node.linenum);
                        }
                    }
                    else
                    {
                        throw new ExecutorException("未定义的标识符" + identifier_node.content, identifier_node.linenum);
                    }
                }
                else
                {
                    Dictionary<string, Value> bindings = findBindings(binding_stack, identifier_node.content);
                    if (bindings != null)
                    {
                        string type = bindings[identifier_node.content].type;
                        if (!type.Contains("Array"))
                        {
                            throw new ExecutorException("请检查您的读入目标是否有误，该标识符并不对应数组", specific_stmt_node.linenum);
                        }
                        NonterminalStackElement simple_expr_node = (NonterminalStackElement)specific_stmt_node.branches[3];
                        int index = get_array_len_from_simple_expr(simple_expr_node, binding_stack);
                        type = type.Substring(0, type.Length - 5);
                        Value read_value = ReaderHelper.read_single_value(type, identifier_node.linenum);
                        bindings[identifier_node.content].changeValueOfArray(index, read_value.getString());
                    }
                    else
                    {
                        throw new ExecutorException("未定义的标识符" + identifier_node.content, identifier_node.linenum);
                    }
                }
                return null;
            }
            //write_stmt→write expr;
            else if (specific_stmt_node.name == "write_stmt")
            {
                NonterminalStackElement expr_node = (NonterminalStackElement)specific_stmt_node.branches[1];
                Value v = Evaluator.eval_expr(expr_node, binding_stack);
                if(v.type == "func")
                {
                    throw new ExecutorException("不支持write函数", specific_stmt_node.linenum);
                }
                WriterHelper.write_value(v, specific_stmt_node.linenum);
                return null;
            }
            //assign_stmt→identifier other_assign
            else if (specific_stmt_node.name == "assign_stmt")
            {
                IdentifierStackElement identifier_node = (IdentifierStackElement)specific_stmt_node.branches[0];
                NonterminalStackElement other_assign_node = (NonterminalStackElement)specific_stmt_node.branches[1];
                //other_assign→= expr; | [simple_expr] = expr;
                if(other_assign_node.branches.Count == 3)
                {
                    NonterminalStackElement expr_node = (NonterminalStackElement)other_assign_node.branches[1];
                    Dictionary<string, Value> bindings = findBindings(binding_stack, identifier_node.content);
                    if (bindings != null)
                    {
                        string type = bindings[identifier_node.content].type;
                        if (type.Contains("func"))
                        {
                            throw new ExecutorException("不可给函数赋值", identifier_node.linenum);
                        }
                        else if (type.Contains("Array"))
                        {
                            throw new ExecutorException("不可给数组类型赋单个的值", identifier_node.linenum);
                        }
                        else
                        {
                            Value v = Evaluator.eval_expr(expr_node, binding_stack);
                            bindings[identifier_node.content] = adjustType(type, v);
                        }
                    }
                    else
                    {
                        throw new ExecutorException("未定义的标识符" + identifier_node.content, identifier_node.linenum);
                    }
                }
                //other_assign → [simple_expr] = expr;
                else
                {
                    NonterminalStackElement simple_expr_node = (NonterminalStackElement)other_assign_node.branches[1];
                    NonterminalStackElement expr_node = (NonterminalStackElement)other_assign_node.branches[4];
                    Dictionary<string, Value> bindings = findBindings(binding_stack, identifier_node.content);
                    if (bindings != null)
                    {
                        string type = bindings[identifier_node.content].type;
                        if (type.Contains("func"))
                        {
                            throw new ExecutorException("不可给函数赋值", specific_stmt_node.linenum);
                        }
                        else if (type.Contains("Array"))
                        {
                            Value v = Evaluator.eval_expr(expr_node, binding_stack);
                            type = type.Substring(0, type.Length - 5);
                            //检查赋值的类型和变量类型是否相符
                            v = adjustType(type, v);
                            int index = get_array_len_from_simple_expr(simple_expr_node, binding_stack);
                            bindings[identifier_node.content].changeValueOfArray(index, v.getString());
                        }
                        else
                        {
                            throw new ExecutorException("该变量并不是数组类型，无法赋值", identifier_node.linenum);
                        }
                    }
                    else
                    {
                        throw new ExecutorException("未定义的标识符" + identifier_node.content, identifier_node.linenum);
                    }
                }
                return null;
            }
            //declare_stmt→type declaratee_lst;
            else if (specific_stmt_node.name == "declare_stmt")
            {
                Executor.declareVaraible(specific_stmt_node, binding_stack);
                return null;
            }
            // compound_stmt→{stmt_lst}|{}
            else if (specific_stmt_node.name == "compound_stmt")
            {
                if(specific_stmt_node.branches.Count == 2)
                {
                    return null;
                }
                //{stmt_lst}执行完就要出栈bindings
                else
                {
                    Dictionary<string, Value> new_local_bindings = new Dictionary<string, Value>();
                    binding_stack.Add(new_local_bindings);
                    //stmt_lst → stmt stmt_lst | stmt
                    NonterminalStackElement stmt_lst_node = (NonterminalStackElement)specific_stmt_node.branches[1];
                    Value v = executeStmtLst(stmt_lst_node, binding_stack);
                    //{stmt_lst}执行完就要出栈bindings
                    binding_stack.RemoveAt(binding_stack.Count - 1);
                    return v;
                }
            }
            // return_stmt→return expr; |  return;
            else if (specific_stmt_node.name == "return_stmt")
            {
                // return_stmt→ return;
                if (specific_stmt_node.branches.Count == 2)
                {
                    return new NullValue(specific_stmt_node.linenum);
                }
                // return_stmt→return expr;
                else
                {
                    NonterminalStackElement expr_node = (NonterminalStackElement)specific_stmt_node.branches[1];
                    Value v = Evaluator.eval_expr(expr_node, binding_stack);
                    return v;
                }
            }
            //expr_stmt→expr;
            else if (specific_stmt_node.name == "expr_stmt")
            {
                NonterminalStackElement expr_node = (NonterminalStackElement)specific_stmt_node.branches[0];
                Value v = Evaluator.eval_expr(expr_node, binding_stack);
                if(v == null)
                {
                    return null;
                }
                if(v.type == "func")
                {
                    throw new ExecutorException("函数调用格式错误", specific_stmt_node.linenum);
                }
                else
                {
                    return null;
                }
            }
            //while_if_stmt→if(expr)while_compound_stmt while_more_ifelse
            else if (specific_stmt_node.name == "while_if_stmt")
            {
                NonterminalStackElement expr_node = (NonterminalStackElement)specific_stmt_node.branches[2];
                Value expr_value = Evaluator.eval_expr(expr_node, binding_stack);
                if (expr_value.getBoolean())
                {
                    NonterminalStackElement compound_stmt = (NonterminalStackElement)specific_stmt_node.branches[4];
                    return executeStmt(compound_stmt, binding_stack);
                }
                else
                {
                    NonterminalStackElement more_if_else_node = (NonterminalStackElement)specific_stmt_node.branches[5];
                    //while_more_ifelse→ε|else while_else_stmt
                    if (more_if_else_node.branches.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        //while_else_stmt→while_if_stmt | while_compound_stmt
                        NonterminalStackElement else_stmt = (NonterminalStackElement)more_if_else_node.branches[1];
                        NonterminalStackElement specific_else_stmt = (NonterminalStackElement)else_stmt.branches[0];
                        return executeStmt(specific_else_stmt, binding_stack);
                    }
                }
            }
            //while_compound_stmt→{while_stmt_lst}|{}
            else if (specific_stmt_node.name == "while_compound_stmt")
            {
                if (specific_stmt_node.branches.Count == 2)
                {
                    return null;
                }
                //{stmt_lst}执行完就要出栈bindings
                else
                {
                    Dictionary<string, Value> new_local_bindings = new Dictionary<string, Value>();
                    binding_stack.Add(new_local_bindings);
                    NonterminalStackElement stmt_lst_node = (NonterminalStackElement)specific_stmt_node.branches[1];
                    //while_stmt_lst →while_substmt while_stmt_lst | while_substmt
                    Value v = executeStmtLst(stmt_lst_node, binding_stack);
                    //{stmt_lst}执行完就要出栈bindings
                    binding_stack.RemoveAt(binding_stack.Count - 1);
                    return v;
                }
            }
            //break_stmt→break;
            else if (specific_stmt_node.name == "break_stmt")
            {
                throw new ExecutorException("暂不实现");
            }
            //continue_stmt→continue;
            else if (specific_stmt_node.name == "continue_stmt")
            {
                throw new ExecutorException("暂不实现");
            }
            else
            {
                throw new ExecutorException("Stmt/While_sub_stmt应该只有这些");
            }
        }

        //在stack中找identifier，找到就返回对应的bindings，找不到就null，这样实现很简洁，如果是Stack结构就很复杂（遍历）
        internal static Dictionary<string, Value> findBindings(List<Dictionary<string, Value>> bindings_stack, string identifier, bool is_declare = false)
        {
            //如果是要声明一个变量，那么有两种：全局变量，局部变量，取决于stack的层数
            if (is_declare)
            {
                //只有一层，必然是global
                if(bindings_stack.Count == 1)
                {
                    if (bindings_stack[0].Keys.Contains(identifier))
                    {
                        return bindings_stack[0];
                    }
                    else
                    {
                        return null;
                    }
                }
                //声明一个局部变量，那么global有与没有都不重要了
                else
                {
                    //如果在除了global外的局部区域找到了声明，那么返回
                    for (int i = bindings_stack.Count - 1; i >= 1; i--)
                    {
                        if (bindings_stack[i].Keys.Contains(identifier))
                        {
                            return bindings_stack[i];
                        }
                    }
                    //否则，局部区没有就返回null
                    return null;
                }
            }
            //不是在声明变量
            else
            {
                for (int i = bindings_stack.Count - 1; i >= 0; i--)
                {
                    if (bindings_stack[i].Keys.Contains(identifier))
                    {
                        return bindings_stack[i];
                    }
                }
                return null;
            }
        }
    }
}
