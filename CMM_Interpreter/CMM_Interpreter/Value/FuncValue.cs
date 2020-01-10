using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class FuncValue : Value
    {
        public NonterminalStackElement func_def_node;
        public List<string> params_types = new List<string>();
        public string funcName;

        public FuncValue(string type, string funcName, NonterminalStackElement func_def_node, int line_num)
        {
            this.type = type;
            this.funcName = funcName;
            this.initialized = true;
            this.func_def_node = func_def_node;
            this.line_num = line_num;
        }

        //返回程序执行的结果，可以是NullValue
        public Value executeFunction()
        {
            return ExecutorTools.executeFunction(this, new List<Value>(), line_num);
        }

        ////返回程序执行的结果，传入params_value节点
        //public Value executeFunctionWithParams(NonterminalStackElement param_values_node, D)
        //{
        //    List<Value> args = new List<Value>();
        //    NonterminalStackElement expr_node;
        //    while(param_values_node.branches.Count == 3)
        //    {
        //        expr_node = (NonterminalStackElement)param_values_node.branches[0];
        //        param_values_node = (NonterminalStackElement)param_values_node.branches[2];
        //        args.Add(Evaluator.eval_expr(expr_node));
        //    }
        //    return ExecutorTools.executeFunction(this, List < Value > arguments_lst)
        //}

        public override string ToString()
        {
            string text = "";
            text += " Value{ type: func, name: " + funcName + ". line_num: " + line_num + "}";
            return text;
        }

    }
}
