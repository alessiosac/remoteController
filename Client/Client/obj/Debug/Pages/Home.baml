   M S B A M L   `   `   `  ����A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Client.PagesClient.Pages  5          Client.Pages.Home   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ,localclr-namespace:Client.Pages     5      .��5	      �   � ColumnDefinitions  5
   
   �� 5      $��1* �6   �� 5      $��1* �6    � RowDefinitions 5   
   �� 5      $��1* �6   $	��140��6(   �� 5      $��3* �6   .+�5   
   �� -   5        $��label1��5   �   $$��SELECT  THE  HOST  TO  CONNECT��6   $��26(�69   $
��Bold"�6G   $��78��6Y   $��Center=�6e   $��Center=�6�   $	��800��6�   $��Trebuchet MS)�6�   $��2��6�   $��Center=�6�   $��Center=�6	  �� 5   
   $��Center=�6   $	��110��62   $��0q�6?   $��1��6J   $��Center=�6W   $	��400��6r   $��1��6~   .+�5      �� -   5         ��Name$ Connect_Button��5      $��Connect to server��6-   $��20(�6I   $
��Bold"�6W   $��Center=�6i   $	��250��6�   $��35��6�   $��Trebuchet MS)�6�   $��Center=�6�    �� 	IsDefault . 6�   �� 5      -   5        $ Return_Button��5      $
��Back��6,   $��20(�6;   $
��Bold"�6I   $	��250��6[   $��35��6g   $��Trebuchet MS)�6�   $��Center=�6�   $��0,20,0,0q�6�   5      �� 5   
   $��Center=�6   $	��110��62   $��0q�6?   $��1��6J   $��Center=�6W   $	��400��6r   .+�5      �� -   5         ��Name$ textBox��5      $��35��6'    �� TextWrapping$
 Wrap=�63   $��IP address or Hostname��6G   $��Center=�6e   $	��250��6�   $��Center=�6�    �� TextAlignment$ Center=�6�   $��Center=�6�   
�������$��Trebuchet MS)�6�   6*  �� 5      -   5         ��Name$ error��5   I   $��20(�6   $��59��6"   $
��Left=�6.   $	��Top=�6X   $	��400��6p   $��Center=�6|   $��Trebuchet MS)�6�   $��Center=�6�   $��0,10,0,0q�6�   $y�Hidden=�6�   .��5      �� -   5         ��Name$ textBoxError��5      $��50��62   $
��Wrap=�6>   5      5      ��5   
   �� 5      	 �� EndPoint$	 0.5,1&�6"   
 �� 
StartPoint$
 0.5,0&�63   .��5      �  � Color$ 	#FF97BEBD��6    � Offset$ 1Q�61   � 5       $ White��6   $ 0Q�6-   5!      5#      5$      