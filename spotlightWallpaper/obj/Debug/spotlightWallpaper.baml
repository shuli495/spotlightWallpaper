   M S B A M L   `   `   `  ����M  IspotlightWallpaper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:spotlightWallpaperspotlightWallpaper  5   	   #    spotlightWallpaper.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:spotlightWallpaper     5   	   -   5          � Title$  spotlightWallpaper��5   	   $��465.988��6$   $	��525��65    � 
ResizeMode$ CanMinimize=�6d    � Icon$ ico.ico��6}   .��5	      � .+�5
   
   �� -   5         ��Name$ setGb��5
      $��设置��6"   $
��Left=�6.   $	��333��6I   $��	10,10,0,0q�6V   $	��Top=�6i   $	��497��6�   �� 5   
   -   5        $ aboutGb��5      $��关于��6$   $
��Left=�60   $��10,348,0,10q�6K   $	��497��6`   �� 5   
   -   5         ��Name$ savePathLab��5      $��壁纸存放路径：��6%   $
��Left=�67   $��	25,39,0,0q�6R   $	��Top=�6e   $��25��6}   $��94��6�   �� 5   
   -   5         ��Name$ savePathBox��5      $
��Left=�6'   $��23��6B   $��
124,40,0,0q�6N    �� TextWrapping$ WrapWithOverflow=�6b   $	��Top=�6�   $	��374��6�    �� IsReadOnlyCaretVisible . 6�   �� 5   
   -   5        $ changeTimeLab1��5      $��更换壁纸频率：��6(   $
��Left=�6:   $��	25,71,0,0q�6U   $	��Top=�6h   $��25��6�   $��94��6�   �� 5   
   -   5        $ changeTimeBox��5      $
��Left=�6)   $��23��6D   $��
124,72,0,0q�6P   $ WrapWithOverflow=�6d   $	��Top=�6�   $��54��6�   $��24��6�   6�   �� 5   
   -   5        $ changeTimeLab��5      $��小时��6'   $
��Left=�64   $��
183,71,0,0q�6O   $	��Top=�6c    J� RenderTransformOrigin$ 1.945,0.641&�6{   $��25��6�   $��34��6�   �� 5   
   -	   5        	 ��Name$	 synBut��5      $��立即同步��6!   $
��Left=�60   $��
25,110,0,0q�6K   $	��Top=�6_   $��75��6w   $ -2.663,-0.108&�6�   $��19��6�   �� 5   
   -
   5        
 ��Name$
 runChk��5      $��开机启动��6#   $
��Left=�62   $��150,112,0,0q�6M   $	��Top=�6b   $ 0.574,0.052&�6z   $��15��6�   $��67��6�   �� 5   
   -   5        $ about1��5      $��spotlightWallpaper 1.0��6    $
��Left=�6A   $��
25,367,0,0q�6\   $	��Top=�6p   $
��Bold"�6�   $��25��6�   $	��148��6�   �� 5   
   -   5        $ about2��5      $a��[windows聚焦壁纸自动同步（6小时一次），自动切换；已开源(๑╹◡╹)��6    $
��Left=�6P   $��
25,392,0,0q�6k   $	��Top=�6   $��25��6�   $	��381��6�   �� 5   
   -   5        $��navigateTextBlock��5      $
��Wrap=�6-   $��183,372,9,-177q�6A   �� 5      -   5         �Name$ 	indexLink��5      .�6A   主页6E   5      �� 5   
   -   5         ��Name$ showLBox��5      $
��Left=�6$   $	��207��6?   $��
12,134,0,0q�6L   $	��Top=�6`   $	��493��6x   $��0,1,0,0q�6�   $��Disabled=�6�   ��5      �� 5      .R�5      A�  A� Columns$ 4��6"   $	��476��6.   5      5       5!      5"      