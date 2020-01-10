using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMM_Interpreter
{
    public class Token
    {
        public int lineNum;
        public int code;
        public string content;
        public static bool comment_before = false;
        public static int comment_index = 0;

        public Token()
        {
            lineNum = -1;
            code = -1;
            content = "未赋值";
        }

        public Token(int lineNum, int code, string content)
        {
            this.lineNum = lineNum;
            this.code = code;
            this.content = content;
        }

        public override string ToString()
        {
            return "Token{ code= " + code + ", content = " + content + ", lineNum = " + lineNum + "}";
        }

        //传入第n行，得到第n行的tokens
        public static string getALineOfTokens(int i)
        {
            string text = "";
            foreach(Token t in MainWindow.allTokens)
            {
                if(t.lineNum == i)
                {
                    text += t.content;
                    text += " ";
                }
            }
            return text;
        }

        //从第s个字符开始读part_of_string.Substring(read_Point)，判断产生什么token，若均不符合则抛出异常（报错），若符合某情况，则信息填入t，并返回token的长度用以调整readpoint
        public static string getNextToken(string s, int line_num, Token t)
        {
            string part_of_string = s.Trim();
            if (string.IsNullOrWhiteSpace(part_of_string))
            {
                throw new LexException("无法产生token，因为传递到getNextToken函数中的string无有效信息", line_num);
            }
            t.lineNum = line_num;
            //G
            if (part_of_string[0] == ';' || part_of_string[0] == ',' || part_of_string[0] == '(' || part_of_string[0] == ')' || part_of_string[0] == '{' || part_of_string[0] == '}' || part_of_string[0] == '[' || part_of_string[0] == ']')
            {
                setTokenCodeG(part_of_string[0], t);
                t.content = part_of_string[0].ToString();
                return part_of_string.Substring(1);
            }
            //A B
            else if (part_of_string[0] == '+' || part_of_string[0] == '-' || part_of_string[0] == '*' || part_of_string[0] == '/' || part_of_string[0] == '=' || part_of_string[0] == '>' || part_of_string[0] == '!')
            {
                if (part_of_string.Length == 1)
                {
                    //A
                    setTokenCodeA(part_of_string[0], t);
                    t.content = part_of_string[0].ToString();
                    return part_of_string.Substring(1);
                }
                else
                {
                    //B
                    if (part_of_string[1] == '=')
                    {
                        setTokenCodeB(part_of_string.Substring(0, 2), t);
                        t.content = part_of_string.Substring(0, 2);
                        return part_of_string.Substring(2);
                    }
                    //A
                    else
                    {
                        setTokenCodeA(part_of_string[0], t);
                        t.content = part_of_string[0].ToString();
                        return part_of_string.Substring(1);
                    }
                }
            }
            //C D
            else if (part_of_string[0] == '<')
            {
                if (part_of_string.Length == 1)
                {
                    //C
                    t.code = 21;
                    t.content = part_of_string[0].ToString();
                    return part_of_string.Substring(1);
                }
                else
                {
                    //D
                    if (part_of_string[1] == '=' || part_of_string[1] == '>')
                    {
                        setTokenCodeD(part_of_string[1], t);
                        t.content = part_of_string.Substring(0, 2);
                        return part_of_string.Substring(2);
                    }
                    //C
                    else
                    {
                        t.code = 21;
                        t.content = part_of_string[0].ToString();
                        return part_of_string.Substring(1);
                    }
                }
            }
            //关键字处理
            else if (part_of_string.StartsWith("if") || part_of_string.StartsWith("return") || part_of_string.StartsWith("else") || part_of_string.StartsWith("int") || part_of_string.StartsWith("while") || part_of_string.StartsWith("read") || part_of_string.StartsWith("write") || part_of_string.StartsWith("real") || part_of_string.StartsWith("true") || part_of_string.StartsWith("false") || part_of_string.StartsWith("null") || part_of_string.StartsWith("char") || part_of_string.StartsWith("string") || part_of_string.StartsWith("void") || part_of_string.StartsWith("break") || part_of_string.StartsWith("continue"))
            {
                return part_of_string.Substring(setTokenForKeyword(part_of_string, t));
            }
            //E F 识别标志符
            else if (Char.IsLetter(part_of_string[0]))
            {
                int i = 0;
                while (i < part_of_string.Length)
                {
                    if (Char.IsLetterOrDigit(part_of_string[i]) || part_of_string[i] == '_')
                    {
                        //继续遍历
                        i++;
                        //确保
                        if (i == part_of_string.Length)
                        {
                            if (part_of_string[i - 1] != '_')
                            {
                                //识别到一个完整的标识符，标识符长度i
                                t.code = 50;
                                t.content = part_of_string.Substring(0, i);
                                return part_of_string.Substring(i);
                            }
                            else
                            {
                                //标识符结尾是一个_下划线，返回词法错误
                                throw new LexException("标识符不能以_结尾", line_num);
                            }
                        }
                    }
                    else
                    {
                        if (part_of_string[i - 1] != '_')
                        {
                            //识别到一个完整的标识符，标识符长度i
                            t.code = 50;
                            t.content = part_of_string.Substring(0, i);
                            return part_of_string.Substring(i);
                        }
                        else
                        {
                            //标识符结尾是一个_下划线，返回词法错误
                            throw new LexException("标识符不能以_结尾", line_num);
                        }
                    }
                }
                throw new LexException("不可能出现的情况", line_num);
            }
            //I J K 很多代码都在处理防范数组越界的问题
            else if (char.IsDigit(part_of_string[0]))
            {
                int i = 1;
                i = 0;
                while (char.IsDigit(part_of_string[i]))
                {
                    i++;
                    if (i == part_of_string.Length)
                    {
                        t.code = 49;
                        t.content = part_of_string;
                        return part_of_string.Substring(part_of_string.Length);
                    }
                }
                //小数
                if (part_of_string[i] == '.')
                {
                    i++;
                    // 1. 这种情况
                    if (i == part_of_string.Length || !char.IsDigit(part_of_string[i]))
                    {
                        t.code = 49;
                        t.content = part_of_string.Substring(0, i);
                        return part_of_string.Substring(i);
                    }
                    while (char.IsDigit(part_of_string[i]))
                    {
                        i++;
                        //防越界
                        if (i == part_of_string.Length)
                        {
                            t.code = 49;
                            t.content = part_of_string;
                            return part_of_string.Substring(part_of_string.Length);
                        }
                    }
                    t.code = 49;
                    t.content = part_of_string.Substring(0, i);
                    return part_of_string.Substring(i);
                }
                //错误的标识符命名int 12a或者12_这种
                else if (char.IsLetter(part_of_string[i]) || part_of_string[i] == '_')
                {
                    throw new LexException("错误的标识符命名形式：标识符不得以数字开头", line_num);
                }
                //整数
                else
                {
                    t.code = 49;
                    t.content = part_of_string.Substring(0, i);
                    return part_of_string.Substring(i);
                }
            }
            //M N
            else if (part_of_string[0] == '&')
            {
                if(part_of_string.Length == 1)
                {
                    throw new LexException("识别到单个&，暂不支持分析。请注意根据CMM语法，应当由两个连续的&（如：&&）作为比较符与", line_num);
                }
                if (part_of_string[1] == '&')
                {
                    t.code = 29;
                    t.content = "&&";
                    return part_of_string.Substring(2);
                }
                else
                {
                    throw new LexException("识别到单个&，暂不支持分析。请注意根据CMM语法，应当由两个连续的&（如：&&）作为比较符与", line_num);
                }
            }
            //O P
            else if (part_of_string[0] == '|')
            {
                if (part_of_string.Length == 1)
                {
                    throw new LexException("识别到单个|，暂不支持分析。请注意根据CMM语法，应当由两个连续的|（如：||）作为比较符或", line_num);
                }
                if (part_of_string[1] == '|')
                {
                    t.code = 30;
                    t.content = "||";
                    return part_of_string.Substring(2);
                }
                else
                {
                    throw new LexException("识别到单个|，暂不支持分析。请注意根据CMM语法，应当由两个连续的|（如：||）作为比较符或", line_num);
                }
            }
            else if(part_of_string[0] == '_')
            {
                throw new LexException("标识符必须以字母开头，且不能以下划线结尾", line_num);
            }
            else if(part_of_string[0] == '.')
            {
                throw new LexException("出现了不符合词法定义的单个或多个.小数点", line_num);
            }
            else if(part_of_string[0] == '\'')
            {
                if(part_of_string.Length == 1)
                {
                    throw new LexException("未检测到char的两个匹配的单引号", line_num);
                }
                else if(part_of_string.Length == 2)
                {
                    if(part_of_string[0] == part_of_string[1])
                    {
                        t.code = 45;
                        t.content = "";
                        return part_of_string.Substring(2);
                    }
                    else
                    {
                        throw new LexException("没有检测到char的两个匹配的单引号", line_num);
                    }
                }
                else if(part_of_string[0] == part_of_string[1])
                {
                    t.code = 45;
                    t.content = "";
                    return part_of_string.Substring(2);
                }
                else
                {
                    //里面不可能是单引号了已经
                    if (part_of_string[0] == part_of_string[2])
                    {
                        t.code = 45;
                        t.content = part_of_string.Substring(1, 1);
                        return part_of_string.Substring(3);
                    }
                    else if (part_of_string[0] == part_of_string[3])
                    {
                        if (part_of_string[1] == '\\')
                        {
                            t.code = 45;
                            t.content = part_of_string.Substring(1, 2);
                            return part_of_string.Substring(4);
                        }
                        else
                        {
                            throw new LexException("单引号使用不符合词法规范，请注意char类型存储字符最多只有一个", line_num);
                        }
                    }
                    else
                    {
                        throw new LexException("单引号使用不符合词法规范，请注意char类型存储字符最多只有一个", line_num);
                    }
                }
            }
            else if (part_of_string[0] == '"')
            {
                int i = 1;
                while(i < part_of_string.Length)
                {
                    if(part_of_string[i] == '"')
                    {
                        t.code = 46;
                        t.content = part_of_string.Substring(1, i-1);
                        return part_of_string.Substring(i+1);
                    }
                    i++;
                }
                throw new LexException("未检测到string的两个匹配的双引号", line_num);
            }
            else
            {
                throw new LexException("无法识别的字符", line_num);
            }
        }

        //在A这种情况设置token的code
        private static void setTokenCodeA(char v, Token t)
        {
            switch (v)
            {
                case '+':
                    t.code = 11;
                    break;
                case '-':
                    t.code = 12;
                    break;
                case '*':
                    t.code = 13;
                    break;
                case '/':
                    t.code = 14;
                    break;
                case '=':
                    t.code = 15;
                    break;
                case '>':
                    t.code = 22;
                    break;
                case '!':
                    t.code = 28;
                    break;
            }
        }

        //在B这种情况设置token的code
        private static void setTokenCodeB(string v, Token t)
        {
            switch (v[0])
            {
                case '+':
                    t.code = 16;
                    break;
                case '-':
                    t.code = 17;
                    break;
                case '*':
                    t.code = 18;
                    break;
                case '/':
                    t.code = 19;
                    break;
                case '=':
                    t.code = 23;
                    break;
                case '>':
                    t.code = 25;
                    break;
                case '!':
                    t.code = 27;
                    break;
            }
        }

        //在D这种情况设置token的code
        private static void setTokenCodeD(char v, Token t)
        {
            if (v == '=')
            {
                t.code = 26;
            }
            else if (v == '>')
            {
                t.code = 24;
            }
            else
            {
                Console.WriteLine("设置D情况下token code时出现不可能的情况");
            }
        }

        //当第一个是关键字时，设置token值，并且返回token长度
        private static int setTokenForKeyword(string s, Token t)
        {
            if (s.StartsWith("if"))
            {
                t.code = 1;
                t.content = "if";
                return 2;
            }
            else if (s.StartsWith("else"))
            {
                t.code = 2;
                t.content = "else";
                return 4;
            }
            else if (s.StartsWith("int"))
            {
                t.code = 3;
                t.content = "int";
                return 3;
            }
            else if (s.StartsWith("while"))
            {
                t.code = 4;
                t.content = "while";
                return 5;
            }
            else if (s.StartsWith("read"))
            {
                t.code = 5;
                t.content = "read";
                return 4;
            }
            else if (s.StartsWith("write"))
            {
                t.code = 6;
                t.content = "write";
                return 5;
            }
            else if (s.StartsWith("real"))
            {
                t.code = 7;
                t.content = "real";
                return 4;
            }
            else if (s.StartsWith("true"))
            {
                t.code = 8;
                t.content = "true";
                return 4;
            }
            else if (s.StartsWith("false"))
            {
                t.code = 9;
                t.content = "false";
                return 5;
            }
            else if (s.StartsWith("null"))
            {
                t.code = 10;
                t.content = "null";
                return 4;
            }
            else if (s.StartsWith("char"))
            {
                t.code = 51;
                t.content = "char";
                return 4;
            }
            else if (s.StartsWith("string"))
            {
                t.code = 52;
                t.content = "string";
                return 6;
            }
            else if (s.StartsWith("void"))
            {
                t.code = 53;
                t.content = "void";
                return 4;
            }
            else if (s.StartsWith("return"))
            {
                t.code = 54;
                t.content = "return";
                return 6;
            }
            else if (s.StartsWith("break"))
            {
                t.code = 55;
                t.content = "break";
                return 5;
            }
            else if (s.StartsWith("continue"))
            {
                t.code = 56;
                t.content = "continue";
                return 8;
            }
            else
            {
                Console.WriteLine("设置关键字token时字符串出现不可能的情况");
                throw new LexException("设置关键字token时字符串出现不可能的情况", -1);
            }
        }

        //在G这种情况设置token的code
        private static void setTokenCodeG(char v, Token t)
        {
            switch (v)
            {
                case ',':
                    t.code = 47;
                    break;
                case ';':
                    t.code = 48;
                    break;
                case '(':
                    t.code = 31;
                    break;
                case ')':
                    t.code = 32;
                    break;
                case '{':
                    t.code = 33;
                    break;
                case '}':
                    t.code = 34;
                    break;
                case '[':
                    t.code = 35;
                    break;
                case ']':
                    t.code = 36;
                    break;
            }
        }
    }
}
