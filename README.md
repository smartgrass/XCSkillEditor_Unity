# XCSkillEditor 小草技能编辑器
# 简介
基于Flux扩展开发的UnityAct技能编辑器, 集成Mirror支持局域网联机,实现只通过配置就可以制作技能

## 声明
项目内的美术素材仅供演示使用, 禁止用于商业相关.

## 一些话

目前项目的代码和 Mirror高度绑定,耦合性比较高,不推荐移植...

由于只想做战斗系统,框架上就做得比较随意了。等有时间想学习下其他人的框架,完善下网络系统。

别人的优秀框架:YooAseet ,[猫刀刀的MDDSkillEngine](https://gitee.com/mtdmt/MDDSkillEngine?_from=gitee_search) ,[烟雨的et7](https://github.com/wqaetly/ET/tree/et7_fgui_yooasset_luban_huatuo) ,[MotionFramework](https://github.com/gmhevinci/MotionFramework)

对项目有什么建议的话,欢迎留言

# 引用

### [Flux](https://assetstore.unity.com/packages/tools/animation/flux-18440) ,[Mirror](https://github.com/MirrorNetworking/Mirror), [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes) ,
### [XiaoCaoTool](https://github.com/smartgrass/XiaoCaoTools) ,[QianMo/X-PostProcessing-Library](https://github.com/QianMo/X-PostProcessing-Library) ,Luban,DoTween

### 说明:

[Mirror](https://github.com/MirrorNetworking/Mirror): 网络框架 ,优点是不用写服务器代码, 缺点只适用于小型项目,如果只想单机逻辑改起来有难度。

[NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes): 是一个轻量级的编辑器扩展,强烈推荐,也推荐它的作者的开源项目

[XiaoCaoTool](https://github.com/smartgrass/XiaoCaoTools): 受NaughtyAttributes启发,制作的XiaoCaoWindow编辑器扩展，快速搭建编辑器窗口，也包含部分开发小工具


# 展示

本github版本只包含技能编辑器核心部分,和效果demo不同!

效果demo展示:

pc试玩demo: https://share.weiyun.com/ik4O15hD)

# 使用

打开SkillEditor.scene

然后打开Flux:菜单Window/Flux/Open Editor

在Flux窗口选择一个技能就可以编辑了

编辑完成,就可以保存了. 

    保存:右上角的保存按钮 或者 在Scene里选中Sequence 右键/Xiaocao/保存选中Seq


# 一些默认设定

### 1.Sequence配置:

(1)一个角色有两个Animator, 是为了防止Flux编辑时乱连, 在Editor场景中使用editor

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/man_editor.png"/>

(2)技能id和额外的配置

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/SeqConfig.png" width= "900"/>

### 2.技能特效都得是预制体,并且放于Resources目录下

一般放于Resources/SkillEffet下,当然你也可以修改成别的加载方式

加载代码可以看RunTimePoolManager的LoadResPoolObj()


### 3.技能坐标系设定

分玩家坐标系和世界坐标

    玩家坐标系(默认): 生成物体时,以玩家的相对坐标系生成物体

    世界坐标: 技能触发时刻的玩家坐标系,不受之后玩家移动的影响

对于玩家坐标系的物体,需要放在PlayerConstraint下

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/struct.png"/>

更改坐标系:

菜单Window/Flux/Open Inspector

然后选中一个Timeline

<img src="https://github.com/smartgrass/ReadMeImgs/blob/main/SkillEditor/TransfromType.png" width= "900"/>

### 4.常用的Aseet位置

Resources/Charecter  角色

Resources/ResUsing 配置

    其中SkiillKeyCodeSo是配置技能按键和图标和cd,没配置按键的技能则为被动技能
  
  
### 5.编辑器菜单功能

技能的excel配置(Luban): 菜单Tools/配置 

收藏夹:菜单Tools/XiaoCao/收藏夹

动画预览窗口:菜单Tools/XiaoCao/动画预览窗口




