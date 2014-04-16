LayerManager for NGUI 3.X
===================================

原理
-----------------------------------
```
每AddLayer 会将新加进来的层置为最上层
每一层有1000的深度范围
例如0层就是 0-999 1层就是1000-1999
一般来说够用了, 如果不够用修改下脚本中的Layer_Distance的值.
```
**注意: 层不具备Remove操作, 在游戏开始时进行AddLayer即可. InsertLayer一般用不到**


相关操作
-----------------------------------

#####对层的操作:
```
AddLayer        =>	添加
InsertLayer	    =>	插入层 
```
#####对Panel的操作:
```
AddPanel		=>  添加Panel到指定层
RemovePanel		=>	从指定层中移除指定的Panel
InsertPanel		=>	插入Panel到指定层
```

#####以下面开头的函数是进行排序的
```
MoveUp*         => 将指定层(panel) 上移
MoveDown*       => 将指定层(panel) 下移
MoveTop*        => 将指定层(panel) 移动顶部
MoveBottom*     => 将指定层(panel) 移到底部
```



