# XCSkillEditor 小草技能编辑器

基于Flux扩展开发的UnityAct技能编辑器, 集成Mirror支持局域网联机

#### 核心功能

* 技能编辑器
* 战斗框架
* 联网同步

#### 次要功能

* 对象池 , 计时器 , 敌人AI, Luban表格配置


## 声明

项目内的美术素材仅供演示使用, 禁止用于商业相关.


本项目**Mirror**的高度绑定,联网代码比较麻烦

为此拆分出单机版本：[XCSkillEditor_Local](https://github.com/smartgrass/XCSkillEditor_Local) 

Unity版本建议2020.3以上


## 思路介绍

* 技能序列化 : 核心思路就是拆解和序列化，将技能拆为动画，位移，生成对象和子技能等事件，提取序列化信息，保存在预制体中。
* 技能释放 ：读取技能数据反序列化，得到技能事件队列，每帧检测事件队列，事件从start帧开始触发, 直到所有事件完成，技能便执行结束。 [知乎专栏](https://zhuanlan.zhihu.com/p/452470415)
* 同步思路：
    * 帧同步: 区分本地与联网行为，将联网行为通过Rpc模式通知客户端，客户端在本地模拟表示效果。
    * 位置同步：为了避免闪现卡顿，使用位移偏移量计算位置，并最终与实际位置做插值做平滑。
    * 技能同步：只需要提取释放前的位置和方向和技能id，将数据发送给服务端，再由服务端转发给客户端



## 细节和表现

想要技能连招丝滑，打击感，则必须要需要很多参数，利用luban表格配置

如破甲值，击飞时间，击飞高度&水平距离，顿帧时间，以及击中特效/音效等，每段攻击都有区别。

* 击退击飞思路：利用dotween动画制作 [b站专栏]（https://www.bilibili.com/read/cv17846274/?from=readlist）
* 敌人霸体：敌人存在霸体值，受击减少霸体值，小于0是敌人动作会被打断和击飞
* 敌人ai: 抽象出敌人行为。配置成行为池,和抽卡一般,把行为按照概率随机抽空。敌人ai循环是：寻找敌人->选择行为->追踪->攻击->攻击后摇->重新选择行为



## 开发效率取舍

技能编辑器是为开发效率和更好的技能效果, 但编辑器需要完善细节太多,开发上要做取舍。

比如碰撞检测,就直接使用unity的碰撞的了,目前并无明显问题

资源加载用Resource梭哈了，编辑器界面用Flux改造 ，用现成的框架能省下不少时间

****


## 插件引用

引用  |  |
--------- | --------|
[Mirror](https://github.com/MirrorNetworking/Mirror)  | 网络框架 ,优点是不用写服务器代码, 缺点只适用于小型项目。 |
[NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes)  | 轻量级的编辑器扩展,也推荐其作者的其他框架  |
[XiaoCaoTool](https://github.com/smartgrass/XiaoCaoTools)  | 受NaughtyAttributes启发,制作的XiaoCaoWindow编辑器扩展  |
[Flux](https://assetstore.unity.com/packages/tools/animation/flux-18440) | 很好的timeline编辑器工具, 用这个改造成技能编辑器  |


其他 |    | 
--------- | --------- |
[X-PostProcessing](https://github.com/QianMo/X-PostProcessing-Library) |  Luban |


框架推荐 |    |    |    |
--------- | --------- | --------- |--------- |
[YooAseet](https://www.yooasset.com/) | [MDDSkillEngine](https://gitee.com/mtdmt/MDDSkillEngine?_from=gitee_search) | [烟雨et7](https://github.com/wqaetly/ET/tree/et7_fgui_yooasset_luban_huatuo) |[TEngine](https://github.com/ALEXTANGXIAO/TEngine) |




## Demo展示

github公开版本只有技能编辑器核心功能，完整技能没公开，

* demo视频： https://www.bilibili.com/video/BV18m4y1675a

* demo试玩包: https://share.weiyun.com/ik4O15hD

****


# 使用说明

## 1.打开

打开SkillEditor.scene

然后打开Flux:菜单Window/Flux/Open Editor

打开Flux监视器：菜单Window/Flux/Open Inspector

## 2.保存

右上角的保存按钮 

或者 在Hierarchy中选中Sequence, 右键/Xiaocao/保存选中Seq

****

## 编辑器设定

### 1.Sequence配置:

* 一个角色有两个Animator, 是为了防止编辑器模式下Flux乱连, 在Editor场景中使用editor

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/man_editor.png"/>

* 技能id和额外的配置

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/SeqConfig.png" width= "900"/>



### 2.技能特效都得是预制体,并且放于Resources目录下

一般放于Resources/SkillEffet下,当然你也可以修改成别的加载方式

加载代码可以看RunTimePoolManager的LoadResPoolObj()


### 3.技能坐标系设定

* 玩家坐标系(默认): 生成物体时,以玩家的相对坐标系生成物体
* 世界坐标: 技能触发时刻的玩家坐标系,不受之后玩家移动的影响

对于玩家坐标系的物体,需要放在PlayerConstraint下

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/struct.png"/>

* 更改坐标系:

然后选中一个Timeline，在Flux监视器中修改

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/TransfromType.png" width= "900"/>

### 4.常用的Aseet位置

* Resources/Charecter  角色

* Resources/ResUsing 配置
  
  
### 5.编辑器菜单功能

* 技能的excel配置(Luban): 菜单Tools/配置 (需要有.net 4.0)
      
* 收藏夹:菜单Tools/XiaoCao/收藏夹

* 动画预览窗口:菜单Tools/XiaoCao/动画预览窗口


<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/maomao.jpg" width= "400"/>


