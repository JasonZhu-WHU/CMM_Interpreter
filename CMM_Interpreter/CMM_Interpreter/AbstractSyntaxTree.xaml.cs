using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CMM_Interpreter
{
    /// <summary>
    /// AbstractSyntaxTree.xaml 的交互逻辑
    /// </summary>
    public partial class AbstractSyntaxTree : Window
    {
        public AbstractSyntaxTree()
        {
            InitializeComponent();
            drawTheAbstractSyntaxTree();
        }

        private void drawTheAbstractSyntaxTree()
        {
            int index = 0;
            while(Parser.stack_to_parse.Count != 0)
            {
                //获取栈的最里面的节点
                StackElement first_ele = (StackElement)Parser.stack_to_parse.Peek();
                //准备添加的树节点
                TreeViewItem new_node = new TreeViewItem();
                if (first_ele.type_code == 6)
                {
                    Parser.stack_to_parse.Pop();
                    continue;
                }
                else if (first_ele.type_code != 3)
                {
                    throw new ParserException("怎么会有不是非终结符的最外层栈元素，它怎么过的语法分析？");
                }
                else
                {
                    NonterminalStackElement e = (NonterminalStackElement)first_ele;
                    try
                    {
                        Console.WriteLine("index" + index);
                        recursiveAddNodes(e, new_node);
                    }
                    catch(Exception ee)
                    {
                        Console.WriteLine(ee);
                    }
                    myAST_Node1.Items.Add(new_node);
                    return;
                }
            }
        }

        private void recursiveAddNodes(StackElement e, TreeViewItem newItem)
        {
            try
            {
                if (e.type_code == 6)
                {
                    throw new ParserException("这个表示状态的栈元素不应该出现在Tree的递归子结点中！");
                }
                else if (e.type_code == 1)
                {
                    IdentifierStackElement ele = (IdentifierStackElement)e;
                    newItem.Header = "第" + ele.linenum + "行的标识符：" + ele.content;
                }
                else if (e.type_code == 2)
                {
                    IntStackElement ele = (IntStackElement)e;
                    newItem.Header = "第" + ele.linenum + "行的整数：" + ele.content;
                }
                else if (e.type_code == 3)
                {
                    NonterminalStackElement ele = (NonterminalStackElement)e;
                    newItem.Header = "非终结符：" + ele.name;
                }
                else if (e.type_code == 4)
                {
                    OtherTerminalStackElement ele = (OtherTerminalStackElement)e;
                    newItem.Header = "第" + ele.linenum + "行的终结符：" + ele.content;
                }
                else if (e.type_code == 5)
                {
                    RealStackElement ele = (RealStackElement)e;
                    newItem.Header = "第" + ele.linenum + "行的实数：" + ele.content;
                }
                else if (e.type_code == 7)
                {
                    CharStackElement ele = (CharStackElement)e;
                    newItem.Header = "第" + ele.linenum + "行的字符：" + ele.content;
                }
                else if (e.type_code == 8)
                {
                    StringStackElement ele = (StringStackElement)e;
                    newItem.Header = "第" + ele.linenum + "行的字符串：" + ele.content;
                }
                else
                {
                    throw new ParserException("这是啥栈元素？");
                }
                //若是终结符，branches就是空，若不是非终结符，它也终究会走到末端叶子节点，也就是非终结符，然后递归终止
                if(e.branches.Count != 0)
                {
                    foreach (StackElement ele in e.branches)
                    {
                        TreeViewItem new_node = new TreeViewItem();
                        newItem.Items.Add(new_node);
                        recursiveAddNodes(ele, new_node);
                    }
                }
            }
            catch(ParserException pe)
            {
                MessageBox.Show(pe.err_msg);
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        private void Expand_Btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in myAST.Items)
            {
                DependencyObject dObject = myAST.ItemContainerGenerator.ContainerFromItem(item);
                ((TreeViewItem)dObject).ExpandSubtree();
            }
        }

        private void Collapse_Btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in myAST.Items)
            {
                DependencyObject dObject = myAST.ItemContainerGenerator.ContainerFromItem(item);
                CollapseTreeviewItems(((TreeViewItem)dObject));
            }
        }

        private void CollapseTreeviewItems(TreeViewItem Item)
        {
            Item.IsExpanded = false;

            foreach (var item in Item.Items)
            {
                DependencyObject dObject = myAST.ItemContainerGenerator.ContainerFromItem(item);

                if (dObject != null)
                {
                    ((TreeViewItem)dObject).IsExpanded = false;

                    if (((TreeViewItem)dObject).HasItems)
                    {
                        CollapseTreeviewItems(((TreeViewItem)dObject));
                    }
                }
            }
        }
    }
}
