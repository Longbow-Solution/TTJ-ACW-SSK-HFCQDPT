   M S B A M L   `   `   `  ����
  ACWSSK!    ACWSSK.View.PaymentPageView   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      V RXamlAnimatedGif, Version=2.0.0.0, Culture=neutral, PublicKeyToken=20a987d8023d9690<gif2https://github.com/XamlAnimatedGif/XamlAnimatedGif  5      -   5        $	��755��5      $
��1280��6   $	��800��6(   $
��1280��68   .��5	      �   � RowDefinitions  5
   
   �� 5      $
��0.2* �6   �� 5      $
��auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $1��+/ACWSSK;component/Resources/Images/Logo.png��6   $	��450��6R   $��10q�6^   $��Center=�6j   $��Center=�6�   �� 5   
   $��1��6      ACWBlack#��[  6   $	��0.5q�6F   �� 5   
   -   5        $��
txtBarcode��5   �   $��2��6   $	��200��6   $��25��6+    �� AcceptsReturn . |�. ��67   �� *CardNo+ �� UpdateSourceTrigger$ PropertyChanged=�6�   � 5   
   $��2��6   $��Stretch=�6   
�������  5      �� 5      $
��0.5* �6    �� 5      $��* �6    .+�5      �� -   5        $��0��5      $��Right=�6%     H20#��[ 6A   $��0,0,20,0q�6�   ��6^   �� *TimeoutCountdown+ �� StringFormat$ 
{0:mm\:ss}��6�   �� 5      $��0��6    �� Orientation$ Vertical=�6&   $��Center=�6=   $��Center=�6Z   .+�5      ��   H50#��[ 6c   $
��Bold"�6�   #��[  6�   $��Center=�6�   $��Center=�6�   ��6   �� *TextInstructionA+$ PropertyChanged=�6�   �� 5      #��[ 6c   $��SemiBold"�6�   #��[  6�   $��Center=�6�   $��Center=�6�   ��6   �� *TextInstructionB+$ PropertyChanged=�6�   5       �� 5#      $��1��6   $��0q�6!    �� BitmapScalingMode$ HighQuality=�6,   '  !XamlAnimatedGif.AnimationBehavior   	AutoStart . $��Center=�6�   $	��350��6�      	SourceUri 6Z   �� *
VideoUri+$ PropertyChanged=�6�   �� 5%      $��1��6   $ Vertical=�6&   $��Stretch=�6=   $��Bottom=�6[   .+�5&      �� $��Right=�6     WhiteButton#��[ 65   $	��200��6|   $��70��6�   $��	0,0,50,20q�6�   ��6Z   �� *CancelCommand+.��5'      ��   H25#��[ 6c   $
��Wrap=�6�    �� TextAlignment$ Center=�6�   $��Center=�5(   $   $��Center=�6A   ���#707070$��SemiBold"�5)   9   ��5'       �� *Label_Cancel+$ PropertyChanged=�5)   9   5*      �� 5,      $ 
Horizontal=�6   $��Center=�66   y�6S   �� *DemoVisibility+$ PropertyChanged=�.+�5-      �� $��Bottom=�6   $��20��65   $��Success��6A   $��1��6v   $��0,0,0,0q�6�   ��6S   �� *DemoPayCommand+6�   �� 5.      $��20��6   $��Bottom=�6)   $��Failed��6A   $��0��6u   $��10,0,0,0q�6�   ��6R   �� *DemoPayCommand+6�   5/      50      51      5>      5?      