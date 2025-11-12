<div align="center">

# Interactable Framework

<p>
	<a href="https://github.com/starryforest-ymxk/Interactable/blob/main/LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue.svg" title="license-mit" /></a>
	<a href="https://github.com/starryforest-ymxk/Interactable/releases"><img src="https://img.shields.io/github/v/release/starryforest-ymxk/Interactable?color=green"/></a>
</p>
</div>

|[中文](./Assets/Interactable/Docs/README.md)|[English](./Assets/Interactable/Docs/README_EN.md)|

---

## 📖 简介

**Interactable** 是一个参考 Unity UI 事件系统架构设计的 3D 交互框架，旨在为 3D 场景提供类似 UI 事件系统的强大交互能力。

### ✨ 核心特性

- 🎯 **熟悉的 API 设计** - 与 Unity UGUI EventSystem 一致的事件接口
- 🔄 **灵活的系统架构** - 支持多个交互系统共存并动态切换
- 🎮 **多种输入支持** - 鼠标、触摸、第一人称、VR 等多种输入方式
- 📡 **强大的射线系统** - 灵活的射线投射器，支持自定义扩展
- 🎨 **完整的事件支持** - 17+ 事件接口，覆盖所有常见交互场景
- 🚀 **高性能优化** - 对象池、缓存机制、智能排序
- 🔧 **易于扩展** - 模块化设计，轻松实现自定义功能

### 🎯 适用场景

- 3D 物体交互（点击、拖拽、悬停高亮）
- 第一人称/第三人称游戏交互系统
- VR/AR 交互实现
- 策略游戏物体选择和操作
- 任何需要类似 UI 事件系统的 3D 应用

---

## 🎬 快速演示

```csharp
using Interactable;
using UnityEngine;

// 简单的点击交互
public class ClickableObject : MonoBehaviour, IIAPointerClickHandler
{
    public void OnPointerClick(IAPointerData pointerData)
    {
        Debug.Log($"{gameObject.name} 被点击了!");
    }
}

// 鼠标悬停高亮
public class HighlightObject : MonoBehaviour, 
    IIAPointerEnterHandler, 
    IIAPointerExitHandler
{
    public void OnPointerEnter(IAPointerData pointerData)
    {
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void OnPointerExit(IAPointerData pointerData)
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
}
```

---

## 📦 安装

### 方式 1: 下载 UnityPackage

1. 前往 [Releases](https://github.com/starryforest-ymxk/Interactable/releases) 页面
2. 下载最新版本的 `.unitypackage` 文件
3. 在 Unity 中导入：`Assets > Import Package > Custom Package...`

### 方式 2: 克隆仓库

```bash
git clone https://github.com/starryforest-ymxk/Interactable.git
```

然后将 `Assets/Interactable` 文件夹复制到你的 Unity 项目中。

---

## 🚀 快速开始

### 第一步：创建交互系统

在场景中创建一个空的 GameObject，命名为 `InteractionSystem`：

1. 添加 `IASystem` 组件
2. 添加输入模块（例如 `IAFirstPersonInputModule`）
3. 添加射线投射器（例如 `IACameraForwardRaycaster`）

```
GameObject: "InteractionSystem"
├─ IASystem
├─ IAFirstPersonInputModule
└─ IACameraForwardRaycaster
```

### 第二步：配置交互系统

在 Inspector 中配置 `IASystem`：

- ✅ **Active** - 勾选以激活系统
- **Drag Threshold** - 设置拖拽阈值（像素），默认 10
- **Send Navigation Events** - 是否发送导航事件

### 第三步：创建可交互物体

在任何需要交互的 3D 物体上：

1. 确保有 `Collider` 组件（必需）
2. 添加实现交互接口的脚本

```csharp
using Interactable;
using UnityEngine;

public class MyInteractable : MonoBehaviour, 
    IIAPointerEnterHandler, 
    IIAPointerExitHandler,
    IIAPointerClickHandler
{
    public void OnPointerEnter(IAPointerData pointerData)
    {
        Debug.Log("鼠标进入");
    }

    public void OnPointerExit(IAPointerData pointerData)
    {
        Debug.Log("鼠标离开");
    }

    public void OnPointerClick(IAPointerData pointerData)
    {
        Debug.Log("物体被点击");
    }
}
```

### 第四步：运行测试

按下 Play 按钮，移动鼠标到物体上，点击即可看到交互效果！

---

## 📚 核心架构

```
IAManager (交互管理器 - 单例)
    │
    ├─ 管理多个 IASystem
    ├─ 控制活跃系统切换
    └─ 驱动系统处理流程
         │
         ▼
    IASystem (交互系统)
         │
         ├─ IABaseInputModule (输入模块)
         │   ├─ IAPointerInputModule
         │   ├─ IAFirstPersonInputModule
         │   └─ IAFreeMouseInputModule
         │
         ├─ IABaseRaycaster (射线投射器)
         │   ├─ IACameraForwardRaycaster
         │   └─ IACursorRaycaster
         │
         └─ Selected GameObject (选中物体)
              └─ IIAHandler 接口实现
```

---

## 🎮 支持的事件接口

### 指针事件
- `IIAPointerEnterHandler` - 指针进入
- `IIAPointerExitHandler` - 指针离开
- `IIAPointerMoveHandler` - 指针移动
- `IIAPointerDownHandler` - 指针按下
- `IIAPointerUpHandler` - 指针抬起
- `IIAPointerClickHandler` - 指针点击
- `IIAScrollHandler` - 滚轮滚动

### 拖拽事件
- `IIAInitializePotentialDragHandler` - 初始化潜在拖拽
- `IIABeginDragHandler` - 开始拖拽
- `IIADragHandler` - 拖拽中
- `IIAEndDragHandler` - 结束拖拽
- `IIADropHandler` - 放置

### 选择事件
- `IIASelectHandler` - 物体被选中
- `IIADeselectHandler` - 物体取消选中
- `IIAUpdateSelectedHandler` - 更新选中状态

### 其他事件
- `IIAMoveHandler` - 轴向移动
- `IIASubmitHandler` - 提交
- `IIACancelHandler` - 取消

---

## 📖 文档

- 📘 [完整使用手册](./Assets/Interactable/Docs/Interactable_Manual.md) - 详细的功能说明和 API 文档
- 📗 [项目概览](./Assets/Overview/Project_Overview.md) - 框架架构和技术详解
- 💡 [示例场景](./Assets/Interactable/Example) - 包含多个实用示例

---

## 💬 支持与反馈

- 📖 查阅 [完整文档](./Assets/Interactable/Docs/Interactable_Manual.md)
- 💡 查看 [示例场景](./Assets/Interactable/Example)
- ❓ 提交 [Issue](https://github.com/starryforest-ymxk/Interactable/issues)
