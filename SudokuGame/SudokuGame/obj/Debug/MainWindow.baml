   M S B A M L   `   `   `  ����E  ASudokuGame, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:SudokuGame
SudokuGame  5   	   K*clr-namespace:SmartContractGUI2.ConvertersSmartContractGUI2.Converters  5   	       SudokuGame.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:SudokuGame     5   	   _ [MaterialDesignThemes.Wpf, Version=4.9.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   5conv*clr-namespace:SmartContractGUI2.Converters  5   	   -   5          � WindowStartupLocation$  CenterScreen=�5   	    � Icon$ ../Assets/pastime.png��5	   	   $	��650��5   	   $	��850��6   $��Copperplate Gothic Light)�6(   ���#3b444b$	��900��6c   $	��650��6r    � Title$ 
Sudoku ETH��5   	   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      ��5   
   �� 5       �� ImageSource$/ )../Assets/abstract-textured-backgound.jpg��6    j� Stretch$ UniformToFill=�6Q   .+�5!   
   �� $��0��6   $��	0,35,0,50q�6!   $��40(�64   $��Center=�6B   $��Center=�6_   
��� ���
Sudoku ETH5#      �� 5&   
   -   5        $��spMainButtons��5&      $��1��6*   $��Center=�68    � 	Resources 5'       �� 
TargetType   MaterialDesignFlatButton �� BasedOn	 �� Property
 �� Value��%,  '��       7   �� 5(      " ��6   8   .�5)      �� 	 � ��$	
 260��6.   �� 5*      	 � ��$
 46��6/   �� 5+      	 � ��$
 0,0,0,30q�6/   �� 5,      	 � ��$
 23(�61   �� 5-      	 � ��
 �#0077be63   �� 5.      	 � ��

 � ���63   -   5        50      51      .+�53      �� -   5         ��Name$ 
buPlayGame��53      $��	Play Game��6)   �� 54      -   5        $ buHall��54      $�� Hall of Fame��6%   �� 55      -   5        $ 
buSettings��55      $��Settings��6)   �� 56      -   5        $ buAbout��56      $��About Us��6&   �� 57      -   5        $ buExit��57      $
��Exit��6%   58       � 5:   
   -	   5          �Name$ contentFrame��5:        � NavigationUIVisibility$ Hidden=�6&   )  #MaterialDesignThemes.Wpf.DialogHost  5;   
   -
   5          Name$ 
RootDialog��5;   6   $��2��6$   5=      5?      5A      