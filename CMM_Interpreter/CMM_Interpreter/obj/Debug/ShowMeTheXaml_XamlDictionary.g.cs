
using System.Collections.Generic;

namespace ShowMeTheXAML
{
    public static class XamlDictionary
    {
        static XamlDictionary()
        {
            XamlResolver.Set("AST", @"<smtx:XamlDisplay Key=""AST"" VerticalContentAlignment=""Top"" Margin=""0,61,10.333,-0.333"" RenderTransformOrigin=""0.928,0.495"" xmlns:smtx=""clr-namespace:ShowMeTheXAML;assembly=ShowMeTheXAML"">
  <TreeView Name=""myAST"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <TreeViewItem Name=""myAST_Node1"">
      <TreeViewItem.Header>
        <StackPanel Orientation=""Horizontal"">
          <Viewbox Width=""16"" Height=""16"">
            <Canvas Width=""24"" Height=""24"">
              <Path Data=""M3,12V6.75L9,5.43V11.91L3,12M20,3V11.75L10,11.9V5.21L20,3M3,13L9,13.09V19.9L3,18.75V13M20,13.25V22L10,20.09V13.1L20,13.25Z"" Fill=""{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type TreeViewItem}}, Path=Foreground}"" />
            </Canvas>
          </Viewbox>
          <TextBlock Margin=""8 0 0 0"">Abstract Syntax Tree</TextBlock>
        </StackPanel>
      </TreeViewItem.Header>
    </TreeViewItem>
  </TreeView>
</smtx:XamlDisplay>");
XamlResolver.Set("menus_1", @"<smtx:XamlDisplay Key=""menus_1"" DockPanel.Dock=""Top"" Margin=""5,5,0,5"" BorderThickness=""0"" BorderBrush=""#FF686868"" HorizontalAlignment=""Left"" Width=""1068"" xmlns:smtx=""clr-namespace:ShowMeTheXAML;assembly=ShowMeTheXAML"">
  <Menu IsMainMenu=""True"" Width=""130"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <MenuItem Header=""_File"">
      <MenuItem Header=""New"" Name=""New_File"" Click=""New_File_Click"">
        <MenuItem.Icon>
          <materialDesign:PackIcon Kind=""Note"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
        </MenuItem.Icon>
      </MenuItem>
      <MenuItem Header=""Open"" Name=""Open"" Click=""Open_Click"">
        <MenuItem.Icon>
          <materialDesign:PackIcon Kind=""Folder"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
        </MenuItem.Icon>
      </MenuItem>
      <MenuItem Header=""Save"" Name=""Save"" Click=""Save_Click"">
        <MenuItem.Icon>
          <materialDesign:PackIcon Kind=""ContentSave"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
        </MenuItem.Icon>
      </MenuItem>
      <MenuItem Header=""Save As.."" />
      <MenuItem Header=""Exit"" InputGestureText=""Ctrl+E"" Name=""Exit"" Click=""Exit_Click"">
        <MenuItem.Icon>
          <materialDesign:PackIcon Kind=""ExitToApp"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
        </MenuItem.Icon>
      </MenuItem>
      <Separator />
      <MenuItem Header=""Excellent"" IsCheckable=""True"" IsChecked=""True"" />
      <MenuItem Header=""Rubbish"" IsCheckable=""True"" />
      <MenuItem Header=""Dig Deeper"" InputGestureText=""Ctrl+D"">
        <MenuItem Header=""Enlightenment?"" IsCheckable=""True"" />
        <MenuItem Header=""Disappointment"" IsCheckable=""True"" />
      </MenuItem>
      <MenuItem Header=""Look Deeper"" InputGestureText=""Ctrl+D"">
        <MenuItem Header=""Plain"" />
        <MenuItem Header=""Ice Cream"" />
      </MenuItem>
    </MenuItem>
    <MenuItem Header=""_Edit"">
      <MenuItem Header=""_Cut"" Command=""Cut"">
        <MenuItem.Icon>
          <materialDesign:PackIcon Kind=""ContentCut"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
        </MenuItem.Icon>
      </MenuItem>
      <MenuItem Header=""_Copy"" Command=""Copy"">
        <MenuItem.Icon>
          <materialDesign:PackIcon Kind=""ContentCopy"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
        </MenuItem.Icon>
      </MenuItem>
      <MenuItem Header=""_Paste"" Command=""Paste"">
        <MenuItem.Icon>
          <materialDesign:PackIcon Kind=""ContentPaste"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
        </MenuItem.Icon>
      </MenuItem>
    </MenuItem>
  </Menu>
</smtx:XamlDisplay>");
XamlResolver.Set("menus_2", @"<smtx:XamlDisplay Key=""menus_2"" DockPanel.Dock=""Top"" BorderThickness=""0"" BorderBrush=""#FF909090"" HorizontalAlignment=""Left"" Width=""1073"" xmlns:smtx=""clr-namespace:ShowMeTheXAML;assembly=ShowMeTheXAML"">
  <ToolBarTray Margin=""0,0,-496,-0.333"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
    <ToolBar Style=""{DynamicResource MaterialDesignToolBar}"" ClipToBounds=""False"">
      <Button ToolTip=""Save"" Click=""Save_Click"">
        <materialDesign:PackIcon Kind=""ContentSave"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <Separator />
      <Button Command=""Cut"" ToolTip=""Cut"" ToolBar.OverflowMode=""AsNeeded"">
        <materialDesign:PackIcon Kind=""ContentCut"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <Button Command=""Copy"" ToolTip=""Copy that stuff"" ToolBar.OverflowMode=""AsNeeded"">
        <materialDesign:PackIcon Kind=""ContentCopy"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <Separator />
      <Button Command=""Paste"" ToolTip=""Paste some stuff"" ToolBar.OverflowMode=""AsNeeded"">
        <materialDesign:PackIcon Kind=""ContentPaste"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <Button ToolTip=""词法分析"" Click=""Lex_OnClick"">
        <materialDesign:PackIcon Kind=""ScriptText"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <Button ToolTip=""语法分析"" Click=""ParserButton_OnClick"">
        <materialDesign:PackIcon Kind=""VectorPolygon"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <Button ToolTip=""解释运行"" Click=""Semantic_Analyze_Button_Click"" Style=""{StaticResource {x:Static ToolBar.ButtonStyleKey}}"">
        <materialDesign:PackIcon Kind=""Play"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <!-- when badging in a toolbar, make sure the parent ToolBar.ClipToBounds=""False"", and
                     manually apply the button style -->
      <!--<materialDesign:Badged ToolBar.OverflowMode=""AsNeeded"" Badge=""{materialDesign:PackIcon Alert}"" >
                            <Button ToolTip=""解释运行"" Style=""{StaticResource {x:Static ToolBar.ButtonStyleKey}}"">
                                <materialDesign:PackIcon Kind=""Play"" />
                            </Button>
                        </materialDesign:Badged>-->
      <Label Content=""Font size:"" VerticalAlignment=""Center"" />
      <ComboBox>
        <ComboBoxItem Content=""10"" />
        <ComboBoxItem IsSelected=""True"" Content=""12"" />
        <ComboBoxItem Content=""14"" />
        <ComboBoxItem Content=""16"" />
      </ComboBox>
      <CheckBox Name=""Lex_Check"">
                            导出词法分析结果
                        </CheckBox>
      <CheckBox Name=""Gra_Output"" Click=""Gra_Output_Click"">
                            导出语法树
                        </CheckBox>
      <Button ToolTip=""Take a nap"" ToolBar.OverflowMode=""Always"">
        <materialDesign:PackIcon Kind=""Hotel"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </Button>
      <RadioButton GroupName=""XXX"" Content=""异步方式"" />
      <!--<RadioButton GroupName=""XXX"" Content=""Ga Ga"" />-->
      <Separator />
      <RadioButton GroupName=""YYY"" Style=""{StaticResource MaterialDesignToolRadioButton}"">
        <materialDesign:PackIcon Kind=""CashUsd"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </RadioButton>
      <!--<RadioButton GroupName=""YYY"" Style=""{StaticResource MaterialDesignToolRadioButton}"">
                            <materialDesign:PackIcon Kind=""Tree""/>
                        </RadioButton>-->
      <RadioButton GroupName=""YYY"" Style=""{StaticResource MaterialDesignToolRadioButton}"">
        <materialDesign:PackIcon Kind=""About"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" />
      </RadioButton>
      <Separator />
      <ToggleButton />
    </ToolBar>
  </ToolBarTray>
</smtx:XamlDisplay>");
XamlResolver.Set("fields_1", @"<smtx:XamlDisplay Key=""fields_1"" Margin=""30,0,42.333,0"" Cursor=""IBeam"" Background=""#FFF2EEC7"" DockPanel.Dock=""Top"" Height=""246"" xmlns:smtx=""clr-namespace:ShowMeTheXAML;assembly=ShowMeTheXAML"">
  <RichTextBox x:Name=""codeBox"" VerticalContentAlignment=""Top"" HorizontalContentAlignment=""Left"" materialDesign:ScrollBarAssist.ButtonsVisibility=""Visible"" AcceptsReturn=""True"" AcceptsTab=""True"" materialDesign:HintAssist.Hint=""请打开或者新建一个文件，或者直接在这里进行编辑"" Margin=""0,0,-506.333,0"" VerticalScrollBarVisibility=""Auto"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" />
</smtx:XamlDisplay>");
XamlResolver.Set("fields_2", @"<smtx:XamlDisplay Key=""fields_2"" Cursor=""IBeam"" Background=""#FF616161"" DockPanel.Dock=""Top"" Height=""244"" Margin=""30,0,42.333,0"" Foreground=""#DDFFFFFF"" xmlns:smtx=""clr-namespace:ShowMeTheXAML;assembly=ShowMeTheXAML"">
  <TextBox x:Name=""resultBox"" VerticalContentAlignment=""Top"" HorizontalContentAlignment=""Left"" materialDesign:ScrollBarAssist.ButtonsVisibility=""Visible"" AcceptsReturn=""True"" AcceptsTab=""True"" materialDesign:HintAssist.Hint=""输出结果"" VerticalScrollBarVisibility=""Auto"" HorizontalScrollBarVisibility=""Auto"" Margin=""0,0,-500.333,0"" xmlns:materialDesign=""http://materialdesigninxaml.net/winfx/xaml/themes"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" />
</smtx:XamlDisplay>");
        }
    }
}