using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    class ExecutorException : Exception
    {
        public string err_msg;
        public int line_num = 0;

        public ExecutorException(string err_msg)
        {
            this.err_msg = err_msg;
        }

        public ExecutorException(string err_msg, int line_num)
        {
            this.line_num = line_num;
            this.err_msg = err_msg;
        }

        public string getExceptionMsg()
        {
            if (line_num == 0)
            {
                return err_msg;
            }
            return "第" + line_num + "行: " + err_msg;
        }
    }
}
