using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic;

namespace CMM_Interpreter
{
    class ReaderHelper
    {
        internal static Value read_single_value(string type, int line_num)
        {
            try
            {
                string input = Interaction.InputBox("请输入第" + line_num + "行要读入的一个" + type + "类型的值", "Read", "input", -1, -1);
                if(input.Length == 0)
                {
                    throw new ExecutorException("用户取消输入或者输入为空，语义分析中断！", line_num);
                }
                if (type == "int")
                {
                    return new IntValue("int", true, input, line_num);
                }
                else if(type == "real")
                {
                    return new RealValue("real", true, input, line_num);
                }
                else if(type == "char")
                {
                    return new CharValue("char", true, input, line_num);
                }
                else if(type == "string")
                {
                    return new StringValue("string", true, input, line_num);
                }
                else
                {
                    throw new ExecutorException("数据读取中出现类型异常");
                }
            }
            catch(ExecutorException ee)
            {
                throw ee;
            }
            catch (Exception ex)
            {
                throw new ExecutorException("输入数据类型不符", line_num);
            }
        }

        internal static Value read_array_value(string type, int line_num, int length)
        {
            try
            {
                string input = Interaction.InputBox("请输入第" + line_num + "行要读入的一个" + type + "类型的数组，以|来区分。", "Read", "input", -1, -1);
                string[] s = input.Split('|');
                if(s.Length > length)
                {
                    throw new ExecutorException("输入数组长度大于声明长度");
                }
                if (type == "intArray")
                {
                    int[] array = new int[length];
                    for(int i = 0; i < s.Length; i++)
                    {
                        try
                        {
                            array[i] = int.Parse(s[i]);
                        }
                        catch
                        {
                            throw new ExecutorException("输入的数组中第" + i + "个元素与类型" + type + "不符合", line_num);
                        }
                    }
                    return new IntArrayValue("intArray", true, array.Length, array, line_num);
                }
                else if (type == "realArray")
                {
                    double[] array = new double[length];
                    for (int i = 0; i < s.Length; i++)
                    {
                        try
                        {
                            array[i] = double.Parse(s[i]);
                        }
                        catch (Exception ee)
                        {
                            throw new ExecutorException("输入的数组中第" + i + "个元素与类型" + type + "不符合", line_num);
                        }
                    }
                    return new RealArrayValue("intArray", true, array.Length, array, line_num);
                }
                else if (type == "charArray")
                {
                    string[] array = new string[length];
                    return new CharArrayValue("intArray", true, array.Length, array, line_num);
                }
                else if (type == "stringArray")
                {
                    string[] array = new string[length];
                    return new StringArrayValue("string", true, array.Length, array, line_num);
                }
                else
                {
                    throw new ExecutorException("数据读取中出现类型异常");
                }
            }
            catch (ExecutorException ex)
            {
                throw ex;
            }
            catch(Exception ee)
            {
                throw new ExecutorException("数据读取中出现类型异常" + Environment.NewLine + ee.ToString());
            }
        }

        //Process process = new Process();
        //process.StartInfo.FileName = @"D:\RunningWindow.exe";
        ////权限问题不行
        ////process.StartInfo.FileName = @"C: \Users\35131\source\repos\CMM_Interpreter\RunningWindow\bin\Debug\RunningWindow.exe"; //启动的应用程序名称
        //process.StartInfo.Arguments = "我是由控制台程序传过来的参数，如果传多个参数用空格隔开" + " 第二个参数";
        //process.StartInfo.CreateNoWindow = true;
        ////UseShellExecute就不能直接重定向！
        //process.StartInfo.UseShellExecute = true;
        //process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        //process.StartInfo.RedirectStandardOutput = true;
        //process.StartInfo.RedirectStandardInput = true;
        //process.StartInfo.RedirectStandardError = true;
        //process.Start();
        //StreamReader sr = process.StandardOutput;
        //StreamWriter sw = process.StandardInput;
        //sw.WriteLine("请输入" + type + "类型参数");
        //string input = sr.ReadLine();
        //MessageBox.Show(input);
        //int return_value = process.ExitCode;
    }
}
