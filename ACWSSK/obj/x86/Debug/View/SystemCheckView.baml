   M S B A M L   `   `   `  ����
  ACWSSK!    ACWSSK.View.SystemCheckView   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Hi:http://schemas.microsoft.com/expression/2010/interactivity     5      -   5          >�Name$  ucSystemCheck��5      $	��755��5	      $
��1280��6   $	��800��6(   $
��1280��68    � 	Resources 5
         
resdBorder �� 
TargetType �� Property �� Value  resdComponentName��%x  &	        &	 �     �� 5   
   " ��6#   .�5      ��  �@��Padding$ 	8,15,5,15q�6(   6<   �� 5       �@��CornerRadius$ 2��6-   69   5      �� 5   
   " ��6*   .�5      ��  � ��$ 20(�6)   66   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ Center=�62   6C   �� 5       � ��$ 8,5,8,5q�6'   69   5      5      .��5      ]� .+�5   
   �� $	��Top=�6    �� CornerRadius$ 3,3,0,0��6&    �� Padding$ 8q�6=   ��5      �� 5       �� 
StartPoint$ 0.5,0&�6&    �� EndPoint$ 0.5,1&�69   .��5      � 	 � Color$	 #407F7F��6#   
 � Offset$
 0Q�63   6@   � 5      $	 #0D4D4D��6#   $
 1Q�63   6@   5      ._�5       ]� ��. .+�5&      �� $��System Checking��6    �� Padding$ 8,0,0,3q�63   ���#FFFFFF$��20(�6Z   $��Center=�6h   6�   5'      5(      ]� 5*   
   $	��Top=�6   ��. .+�5/      �� $
��Auto=�6   .��50      �� $	��Top=�6   ��64   �� *
ItemList+��51      �� 52      .R�53      �� 6+   54      ��56      �� 57      .R�58      �� .+�59   "   �� #��[  6P   ��6)   �� *BackgroundColor+ J� InputBindings 5:   &   d� 5;   *    d� Gesture$ 	LeftClicka�67    �� Command 6K   �� *%#DataContext.ToggleStatusDescCommand+ �� ElementName$ ucSystemCheck�� �� CommandParameter 6�   �� *
ItemCode+6�   ._�5=   &   ��  �� Orientation$ 
Horizontal=�61   .+�5>   *   �� $��40��6M    �� BitmapScalingMode$ HighQuality=�6X   $��Center=�6�   ��60   �� *	CheckIcon+6�   �� 5?   *   #��[ 6N   ��64   �� *
ItemName+6{   �� 5@   *   #��[ 6L   ��64   �� *Status+6y   5A   '   5B   #   �� 5C   "   y�6)   �� *StatusDescVisibility+._�5D   &   �� $��0q�6X   ���#F0EEEA � DataContext 6.   �� *StatusDescription+.@�5E   *   �� .��5F   .   �� $ 5q�6I   $��45,5,5,5q�6U   $��12(�6g   ��68   �� 6w   5G   +   5H   '   5I   #   5J      5K      5M      5N      5O      5P      5Q      