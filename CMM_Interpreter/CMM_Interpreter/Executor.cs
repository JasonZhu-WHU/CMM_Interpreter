using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CMM_Interpreter
{

    class Executor
    {
        //符号表：identifier - value
        public static Dictionary<string, Value> globalSymbolTable = new Dictionary<string, Value>();
        public static List<Dictionary<string, Value>> mainFrameStack = new List<Dictionary<string, Value>>();
        public static List<int> while_indexes = new List<int>();
        
        public Executor()
        {

        }

        public static bool semantic_analyze()
        {
            clearAll();
            constructGlobalSymbolTable();
            executeMain();
            return true;    
        }

        private static void executeMain()
        {
            if (globalSymbolTable.Keys.Contains("main"))
            {
                if(globalSymbolTable["main"].type == "func")
                {
                    int return_val = 0;
                    FuncValue main_func = ((FuncValue)globalSymbolTable["main"]);
                    ExecutorTools.executeFunction(main_func, new List<Value>(), main_func.line_num);
                }
            }
            else
            {
                throw new ExecutorException("不存在main函数，无法执行！");
            }   
        }

        public static void clearAll()
        {
            globalSymbolTable.Clear();
            mainFrameStack.Clear();
            while_indexes.Clear();
        }

        private static void constructGlobalSymbolTable()
        {
            object top_node = Parser.stack_to_parse.Peek();
            StackElement top = (StackElement)top_node;
            mainFrameStack.Add(globalSymbolTable);
            if (top.type_code == 3)
            {
                object initial_node = Parser.stack_to_parse.Pop();
                StackElement root_node = (StackElement)initial_node;
                goThroughAST_toExecute(root_node);
                showGlobalBindings();
            }
            else
            {
                throw new ExecutorException("应该是语法树的根节点E在栈顶");
            }
        }

        public static void showGlobalBindings()
        {
            Console.WriteLine("全局符号表共有" + globalSymbolTable.Count + "个符号如下：");
            int count = 0;
            foreach (var item in globalSymbolTable)
            {
                count++;
                Console.WriteLine(item.Key + item.Value);
            }
            Console.WriteLine(count);
        }

        //自顶向下，深度优先遍历语法树（与生成树顺序一致）
        private static void goThroughAST_toExecute(StackElement e)
        {
            if(e.type_code == 3)
            {
                NonterminalStackElement ee = (NonterminalStackElement)e;
                if(ee.name == "func_definition")
                {
                    addFuncDefinition(ee);
                    return;
                }
                else if(ee.name == "declare_stmt")
                {
                    declareVaraible(ee, mainFrameStack);
                    return;
                }
                else
                {
                    foreach (object b in e.branches)
                    {
                        StackElement child = (StackElement)b;
                        goThroughAST_toExecute(child);
                    }
                }
            }
        }

        //当该节点为func_definition时进入这个函数，把函数添加到全局符号表
        private static void addFuncDefinition(NonterminalStackElement ee)
        {
            NonterminalStackElement func_declaratee_node = (NonterminalStackElement)ee.branches[1];
            IdentifierStackElement func_identifier_node = (IdentifierStackElement)func_declaratee_node.branches[0];
            string func_name = func_identifier_node.content;
            Console.WriteLine("AddFunc: " + func_name);
            FuncValue func = new FuncValue("func", func_name, ee, func_identifier_node.linenum);
            //检查重复声明
            if (globalSymbolTable.Keys.Contains(func_name))
            {
                throw new ExecutorException("已声明的变量或函数名" + func_name, func_identifier_node.linenum);
            }
            else
            {
                globalSymbolTable.Add(func_name, func);
            }
        }

        //当该节点为declare_stmt时进入这个函数，把变量添加到符号表bindings
        //可能一个declare语句会声明很多个变量，这些变量可能是数组也可能不是，这些变量可能被赋值也可能没有
        // declare_stmt→type declaratee_lst;
        public static void declareVaraible(NonterminalStackElement declarate_stmt, List<Dictionary<string, Value>> bindings_stack)
        {
            if (declarate_stmt.branches.Count == 3)
            {
                NonterminalStackElement type_nonterminal = (NonterminalStackElement)declarate_stmt.branches[0];
                OtherTerminalStackElement type_node = (OtherTerminalStackElement)type_nonterminal.branches[0];
                //获得变量类型
                string type = type_node.content;
                //声明的所有变量名
                List<NonterminalStackElement> declaratees = new List<NonterminalStackElement>();
                //有两种可能：declaratee_lst→declaratee,declaratee_lst|declaratee
                NonterminalStackElement declaratee_lst_node = (NonterminalStackElement)declarate_stmt.branches[1];
                //处理递归，把每个declaratee加到declaratees
                while (declaratee_lst_node.branches.Count != 1)
                {
                    NonterminalStackElement first_declaratee_node = (NonterminalStackElement)declaratee_lst_node.branches[0];
                    declaratees.Add(first_declaratee_node);
                    declaratee_lst_node = (NonterminalStackElement)declaratee_lst_node.branches[2];
                }
                NonterminalStackElement declaratee_node = (NonterminalStackElement)declaratee_lst_node.branches[0];
                declaratees.Add(declaratee_node);

                //现在已经获取到的东西有：变量类型（暂时未区分数组），声明的变量个数，所有的declaratees
                //还缺少：初始化所有变量
                initialize_declaratees(type, declaratees, bindings_stack);
            }
            else
            {
                throw new ExecutorException("语法树的declare_stmt应该有且只有三个子结点");
            }
        }

        /*
         * 初始化所有变量：规则（未给出初始化值initializer的，也就是只是声明的变量都赋0）
         * declaratee→identifier | identifier[simple_expr]  |  identifier initializer | identifier [simple_expr]  initializer |  identifier[] initializer
         * 接收参数declaratees：初始化数组/单个变量均可
         * 可能出现错误：重复声明，void类型变量错误
         * 声明单个变量：用数组给单个变量赋值，initializer值的类型与声明类型不符合
         * 声明变量数组：用单个变量给数组变量赋值，initializer值的类型与声明类型不符合，initializer值的长度大于声明长度
         * 可能有多个变量一起声明甚至初始化，但是他们类型必须相同（符合C语法）
        */
        private static void initialize_declaratees(string type, List<NonterminalStackElement> declaratees, List<Dictionary<string, Value>> bindings_stack)
        {
            foreach(NonterminalStackElement declaratee in declaratees)
            {
                IdentifierStackElement id_node = (IdentifierStackElement)declaratee.branches[0];
                string new_identifier = id_node.content;
                int linenum = id_node.linenum;
                //检查重复声明
                Dictionary<string, Value> bindings = ExecutorTools.findBindings(bindings_stack, new_identifier, true);
                //也就是已经有了同名变量
                if(bindings != null)
                {
                    throw new ExecutorException("重复声明已存在的变量" + new_identifier, id_node.linenum);
                }
                else
                {
                    //那就在最上层准备声明
                    bindings = bindings_stack[bindings_stack.Count - 1];
                    //declaratee→identifier声明变量
                    if (declaratee.branches.Count == 1)
                    {
                        if(type == "int")
                        {
                            bindings.Add(new_identifier, new IntValue("int", false, "0", id_node.linenum));
                        }
                        else if(type == "real")
                        {
                            bindings.Add(new_identifier, new RealValue("real", false, "0.0", id_node.linenum));
                        }
                        else if (type == "char")
                        {
                            bindings.Add(new_identifier, new CharValue("char", false, "", id_node.linenum));
                        }
                        else if (type == "string")
                        {
                            bindings.Add(new_identifier, new StringValue("string", false, "", id_node.linenum));
                        }
                        else
                        {
                            throw new ExecutorException("试图声明一个void类型的变量" + new_identifier, id_node.linenum);
                        }
                    }
                    //declaratee→identifier initializer 声明并初始化变量
                    else if (declaratee.branches.Count == 2)
                    {
                        NonterminalStackElement initializer_node = (NonterminalStackElement)declaratee.branches[1];
                        if (initializer_node.branches.Count != 2)
                        {
                            throw new ExecutorException("试图用数组给单个变量" + new_identifier + "进行初始化", linenum);
                        }
                        //排除后，必然是initializer→=expr
                        NonterminalStackElement expr_node = (NonterminalStackElement)initializer_node.branches[1];
                        Value v = Evaluator.eval_expr(expr_node, mainFrameStack);
                        v = ExecutorTools.adjustType(type, v);
                        bindings.Add(new_identifier, v);
                    }
                    //declaratee→identifier[simple_expr] | identifier[] initializer声明定长数组，初始化auto自适应长度数组
                    else if (declaratee.branches.Count == 4)
                    {
                        StackElement third_child_of_last_declaratee = (StackElement)declaratee.branches[2];
                        
                        //declaratee→identifier[] initializer
                        if (third_child_of_last_declaratee.type_code == 4)
                        {
                            //现在还不知道数组长度，要根据initializer算
                            int array_len;
                            NonterminalStackElement initializer_node = (NonterminalStackElement)declaratee.branches[3];
                            //initializer→ = expr |={ initializer_lst}
                            if (initializer_node.branches.Count != 4)
                            {
                                throw new ExecutorException("试图用单个变量给数组" + new_identifier + "进行初始化", linenum);
                            }
                            //排除后，必然是initializer→={initializer_lst}
                            NonterminalStackElement initializer_lst_node = (NonterminalStackElement)initializer_node.branches[2];
                            //获取initializer数组的长度 intializer_lst→expr | expr,initializer_lst | expr,
                            //获取给出的initializer中的值的集合
                            List<Value> given_values = new List<Value>();
                            while (initializer_lst_node.branches.Count == 3)
                            {
                                Value vv = Evaluator.eval_expr((NonterminalStackElement)initializer_lst_node.branches[0], mainFrameStack);
                                vv = ExecutorTools.adjustType(type, vv);
                                given_values.Add(vv);
                                initializer_lst_node = (NonterminalStackElement)initializer_lst_node.branches[2];
                            }
                            Value v = Evaluator.eval_expr((NonterminalStackElement)initializer_lst_node.branches[0], mainFrameStack);
                            v = ExecutorTools.adjustType(type, v);
                            given_values.Add(v);
                            initializer_lst_node = (NonterminalStackElement)initializer_lst_node.branches[2];
                            //根据给出的值数量，确定我们数组的长度
                            array_len = given_values.Count;
                            if (type == "int")
                            {
                                int[] arrayElements = new int[array_len];
                                for (int i = 0; i < array_len; i++)
                                {
                                    if (given_values[i].type != "int")
                                    {
                                        throw new ExecutorException("用" + given_values[i].type + "类型给" + type + "类型数组变量" + new_identifier + "初始化", linenum);
                                    }
                                    IntValue value = (IntValue)given_values[i];
                                    arrayElements[i] = value.value;
                                }
                                bindings.Add(new_identifier, new IntArrayValue("intArray", false, array_len, arrayElements, linenum));
                            }
                            else if (type == "real")
                            {
                                double[] arrayElements = new double[array_len];
                                for (int i = 0; i < array_len; i++)
                                {
                                    if (given_values[i].type != "real")
                                    {
                                        throw new ExecutorException("用" + given_values[i].type + "类型给" + type + "类型数组变量" + new_identifier + "初始化", linenum);
                                    }
                                    RealValue value = (RealValue)given_values[i];
                                    arrayElements[i] = value.value;
                                }
                                bindings.Add(new_identifier, new RealArrayValue("realArray", false, array_len, arrayElements, linenum));
                            }
                            else if (type == "char")
                            {
                                string[] arrayElements = new string[array_len];
                                for (int i = 0; i < array_len; i++)
                                {
                                    if (given_values[i].type != "char")
                                    {
                                        throw new ExecutorException("用" + given_values[i].type + "类型给" + type + "类型数组变量" + new_identifier + "初始化", linenum);
                                    }
                                    CharValue value = (CharValue)given_values[i];
                                    arrayElements[i] = value.value;
                                }
                                bindings.Add(new_identifier, new CharArrayValue("charArray", false, array_len, arrayElements, linenum));
                            }
                            else if (type == "string")
                            {
                                string[] arrayElements = new string[array_len];
                                for (int i = 0; i < array_len; i++)
                                {
                                    if (given_values[i].type != "string")
                                    {
                                        throw new ExecutorException("用" + given_values[i].type + "类型给" + type + "类型数组变量" + new_identifier + "初始化", linenum);
                                    }
                                    StringValue value = (StringValue)given_values[i];
                                    arrayElements[i] = value.value;
                                }
                                bindings.Add(new_identifier, new StringArrayValue("stringArray", false, array_len, arrayElements, linenum));
                            }
                            else
                            {
                                throw new ExecutorException("试图声明一个void类型的变量数组" + new_identifier, linenum);
                            }
                        }
                        //declaratee→identifier[simple_expr]
                        else
                        {
                            NonterminalStackElement simple_expr_node = (NonterminalStackElement)declaratee.branches[2];
                            int array_len = ExecutorTools.get_array_len_from_simple_expr(simple_expr_node, mainFrameStack);
                            if (type == "int")
                            {
                                int[] arrayElements = new int[array_len];
                                for (int i = 0; i < array_len; i++)
                                    arrayElements[i] = 0;
                                bindings.Add(new_identifier, new IntArrayValue("intArray", false, array_len, arrayElements, linenum));
                            }
                            else if (type == "real")
                            {
                                double[] arrayElements = new double[array_len];
                                for (int i = 0; i < array_len; i++)
                                    arrayElements[i] = 0.0;
                                bindings.Add(new_identifier, new RealArrayValue("realArray", false, array_len, arrayElements, linenum));
                            }
                            else if (type == "char")
                            {
                                string[] arrayElements = new string[array_len];
                                for (int i = 0; i < array_len; i++)
                                    arrayElements[i] = "\0";
                                bindings.Add(new_identifier, new CharArrayValue("charArray", false, array_len, arrayElements, linenum));
                            }
                            else if (type == "string")
                            {
                                string[] arrayElements = new string[array_len];
                                for (int i = 0; i < array_len; i++)
                                    arrayElements[i] = "";
                                bindings.Add(new_identifier, new StringArrayValue("stringArray", false, array_len, arrayElements, linenum));
                            }
                            else
                            {
                                throw new ExecutorException("试图声明一个void类型的变量数组" + new_identifier, linenum);
                            }
                        }
                    }
                    //declaratee→identifier [simple_expr]  initializer
                    else if (declaratee.branches.Count == 5)
                    {
                        NonterminalStackElement simple_expr_node = (NonterminalStackElement)declaratee.branches[2];
                        //数组声明的长度
                        int array_len = ExecutorTools.get_array_len_from_simple_expr(simple_expr_node, mainFrameStack);
                        //现在还不知道被赋值的数组长度，要根据initializer算
                        NonterminalStackElement initializer_node = (NonterminalStackElement)declaratee.branches[4];
                        //initializer→ = expr | ={ initializer_lst}
                        if (initializer_node.branches.Count != 4)
                        {
                            throw new ExecutorException("试图用单个变量给数组" + new_identifier + "进行初始化", linenum);
                        }
                        //排除后，必然是initializer→={initializer_lst}
                        NonterminalStackElement initializer_lst_node = (NonterminalStackElement)initializer_node.branches[2];
                        //获取initializer数组的长度 intializer_lst→expr | expr,initializer_lst | expr,
                        //获取给出的initializer中的值的集合
                        List<Value> given_values = new List<Value>();
                        while (initializer_lst_node.branches.Count == 3)
                        {
                            Value vv = Evaluator.eval_expr((NonterminalStackElement)initializer_lst_node.branches[0], mainFrameStack);
                            vv = ExecutorTools.adjustType(type, vv);
                            given_values.Add(vv);
                            initializer_lst_node = (NonterminalStackElement)initializer_lst_node.branches[2];
                        }
                        Value v = Evaluator.eval_expr((NonterminalStackElement)initializer_lst_node.branches[0], mainFrameStack);
                        v = ExecutorTools.adjustType(type, v);
                        given_values.Add(v);
                        //赋值
                        if(given_values.Count > array_len)
                        {
                            throw new ExecutorException("数组" + new_identifier + "的初始值设定项长度" + given_values.Count + "大于声明的长度" + array_len, linenum);
                        }
                        if (type == "int")
                        {
                            int[] arrayElements = new int[array_len];
                            for (int i = 0; i < array_len; i++)
                            {
                                if (i < given_values.Count)
                                {
                                    IntValue intv = (IntValue)given_values[i];
                                    arrayElements[i] = intv.value;
                                }
                                else
                                {
                                    arrayElements[i] = 0;
                                }
                            }
                            bindings.Add(new_identifier, new IntArrayValue("intArray", false, array_len, arrayElements, linenum));
                        }
                        else if (type == "real")
                        {
                            double[] arrayElements = new double[array_len];
                            for (int i = 0; i < array_len; i++)
                            {
                                if (i < given_values.Count)
                                {
                                    RealValue intv = (RealValue)given_values[i];
                                    arrayElements[i] = intv.value;
                                }
                                else
                                {
                                    arrayElements[i] = 0.0;
                                }
                            }
                            bindings.Add(new_identifier, new RealArrayValue("realArray", false, array_len, arrayElements, linenum));
                        }
                        else if (type == "char")
                        {
                            string[] arrayElements = new string[array_len];
                            for (int i = 0; i < array_len; i++)
                            {
                                if (i < given_values.Count)
                                {
                                    CharValue intv = (CharValue)given_values[i];
                                    arrayElements[i] = intv.value;
                                }
                                else
                                {
                                    arrayElements[i] = "\0";
                                }
                            }
                            bindings.Add(new_identifier, new CharArrayValue("charArray", false, array_len, arrayElements, linenum));
                        }
                        else if (type == "string")
                        {
                            string[] arrayElements = new string[array_len];
                            for (int i = 0; i < array_len; i++)
                            {
                                if (i < given_values.Count)
                                {
                                    StringValue intv = (StringValue)given_values[i];
                                    arrayElements[i] = intv.value;
                                }
                                else
                                {
                                    arrayElements[i] = "";
                                }
                            }
                            bindings.Add(new_identifier, new StringArrayValue("stringArray", false, array_len, arrayElements, linenum));
                        }
                        else
                        {
                            throw new ExecutorException("试图声明一个void类型的变量数组" + new_identifier, linenum);
                        }
                     }
                    else
                    {
                        throw new ExecutorException("initialize_declaratee_withzero出现不正常的错误，可能是语法树解析declaratee出现问题");
                    }
                }
            }
        }
    }
}
 
 