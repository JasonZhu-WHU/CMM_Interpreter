   M S B A M L   `   `   `  ���� J  FCMM_Interpreter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:CMM_InterpreterCMM_Interpreter  H DShowMeTheXAML, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullD2clr-namespace:ShowMeTheXAML;assembly=ShowMeTheXAMLShowMeTheXAML      CMM_Interpreter.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     /localclr-namespace:CMM_Interpreter     U QMaterialDesignThemes.Wpf, Version=2.6.0.325, Culture=neutral, PublicKeyToken=nullFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  =smtx2clr-namespace:ShowMeTheXAML;assembly=ShowMeTheXAML     MaterialDesignBody#���   $��Regular"�$��13(�&   System.Windows.Media.TextOptions    TextFormattingMode$  Ideal=�   TextRenderingMode$
 Auto=�  MaterialDesignPaper#���    MaterialDesignFont#���   � Title$ CMM_Interpreter��$
��1080��$
��1920�� � 	Resources  �� MergedDictionaries �� Source��  �� $g apack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.TextBox.xaml?�%    .���  � ColumnDefinitions �� .+�c� $
��Left=�$��38��$��10,0,0,0q�$	��Top=�$
��1893��]� $��0,0,0.333,-0.333q�.+�  ShowMeTheXAML.XamlDisplay  
   Key$ menus_1��$	��Top=�$��5 5 0 5q�.��o�  o� 
IsMainMenu . .@�m� $��_File��.@�m� -   $��New_File��$	��New��	 m� Icon	 '  !MaterialDesignThemes.Wpf.PackIcon  �  �ControlzEx.PackIconBase`1[[MaterialDesignThemes.Wpf.PackIconKind, MaterialDesignThemes.Wpf, Version=2.6.0.325, Culture=neutral, PublicKeyToken=null]]
   Kind$

 Note=�m� -   $
��Open��$
��Open��	   $
 Folder=�m� -   $
��Save��$
��Save��	   $
 ContentSave=�m� $��	Save As..��m� -   $
��Exit��$
��Exit�� m� InputGestureText$ Ctrl+E��	   $
 	ExitToApp=��� m� $��	Excellent�� m� IsCheckable .  m� 	IsChecked . m� $��Rubbish�� . m� $��
Dig Deeper��$ Ctrl+D��.@�m� $��Enlightenment?�� . m� $��Disappointment�� . m� $��Look Deeper��$ Ctrl+D��.@�m� $��Plain��m� $��	Ice Cream��m� $��_Edit��.@�m� $
��_Cut�� m� Command$	 Cut��	   $
 
ContentCut=�m� $��_Copy��$
 Copy��	   $
 ContentCopy=�m� $��_Paste��$ Paste��	   $
 ContentPaste=�  $ menus_2��$	��Top=�$��5 0 0 0q�.��`� .�c�   MaterialDesignToolBar#���  }�.  .@��� -    � ToolTip$ Follow me on Twitter��.��  $
 
TwitterBox=��� $
 Save��.��  $
 ContentSave=��� �� $	��Cut��$	 Cut�� c� OverflowMode$ AsNeeded=�.��  $
 
ContentCut=��� $
��Copy��$ Copy that stuff��$ AsNeeded=�.��  $
 ContentCopy=��� �� $��Paste��$ Paste some stuff��$ AsNeeded=�.��  $
 ContentPaste=�%  MaterialDesignThemes.Wpf.Badged  $ AsNeeded=�  ControlzEx.BadgedEx   Badge 0  *MaterialDesignThemes.Wpf.PackIconExtension  *Alert+.���� $ Badge it up!��#��[" �.��  $
 AirplaneTakeoff=��� �� .@��� $< 6This is a lonley toggle with TextBlock instead of icon��.���� W�� ��  �� SelectedIndex$ 0�� �� .+��� $)��#MaterialDesignToolToggleFlatListBox���� $��Exclusive selection���� $A��;ListBoxAssist.IsToggle allows more natural toggle behaviour��.@��� .��  $
 FormatAlignLeft=��� .��  $
 FormatAlignCenter=��� .��  $
 FormatAlignRight=��� .��  $
 FormatAlignJustify=��� ��  �� SelectionMode$ Extended=� �� .+��� $%��MaterialDesignToolToggleListBox���� $��Multiple selection���� $A��;ListBoxAssist.IsToggle allows more natural toggle behaviour��.@��� .��  $
 
FormatBold=��� .��  $
 FormatItalic=��� -    ��Name$ UnderlineCheckbox��.��  $
 FormatUnderline=��� �� $��
Font size:��$��Center=��� .@��� $��10����  �� 
IsSelected . $��12���� $��14���� $��16���� .��Check�� $ 
Take a nap��$ Always=�.��  $
 Hotel=�	�  	� 	GroupName$	 XXX��$��Radio��	� $	 XXX��$��Ga Ga���� 	� $	 YYY�� ! MaterialDesignToolRadioButton#��[ .��  $
 Radio=�	� $	 YYY��#��[ .��  $
 EmoticonPoop=��� d�   $ fields_1��$
��1584��$��0,36,0,419.667q�.���� -    ��Name$ NameTextBox��)  #MaterialDesignThemes.Wpf.HintAssist   Hint$
 Name��$��1124y1io14y34i1o��