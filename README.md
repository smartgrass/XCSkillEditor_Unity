# XCSkillEditor 小草技能编辑器
基于Flux扩展开发的UnityAct技能编辑器, 集成Mirror支持局域网联机,实现只通过配置就可以制作技能

主要功能：技能编辑器，战斗框架，联网同步

附加功能：对象池，计时器，敌人AI，编辑器收藏夹

Unity版本建议2020.3以上

编译报错是dll文件丢失,目前已修复

目前项目的代码和 Mirror高度绑定,耦合性比较高,不推荐移植...

## 声明
项目内的美术素材仅供演示使用, 禁止用于商业相关.

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/maomao.jpg" width= "400"/>

*** 

## 思路介绍

1.编辑器保存技能：将技能序列化，技能分解为动画，位移，生成对象和子技能等事件，提取关键序列化信息，保存在预制体中。

2.技能释放原理：根据技能id读取技能事件序列，然后根据事件的start帧触发事件，直到所有事件完成，技能便执行结束。不同类型的技能事件在各自的类中实现。

3.同步思路：帧同步，区分本地与联网行为， 将联网行为通过Rpc模式通知客户端，客户端再从本地执行表示效果。

4.位置同步：为了避免闪现卡顿，使用位移偏移量计算位置， 并最终与实际位置做插值做平滑。

5.技能同步：技能释放前，将玩家位置和方向记录进技能信息，一并发送给服务端，再由服务端转发给客户端，客户端本地处理表现。


## 展示

### 本github版本只包含技能编辑器核心部分,和视频的效果demo不同

在SkillEditor.scene只保留了几个示例技能

效果demo展示: https://www.bilibili.com/video/BV18m4y1675a

pc试玩demo: https://share.weiyun.com/ik4O15hD)


## 插件引用

### [Flux](https://assetstore.unity.com/packages/tools/animation/flux-18440) ,[Mirror](https://github.com/MirrorNetworking/Mirror), [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes) , [XiaoCaoTool](https://github.com/smartgrass/XiaoCaoTools) ,[QianMo/X-PostProcessing-Library](https://github.com/QianMo/X-PostProcessing-Library) ,Luban,DoTween


[Mirror](https://github.com/MirrorNetworking/Mirror): 网络框架 ,优点是不用写服务器代码, 缺点只适用于小型项目,如果只想单机逻辑改起来有难度。

[NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes): 是一个轻量级的编辑器扩展,强烈推荐,也推荐它的作者的开源项目

[XiaoCaoTool](https://github.com/smartgrass/XiaoCaoTools): 受NaughtyAttributes启发,制作的XiaoCaoWindow编辑器扩展，快速搭建编辑器窗口，也包含部分开发小工具

其他优秀框架推荐：YooAseet ,[猫刀刀的MDDSkillEngine](https://gitee.com/mtdmt/MDDSkillEngine?_from=gitee_search) ,[烟雨的et7](https://github.com/wqaetly/ET/tree/et7_fgui_yooasset_luban_huatuo) ,[MotionFramework](https://github.com/gmhevinci/MotionFramework)

# 开始使用

## 1.打开

打开SkillEditor.scene

然后打开Flux:菜单Window/Flux/Open Editor

在Flux窗口选择一个技能就可以编辑了

编辑完成,就可以保存了. 

    保存:右上角的保存按钮 或者 在Scene里选中Sequence 右键/Xiaocao/保存选中Seq

*** 

# 默认设定

## 1.Sequence配置:

(1)一个角色有两个Animator, 是为了防止Flux编辑时乱连, 在Editor场景中使用editor

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/man_editor.png"/>

(2)技能id和额外的配置

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/SeqConfig.png" width= "900"/>

## 2.技能特效都得是预制体,并且放于Resources目录下

一般放于Resources/SkillEffet下,当然你也可以修改成别的加载方式

加载代码可以看RunTimePoolManager的LoadResPoolObj()

 

## 3.技能坐标系设定


分玩家坐标系和世界坐标

    玩家坐标系(默认): 生成物体时,以玩家的相对坐标系生成物体

    世界坐标: 技能触发时刻的玩家坐标系,不受之后玩家移动的影响

对于玩家坐标系的物体,需要放在PlayerConstraint下

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/struct.png"/>

更改坐标系:

菜单Window/Flux/Open Inspector

然后选中一个Timeline

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/TransfromType.png" width= "900"/>

## 4.常用的Aseet位置

Resources/Charecter  角色

Resources/ResUsing 配置

    其中SkiillKeyCodeSo是配置技能按键和图标和cd,没配置按键的技能则为被动技能
  
  
## 5.编辑器菜单功能

技能的excel配置(Luban): 菜单Tools/配置 

     
    使用的Luban,所以应该需要安装.net 4.0
      
收藏夹:菜单Tools/XiaoCao/收藏夹

动画预览窗口:菜单Tools/XiaoCao/动画预览窗口


