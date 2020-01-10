using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace CMM_Interpreter
{
    class ParserException : Exception
    {
        public string err_msg;
        public int line_num = 0;

        public ParserException(string err_msg)
        {
            this.err_msg = err_msg;
        }

        public ParserException(string err_msg, int line_num)
        {
            this.line_num = line_num;
            this.err_msg = err_msg;
        }

        public string getExceptionMsg()
        {
            if(line_num == 0)
            {
                return err_msg;
            }
            return "第" + line_num + "行: " + err_msg;
        }
    }
}
