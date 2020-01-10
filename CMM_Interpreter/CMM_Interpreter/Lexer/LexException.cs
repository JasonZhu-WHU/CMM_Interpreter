using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM_Interpreter
{
    public class LexException : Exception
    {
        public string err_msg;
        public int line_num;

        public LexException()
        {

        }

        public LexException(string err_msg, int line_num)
        {
            this.line_num = line_num;
            this.err_msg = err_msg; 
        }

        public string getExceptionMsg()
        {
            return "第" + line_num + "行: " + err_msg;
        }
    }
}
