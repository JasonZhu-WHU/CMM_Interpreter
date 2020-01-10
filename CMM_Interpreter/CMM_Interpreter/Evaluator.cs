using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class Evaluator
    {

        /* Function: 返回type类型的Value
         * PS: 若不能返回type类型的Value，那么就报错
         * 引入type的原因：real i = 1 + 2; 在evaluate这个expr时，我们不知道到底是int还是real
        */
        public static Value eval_expr(NonterminalStackElement expr_node, List<Dictionary<string, Value>> bindings_stack)
        {
            if (expr_node.branches.Count != 1)
            {
                throw new ExecutorException("文法发生了变化", expr_node.linenum);
            }
            else
            {
                NonterminalStackElement logical_expr_node = (NonterminalStackElement)expr_node.branches[0];
                return eval_logical_expr(logical_expr_node, bindings_stack);
            }
        }

        //logical_expr→relational_expr logical_expr_more
        //  logical_expr_more→ ε | logical_op relational_expr logical_expr_more
        public static Value eval_logical_expr(NonterminalStackElement logical_expr_node, List<Dictionary<string, Value>> bindings_stack)
        {
            if (logical_expr_node.branches.Count != 2)
            {
                throw new ExecutorException("文法发生了变化", logical_expr_node.linenum);
            }
            else
            {
                NonterminalStackElement relational_expr_node = (NonterminalStackElement)logical_expr_node.branches[0];
                NonterminalStackElement logical_expr_more_node = (NonterminalStackElement)logical_expr_node.branches[1];
                Value first_relational_expr_value = eval_relational_expr(relational_expr_node, bindings_stack);
                //逐个迭代，很自然的左结合
                while (logical_expr_more_node.branches.Count == 3)
                {
                    NonterminalStackElement logical_op_node = (NonterminalStackElement)logical_expr_more_node.branches[0];
                    NonterminalStackElement second_relational_expr_node = (NonterminalStackElement)logical_expr_more_node.branches[1];
                    OtherTerminalStackElement logical_op = (OtherTerminalStackElement)logical_op_node.branches[0];
                    if (logical_op.content == "&&")
                    {
                        //short circuit evaluation短路求值
                        if (!first_relational_expr_value.getBoolean())
                        {
                            return new BoolValue("bool", true, false, logical_op.linenum);
                        }
                        //短路求值！
                        Value second_relational_expr_value = eval_relational_expr(second_relational_expr_node, bindings_stack);
                        first_relational_expr_value = new BoolValue("bool", true, second_relational_expr_value.getBoolean(), logical_op.linenum);
                    }
                    else
                    {
                        //short circuit evaluation短路求值
                        if (first_relational_expr_value.getBoolean())
                        {
                            return new BoolValue("bool", true, true, logical_op.linenum);
                        }
                        //短路求值！
                        Value second_relational_expr_value = eval_relational_expr(second_relational_expr_node, bindings_stack);
                        first_relational_expr_value = new BoolValue("bool", true, second_relational_expr_value.getBoolean(), logical_op.linenum);
                    }
                    logical_expr_more_node = (NonterminalStackElement)logical_expr_more_node.branches[2];
                }
                return first_relational_expr_value;
            }
        }

        //relational_expr→simple_expr relational_expr_more 返回一个value，这个value是truthy还是falsey交给上层函数
        //  relational_expr_more →ε | comparasion_op simple_expr relational_expr_more
        //这里只要进行了compare，那返回int value而不是其他的，其原因是：3>2>1 -> false
        public static Value eval_relational_expr(NonterminalStackElement relational_expr_node, List<Dictionary<string, Value>> bindings_stack)
        {
            NonterminalStackElement simple_expr_node = (NonterminalStackElement)relational_expr_node.branches[0];
            NonterminalStackElement relational_expr_more_node = (NonterminalStackElement)relational_expr_node.branches[1];
            //这里面临一个问题，如果comparison是==或者空，那么simple_expr什么都可以，如果comparison是> <=等等，那就只能是number
            Value first_simple_expr_value = eval_simple_expr(simple_expr_node, bindings_stack);
            while (relational_expr_more_node.branches.Count == 3)
            {
                NonterminalStackElement comparison_op_node = (NonterminalStackElement)relational_expr_more_node.branches[0];
                NonterminalStackElement second_simple_expr = (NonterminalStackElement)relational_expr_more_node.branches[1];
                relational_expr_more_node = (NonterminalStackElement)relational_expr_more_node.branches[2];
                OtherTerminalStackElement comparison_op = (OtherTerminalStackElement)comparison_op_node.branches[0];
                if (comparison_op.content == "==")
                {
                    first_simple_expr_value = first_simple_expr_value.compareWith(eval_simple_expr(second_simple_expr, bindings_stack));
                }
                //这里相对上面是反过来的（不等于）
                else if (comparison_op.content == "!=" || comparison_op.content == "<>")
                {
                    IntValue result = first_simple_expr_value.compareWith(eval_simple_expr(second_simple_expr, bindings_stack));
                    if (result.value == 0)
                    {
                        result.value = 1;
                    }
                    else
                    {
                        result.value = 0;
                    }
                    first_simple_expr_value = result;
                }
                else
                {
                    first_simple_expr_value = first_simple_expr_value.compareWith(eval_simple_expr(second_simple_expr, bindings_stack), comparison_op.content);
                }
            }
            return first_simple_expr_value;
        }



        /* Function: 返回type类型的Value
         * 若不能返回type类型的Value，那么就报错
         * 注意：要区分负数，把负数放到value里面返回
         * 引入type的原因：real i = 1 + 2; 在evaluate这个expr时，我们不知道到底是int还是real
         * 相关的产生式：
         * simple_expr→term more_term
         *   term→factor more_factor
         *   more_term→ε|add_op term more_term
         *     add_op→+|-
        */
        public static Value eval_simple_expr(NonterminalStackElement simple_expr_node, List<Dictionary<string, Value>> bindings_stack)
        {
            NonterminalStackElement term_node = (NonterminalStackElement)simple_expr_node.branches[0];
            NonterminalStackElement more_term_node = (NonterminalStackElement)simple_expr_node.branches[1];
            Value first_term_value = eval_term(term_node, bindings_stack);
            while (more_term_node.branches.Count == 3)
            {
                NonterminalStackElement add_op_node = (NonterminalStackElement)more_term_node.branches[0];
                OtherTerminalStackElement add_op = (OtherTerminalStackElement)add_op_node.branches[0];
                NonterminalStackElement second_term_node = (NonterminalStackElement)more_term_node.branches[1];
                Value second_term_value = eval_term(second_term_node, bindings_stack);
                if (Value.computable_types.Contains(first_term_value.type) && Value.computable_types.Contains(second_term_value.type))
                {
                    first_term_value = first_term_value.mathCalculate(second_term_value, add_op.content);
                }
                else
                {
                    throw new ExecutorException("无法直接进行加减运算的类型", add_op.linenum);
                }
                more_term_node = (NonterminalStackElement)more_term_node.branches[2];
            }
            return first_term_value;
        }

        /* term → factor more_factor
         *   factor→+factor |-factor|number|identifier more_identifier|(expr)|"string"|'char'
         *   more_factor→ε|mul_op factor more_factor
         *     mul_op → *|/
         * 如法炮制
        */
        public static Value eval_term(NonterminalStackElement term_node, List<Dictionary<string, Value>> bindings_stack)
        {
            NonterminalStackElement factor_node = (NonterminalStackElement)term_node.branches[0];
            NonterminalStackElement more_factor_node;
            more_factor_node = (NonterminalStackElement)term_node.branches[1];
            Value first_factor_value = eval_factor(factor_node, bindings_stack);
            //more_factor → ε 
            while (more_factor_node.branches.Count == 3)
            {
                NonterminalStackElement mul_op_node = (NonterminalStackElement)more_factor_node.branches[0];
                OtherTerminalStackElement mul_op = (OtherTerminalStackElement)mul_op_node.branches[0];
                NonterminalStackElement second_factor_node = (NonterminalStackElement)more_factor_node.branches[1];
                Value second_factor_value = eval_factor(second_factor_node, bindings_stack);
                if (Value.computable_types.Contains(first_factor_value.type) && Value.computable_types.Contains(second_factor_value.type))
                {
                    first_factor_value = first_factor_value.mathCalculate(second_factor_value, mul_op.content);
                }
                else
                {
                    throw new ExecutorException("无法直接进行乘除运算的类型", mul_op.linenum);
                }
                more_factor_node = (NonterminalStackElement)more_factor_node.branches[2];
            }
            return first_factor_value;
        }

        //这里情况就不同了，上面是对应着+-*/四则运算的，所以char/string/数组 都不应该直接进行这些运算，但是他们可以被访问赋值，因此这里不做过多限制
        /*
         * factor→+factor |-factor|number|identifier more_identifier|(expr)|"string"|'char'
         *   more_identifier→ ε | [simple_expr]  |  ()  |  (param_values)
         *   number→real_number |integer
        */
        public static Value eval_factor(NonterminalStackElement factor_node, List<Dictionary<string, Value>> bindings_stack)
        {
            StackElement first_child = (StackElement)factor_node.branches[0];
            //factor → number | "string" | 'char'
            if (factor_node.branches.Count == 1)
            {
                //char
                if (first_child.type_code == 7)
                {
                    CharStackElement char_node = (CharStackElement)first_child;
                    return new CharValue("char", true, char_node.content, char_node.linenum);
                }
                //string
                else if (first_child.type_code == 8)
                {
                    StringStackElement string_node = (StringStackElement)first_child;
                    return new StringValue("string", true, string_node.content, string_node.linenum);
                }
                //number
                else if (first_child.type_code == 3)
                {
                    NonterminalStackElement number_node = (NonterminalStackElement)first_child;
                    return eval_number(number_node);
                }
                else
                {
                    throw new ExecutorException("eval_factor运行中发现factor文法发生了意想不到的变化");
                }
            }
            //factor→+factor |-factor | identifier more_identifier
            else if (factor_node.branches.Count == 2)
            {
                //+ - factor
                if (first_child.type_code == 4)
                {
                    int linenum = ((OtherTerminalStackElement)first_child).linenum;
                    OtherTerminalStackElement positive_or_negative = (OtherTerminalStackElement)first_child;
                    NonterminalStackElement child_factor_node = (NonterminalStackElement)factor_node.branches[1];
                    if (positive_or_negative.content == "+")
                    {
                        return eval_factor(child_factor_node, bindings_stack);
                    }
                    //负号就要把值的符号颠倒
                    else
                    {
                        Value child_factor_node_value = eval_factor(child_factor_node, bindings_stack);
                        if (!Value.computable_types.Contains(child_factor_node_value.type))
                        {
                            throw new ExecutorException("不可在非数值类型前加上正号或者负号", linenum);
                        }
                        if (child_factor_node_value.type == "real")
                        {
                            RealValue value = (RealValue)child_factor_node_value;
                            value.value = -value.value;
                            return value;
                        }
                        else if (child_factor_node_value.type == "int")
                        {
                            IntValue value = (IntValue)child_factor_node_value;
                            value.value = -value.value;
                            return value;
                        }
                        else if (child_factor_node_value.type == "number")
                        {
                            NumberValue value = (NumberValue)child_factor_node_value;
                            value.value = -value.value;
                            return value;
                        }
                        else
                        {
                            throw new ExecutorException("eval_factor运行中发现factor文法发生了意想不到的变化");
                        }
                    }
                }
                //factor→ identifier more_identifier
                //  more_identifier→ε | [simple_expr]  |  ()  |  (param_values)
                else
                {
                    IdentifierStackElement identifier_node = (IdentifierStackElement)factor_node.branches[0];
                    int linenum = identifier_node.linenum;
                    NonterminalStackElement more_identifier_node = (NonterminalStackElement)factor_node.branches[1];
                    //检查是否定义
                    Dictionary<string, Value> bindings = ExecutorTools.findBindings(bindings_stack, identifier_node.content); 
                    if (bindings != null)
                    {
                        //只是访问一个identifier 即 factor→ identifier 
                        // more_identifier→ε
                        if (more_identifier_node.branches.Count == 0)
                        {
                            Value identifier_value = bindings[identifier_node.content];
                            return identifier_value;
                        }
                        //应该是调用了一个不接受任何参数的函数比如foo()这样， more_identifier→ () 
                        else if (more_identifier_node.branches.Count == 2)
                        {
                            Value identifier_value = bindings[identifier_node.content];
                            if (identifier_value.type != "func")
                            {
                                throw new ExecutorException("标识符后带()应该是调用函数，但该标识符" + identifier_node.content + "对应函数并不存在", linenum);
                            }
                            FuncValue relevant_fucntion = (FuncValue)identifier_value;
                            Value return_value = relevant_fucntion.executeFunction();
                            return return_value;
                        }
                        // 应该是调用了一个带参数的函数，或者是访问数组的某个元素 more_identifier → [simple_expr]  |  (param_values)
                        else if (more_identifier_node.branches.Count == 3)
                        {
                            Value identifier_value = bindings[identifier_node.content];
                            NonterminalStackElement second_element = (NonterminalStackElement)more_identifier_node.branches[1];
                            // more_identifier → [simple_expr]  要检查：第一，数组访问越界了没有；第二，访问的数组元素类型是否与要求的类型type相符合
                            if (second_element.name == "simple_expr")
                            {
                                //类型错误
                                if (!identifier_value.type.Contains("Array"))
                                {
                                    throw new ExecutorException("标识符后带[ ]应该是访问数组某个元素，但该标识符" + identifier_node.content + "对应的并不是数组", linenum);
                                }
                                string array_type = identifier_value.type.Substring(0, identifier_value.type.Length - 5);
                                NonterminalStackElement simple_expr_node = second_element;
                                int index_to_access = ExecutorTools.get_array_len_from_simple_expr(simple_expr_node, bindings_stack);
                                //越界错误
                                if (index_to_access >= identifier_value.getArrayLen())
                                {
                                    throw new ExecutorException("访问数组" + identifier_node.content + "时出现越界访问错误，数组长度只有" + identifier_value.getArrayLen(), linenum);
                                }
                                //eval要求就是获取到值，所以这里直接获取数组中元素的值，再转换为Value类型
                                else
                                {
                                    if (identifier_value.type.Contains("int"))
                                    {
                                        return new IntValue("int", false, identifier_value.getIntArrayElement(index_to_access).ToString(), linenum);
                                    }
                                    else if (identifier_value.type.Contains("real"))
                                    {
                                        return new RealValue("real", false, identifier_value.getRealArrayElement(index_to_access).ToString(), linenum);
                                    }
                                    else if (identifier_value.type.Contains("int"))
                                    {
                                        return new CharValue("char", false, identifier_value.getCharArrayElement(index_to_access).ToString(), linenum);
                                    }
                                    else
                                    {
                                        return new StringValue("string", false, identifier_value.getStringArrayElement(index_to_access).ToString(), linenum);
                                    }
                                }
                            }
                            // more_identifier → (param_values)  要检查：第一，函数签名是否符合（类型，次序）；第二，函数返回类型是否与要求的类型type相符合
                            else if (second_element.name == "param_values")
                            {
                                //类型错误
                                if (!identifier_value.type.Contains("func"))
                                {
                                    throw new ExecutorException(identifier_node.content + "对应的并不是函数，无法调用", linenum);
                                }
                                FuncValue func_value = (FuncValue)identifier_value;
                                List<Value> args = new List<Value>();
                                NonterminalStackElement expr_node;
                                //param_values →expr,param_values|expr
                                NonterminalStackElement param_values_node = second_element;
                                while (param_values_node.branches.Count == 3)
                                {
                                    expr_node = (NonterminalStackElement)param_values_node.branches[0];
                                    param_values_node = (NonterminalStackElement)param_values_node.branches[2];
                                    args.Add(eval_expr(expr_node, bindings_stack));
                                }
                                expr_node = (NonterminalStackElement)param_values_node.branches[0];
                                args.Add(eval_expr(expr_node, bindings_stack));
                                return ExecutorTools.executeFunction(func_value, args, linenum);
                            }
                            else
                            {
                                throw new ExecutorException("文法发生了变化", linenum);
                            }
                        }
                        else
                        {
                            //Value relevant_value = Frame.curr_frame.local_bindings[identifier_node.content];
                            throw new ExecutorException("怎么到这的？", linenum);
                        }
                    }
                    throw new ExecutorException("语句中访问未定义的标识符" + identifier_node.content, linenum);
                }
            }
            //factor→ ( expr )
            else if (factor_node.branches.Count == 3)
            {
                NonterminalStackElement expr_node = (NonterminalStackElement)factor_node.branches[1];
                return eval_expr(expr_node, bindings_stack);
            }
            else
            {
                throw new ExecutorException("文法发生了变化", factor_node.linenum);
            }
        }

        //number→real_number |integer 产生实数或整数
        public static Value eval_number(NonterminalStackElement number_node)
        {
            if (number_node.branches.Count != 1)
            {
                throw new ExecutorException("文法发生了变化", number_node.linenum);
            }
            else
            {
                StackElement number = (StackElement)number_node.branches[0];
                if (number.type_code == 2)
                {
                    IntStackElement int_number = (IntStackElement)number;
                    return new IntValue("int", true, int_number.content, int_number.linenum);
                }
                else if (number.type_code == 5)
                {
                    RealStackElement real_number = (RealStackElement)number;
                    return new RealValue("real", true, real_number.content, real_number.linenum);
                }
                else
                {
                    throw new ExecutorException("文法发生了变化", number_node.linenum);
                }
            }
        }

    }
}
