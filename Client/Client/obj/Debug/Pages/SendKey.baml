   M S B A M L   `   `   `  ����A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Client.PagesClient.Pages  5          Client.Pages.SendKey   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ,localclr-namespace:Client.Pages     5      .��5	      �   � RowDefinitions  5
   
   �� 5      $
��184* �6   �� 5      $
��217* �6   �� 5      $
��199* �6    � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
   �� $��2��6   $��Center=�6)   $��Center=�6D   .+�5      �� -   5        $��label1��5   v   $��Center=�6   $��18(�68   $
��Bold"�6F   $��Stretch=�6X   $��0��6�   $��2��6�   $��0��6�   $��Trebuchet MS)�6�   $��Center=�6�   $��30��6�   $��0q�6�   .��5      �� -   5         ��Name$ title��5      $��WordEllipsis=�6+   $J��DPlease, enter here the commands to send to server for application : ��6G   $
��Wrap=�6�   5      �� 5      -   5        $��label2��5   v   $��Center=�6   $��18(�68   $
��Bold"�6F   $��Stretch=�6X   $��0��6�   $��2��6�   $��0��6�   $��Trebuchet MS)�6�   $��Center=�6�   $��50��6�   $��0,5,0,0q�6�   $��Italic$�6  .��5      �� -   5        $ nameApp��5      $��WordEllipsis=�6-   $�� ��6I   $
��Wrap=�6Q   5      5      �� 5   
   -   5         ��Name$ 
CheckPanel��5      $��Stretch=�6)   $��59��6G   $��0,12,0,0q�6S   $��1��6e   $	��Top=�6r   $��2��6�   $��0��6�   $y�Hidden=�6�   .+�5      �� -   5         ��Name$ checkForAllServer��5      $��Center=�62   $��Center=�6M   $��Center=�6o   .��5      �� $7��1Send to all the server with this open application��6   $��Center=�6U   $��19(�6p   $��Trebuchet MS)�6~   5      5      �� 5    
   -   5        $ panelForKey��5       $��Center=�6*   $��61��6G   $��0,30,0,0q�6S   $��1��6e   $��Center=�6r   $��2��6�   $��0��6�   .+�5!      �� $��Center=�6   $��35��66   $��Center=�6B   $	��555��6]    �� Orientation$ 
Horizontal=�6i    +� FocusedElement 6�   ��  �� ElementName$ commandsField��.+�5"      �� $��
Commands :��6   $��Center=�6.   $��0,0,20,0q�6I   $��Trebuchet MS)�6[   $��14(�6u   �� 5#      -   5         ��Name$ commandsField��5#   6   $
��Left=�6   $��Center=�6M   $	��310��6h   $��26��6t   $��Trebuchet MS)�6�   $��Center=�6�   6�   �� 5$      -	   5        	 ��Name$	 buttonClear��5$   Q   $��26��6   $��14(�6&   $��Center=�64   $��Center=�6f   $��80��6�   $��Trebuchet MS)�6�   $��0��6�   $��2��6�   $��	Clear All��6�   $��30,0,0,0q�6�   5%      �� 5&      -
   5        
 ��Name$
 messageError��5&      $��Error��6*   $��Center=�6:   $��Trebuchet MS)�6^   $y�	Collapsed=�6x   5'      �� 5(   
   -   5        $	 button2��5(   H   $��44��6   $��15(�6   $��Center=�6+   $��Center=�6Y   $	��180��6t   $��Trebuchet MS)�6�   $��0��6�   $��2��6�   $��110,0q�6�   .��5)      �� $ 
Horizontal=�6   $��Stretch=�62   .+�5*      �� $��20��6   $��20��6#   $��5,0,0,0q�6/   $��Center=�6@   $*��$/Client;component/images/Go-back.ico��6[   �� 5+      $
��Back��6   $��15,0,0,0q�6'   $��Center=�69   $��Trebuchet MS)�6T   $��15(�6n   5,      5-      �� 5.   
   -   5        $	 button1��5.   H   $��44��6   $��15(�6   $��Center=�6+   $��Center=�6Y   $	��180��6t   $��Trebuchet MS)�6�   $��1��6�   $��2��6�   $��110,0q�6�   .��5/      �� $ 
Horizontal=�6   $��Stretch=�62   .+�50      �� $��20��6   $��20��6#   $��5,0,0,0q�6/   $��Center=�6@   $'��!/Client;component/images/send.png��6[   �� 51      $��Send to server��6   $��15,0,0,0q�61   $��Center=�6C   $��Trebuchet MS)�6^   $��15(�6x   52      53      ��54   
   �� 55       �� EndPoint$ 0.5,1&�6"    �� 
StartPoint$ 0.5,0&�63   .��56      �  � Color$ 	#FF97BEBD��6    � Offset$ 1Q�61   � 57      $ White��6   $ 0Q�6-   58      5:      5;      