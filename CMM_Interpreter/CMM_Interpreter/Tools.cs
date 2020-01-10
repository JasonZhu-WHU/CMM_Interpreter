using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CMM_Interpreter
{
    class Tools
    {
        public Tools()
        {

        }

        //取得指定RichTextBox的内容Text：
        public static string getRichTextBox_Text(RichTextBox richTextBox)
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return textRange.Text;
        }

        //将RichTextBox的内容保存为文件：
        public static void SaveFile(string filename, RichTextBox richTextBox)
        {
            FileStream xiaFile = new FileStream(filename, FileMode.Create);
            byte[] buf = Encoding.UTF8.GetBytes(getRichTextBox_Text(richTextBox));
            xiaFile.Write(buf, 0, buf.Length);
            xiaFile.Flush();
            xiaFile.Close();
        }

        public static object transformTokenToSymbol(Token t)
        {
            if(t.code == 49)
            {
                if (t.content.Contains("."))
                {
                    return new RealStackElement(t.lineNum, t.content);
                }
                else
                {
                    return new IntStackElement(t.lineNum, t.content);
                }
            }
            if(t.code == 50)
            {
                return new IdentifierStackElement(t.lineNum, t.content);
            }
            if (t.code == 8 || t.code == 9)
            {
                throw new ParserException("这些功能还没实现呢！");
            }
            if(t.code == 45)
            {
                return new CharStackElement(t.lineNum, t.content);
            }
            if(t.code == 46)
            {
                return new StringStackElement(t.lineNum, t.content);
            }
            else
            {
                return new OtherTerminalStackElement(t.lineNum, t.content);
            }
        }

        public static void cleanAll()
        {
            MainWindow.tokens_Lst.Clear();
            MainWindow.allTokens.Clear();
            MainWindow.lexer_successfully = false;
            Parser.stack_to_parse.Clear();
            Parser.all_tokens.Clear();
        }

        //接收到某行的字符串，并转换为对应的tokens
        public static List<Token> lexAnalyze(string s, int line_num)
        {
            //预处理部分Preprocess
            int comment_index = s.IndexOf("//");
            if (comment_index != -1)
            {
                s = s.Remove(comment_index);
            }
            s = s.Trim();
            
            //至此，所有注释都已经被清除，每行开始和结束的空格tab等等也被清除
            List<Token> tokens = new List<Token>();
            if(s is null || s == "")
            {
                //若该行全部是注释/空白，即没有任何有效信息，那么返回空List
                return tokens;
            }
            if (s.IndexOf("/*") != -1 && s.IndexOf("*/") != -1)
            {
                s = s.Substring(0, s.IndexOf("/*"));
            }
            if (!Token.comment_before)
            {
                if (s.IndexOf("*/") != -1)
                {
                    MessageBox.Show("第" + line_num + "行：未识别到开始的多行注释符号");
                }
            }
            if (Token.comment_before)
            {
                if (s.IndexOf("*/") != -1)
                {
                    Token.comment_before = false;
                    return tokens;
                }
                return tokens;
            }
            if (s.IndexOf("/*") != -1)
            {
                Token.comment_index = line_num;
                Token.comment_before = true;
                s = s.Substring(0, s.IndexOf("/*"));
            }


            //开始真正的词法分析
            while (!string.IsNullOrWhiteSpace(s))
            {
                Token t = new Token();
                //分析token并调整读指针，具体请转至方法实现查看
                try
                {
                    s = Token.getNextToken(s, line_num, t);
                    tokens.Add(t);
                }
                //break很关键，保证它不会一直报错，因为本身的while循环条件使得其出错时很难跳出，所以break跳出这一行识别
                catch(LexException le)
                {
                    MessageBox.Show(le.getExceptionMsg());
                    MainWindow.lexer_successfully = false;
                    break;
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.ToString());
                    MainWindow.lexer_successfully = false;
                    break;
                }
                finally
                {
                    s = s.Trim();
                }
            }
            return tokens;
        }

        //词法分析后，把词法分析产生的tokens综合起来放到一个一个串里面
        public static void getAllTokens()
        {
            MainWindow.allTokens = new List<Token>();
            foreach(List<Token> lst in MainWindow.tokens_Lst)
            {
                foreach(Token t in lst)
                {
                    MainWindow.allTokens.Add(t);
                    Console.WriteLine(t.ToString());
                }
            }
        }
    }
}
