using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;

namespace CMM_Interpreter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //当前编辑文件
        public static string curr_file_name;
        public static bool lexer_successfully = false;
        public static List<List<Token>> tokens_Lst = new List<List<Token>>();
        public static List<Token> allTokens = new List<Token>();
        public static bool grammer_output = false;

        public MainWindow()
        {
            InitializeComponent();
            GrammerConfig.init();
        }

        //点击按钮，进行语法分析
        private void ParserButton_OnClick(object sender, RoutedEventArgs e)
        {
            parser_analyze();
        }

        //在工具栏/展开栏点击保存按钮 保存当前文件 若不明确保存目标则予以提示
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("点击Save");
            if(curr_file_name is null || curr_file_name == "")
            {
                MessageBox.Show("请打开一个文件或者先保存为某个文件");
                return;
            }
            Tools.SaveFile(curr_file_name, codeBox);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("关闭程序");
            if (MessageBox.Show("您确定退出程序？", "提示信息", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();//关闭
            }
        }

        //打开一个文件的处理函数
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("打开文件");
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\Desktop\\Interpreter";
            openFileDialog1.Filter = "(*.cmm)|*.cmm";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //此处做你想做的事 读取采取uft-8编码应该是
                string fileName = openFileDialog1.FileName;
                curr_file_name = fileName;
                FileStream fs;
                if (fileName != "")
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    using (fs)
                    {
                        TextRange text = new TextRange(codeBox.Document.ContentStart, codeBox.Document.ContentEnd);
                        text.Load(fs, DataFormats.Text);
                    }
                }
            }
        }

        private void New_File_Click(object sender, RoutedEventArgs e)
        {

        }

        //点击按钮，进行词法分析
        private void Lex_OnClick(object sender, RoutedEventArgs e)
        {
            lex_analyze();
        }

        //词法分析调用
        private void lex_analyze()
        {
            lexer_successfully = true;
            Console.WriteLine("开始词法分析");
            //首先刷新一下我们存tokens的数据结构
            tokens_Lst.Clear();

            //获取输入部分
            string codeBoxText = Tools.getRichTextBox_Text(codeBox);
            //按行切割
            string[] codeBoxText_delete_comment = codeBoxText.Split(Environment.NewLine.ToCharArray());
            int i = 0;
            //因为Split函数没有切割掉换行符，所以通过控制偶数行把换行符给过滤掉，最后一行也滤掉即可得到真正代码
            while (i < codeBoxText_delete_comment.Length - 2)
            {
                //真正执行部分
                //把第i（偶数行）行的内容全部传给lexAnalyze函数词法分析，再返回List<string>，并添加到大的tokens_lst集合
                tokens_Lst.Add(Tools.lexAnalyze(codeBoxText_delete_comment[i], (i + 2) / 2));
                i += 2;
            }

            if (Token.comment_before)
            {
                MessageBox.Show("警告：第" + Token.comment_index + "行存在未匹配的多行注释符号");
                lexer_successfully = false;
            }

            //输出显示结果部分
            resultBox.Text = "";
            for (int j = 0; j < tokens_Lst.Count; j++)
            {
                if (tokens_Lst[j].Count == 0)
                {
                    resultBox.Text += ("第" + (j + 1) + "行没有识别到任何token！" + Environment.NewLine);
                }
                else
                {
                    resultBox.Text += ("第" + (j + 1) + "行共有" + tokens_Lst[j].Count + "个tokens：" + Environment.NewLine);
                    foreach (var tok in tokens_Lst[j])
                    {
                        resultBox.Text += tok.ToString() + "  ";
                    }
                    resultBox.Text += Environment.NewLine;
                }
            }
            Tools.getAllTokens();
        }

        //语法分析调用
        private bool parser_analyze()
        {
            Tools.cleanAll();
            lex_analyze();
            //词法执行成功才会进行语法分析
            if (lexer_successfully)
            {
                //语法分析若失败，则会返回false
                return Parser.parse_tokens();
            }
            else
            {
                MessageBox.Show("词法分析出现错误，语法分析无法执行");
                return false;
            }
        }

        //语义分析调用
        private void Semantic_Analyze_Button_Click(object sender, RoutedEventArgs e)
        {
            if (parser_analyze() == true)
            {
                try
                {
                    Executor.semantic_analyze();
                }
                catch (ExecutorException ee)
                {
                    MessageBox.Show(ee.getExceptionMsg());
                }
                catch (Exception eee)
                {
                    MessageBox.Show(eee.ToString());
                    Console.WriteLine(eee.ToString());
                    Executor.showGlobalBindings();
                }
            }
            else
            {
                MessageBox.Show("语法分析出现错误，语义分析无法执行，解释执行失败。");
            }
        }

        public static bool StartProcess(string filename, string[] args)
        {
            try
            {
                string s = "";
                foreach (string arg in args)
                {
                    s = s + arg + " ";
                }
                s = s.Trim();
                var myprocess = new Process();
                var startInfo = new ProcessStartInfo(filename, s);
                myprocess.StartInfo = startInfo;
                //通过以下参数可以控制exe的启动方式，具体参照 myprocess.StartInfo.下面的参数，如以无界面方式启动exe等
                //myprocess.StartInfo.UseShellExecute = true;
                myprocess.Start();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("启动应用程序时出错！原因：" + ex.Message);
            }
            return false;
        }

        private void Gra_Output_Click(object sender, RoutedEventArgs e)
        {
            grammer_output = !grammer_output;
        }
    }
}
