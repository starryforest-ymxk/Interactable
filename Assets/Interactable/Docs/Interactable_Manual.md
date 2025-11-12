# Interactable 框架使用手册



- [框架概述](#框架概述)

- [核心架构](#核心架构)

- [快速开始](#快速开始)

- [核心组件详解](#核心组件详解)

- [事件接口说明](#事件接口说明)

- [输入模块](#输入模块)

- [射线投射器](#射线投射器)

- [高级用法](#高级用法)

- [示例详解](#示例详解)

---

## 框架概述

**Interactable** 是一个参考 Unity UI 事件系统架构设计的 3D 交互框架，用于在 3D 场景中实现丰富的交互功能。

### 主要特性
- 类似 UGUI 的事件系统设计
- 支持多种输入方式（鼠标、触摸、第一人称）
- 灵活的射线投射系统
- 完整的指针事件支持（Enter、Exit、Down、Up、Click、Drag 等）
- 支持多个交互系统并存和切换
- 易于扩展的模块化架构

### 适用场景
- 3D 物体交互（点击、拖拽、悬停）
- 第一人称游戏中的物品拾取和交互
- VR/AR 交互系统
- 任何需要类似 UI 事件系统的 3D 场景

---

## 核心架构

框架采用分层架构设计，主要由以下几个部分组成：

```
IAManager (交互管理器)
    ├── IASystem (交互系统)
    │   ├── IABaseInputModule (输入模块)
    │   │   ├── IAPointerInputModule
    │   │   ├── IAFirstPersonInputModule
    │   │   └── IAFreeMouseInputModule
    │   └── IABaseRaycaster (射线投射器)
    │       ├── IACameraForwardRaycaster
    │       └── IACursorRaycaster
    └── GameObject (交互对象)
        └── IIAHandler (事件接口实现)
```

### 核心类说明

#### IAManager
- **单例模式**的交互管理器
- 管理所有交互系统的注册和注销
- 负责活跃系统的切换
- 每帧调用当前活跃系统的处理方法

#### IASystem
- 交互系统，一个场景可以有多个
- 管理输入模块和射线投射器
- 维护当前选中的 GameObject
- 处理事件分发

#### IABaseInputModule
- 输入模块基类
- 负责处理输入并生成交互数据
- 执行射线投射和事件触发

#### IABaseRaycaster
- 射线投射器基类
- 负责检测射线命中的物体
- 支持自定义投射逻辑

---

## 快速开始

### 最小化设置

#### 1. 创建交互系统

在场景中创建一个 GameObject，添加以下组件：

```
GameObject "InteractionSystem"
├── IASystem
├── IAFirstPersonInputModule (或其他输入模块)
└── IACameraForwardRaycaster (或其他射线投射器)
```

#### 2. 创建可交互物体

在需要交互的 3D 物体上：
1. 确保有 Collider 组件
2. 添加实现了交互接口的脚本

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
        Debug.Log("鼠标进入物体");
    }

    public void OnPointerExit(IAPointerData pointerData)
    {
        Debug.Log("鼠标离开物体");
    }

    public void OnPointerClick(IAPointerData pointerData)
    {
        Debug.Log("点击物体");
    }
}
```

#### 3. 运行测试

运行场景，将鼠标移动到可交互物体上，即可触发相应事件。

---

## 核心组件详解

### IAManager - 交互管理器

`IAManager` 是全局单例，自动管理所有交互系统。

#### 主要功能

```csharp
public static IAManager GetInstance()
```
获取管理器实例（会自动创建）。

```csharp
public IASystem CurrentActiveSystem { get; }
```
获取当前活跃的交互系统。

```csharp
public void SwitchIASystem(IASystem target)
```
手动切换活跃的交互系统。

#### 系统自动切换规则

- 始终只有一个活跃系统
- 当前活跃系统变为非活跃时，自动切换到下一个活跃系统
- 通过 `IASystem.IsActive()` 判断系统是否应该活跃

---

### IASystem - 交互系统

交互系统是框架的核心，管理输入和射线投射。

#### Inspector 参数

| 参数 | 说明 |
|------|------|
| Active | 系统是否活跃 |
| First Selected | 系统激活时默认选中的物体 |
| Send Navigation Events | 是否发送导航事件（类似 UGUI） |
| Drag Threshold | 触发拖动事件的最小像素位移 |
| Keep Selection State | 重新激活时是否恢复上次选中状态 |

#### 主要方法

```csharp
public bool IsActive()
```
判断系统是否活跃。

```csharp
public void SetActive(bool value)
```
设置系统活跃状态。

```csharp
public GameObject CurrentSelected { get; }
```
获取当前选中的物体。

```csharp
public void SetSelectedGameObject(GameObject selected)
```
设置选中的物体（会触发 Select/Deselect 事件）。

```csharp
public void AddRaycaster(IABaseRaycaster raycaster)
public void RemoveRaycaster(IABaseRaycaster raycaster)
```
添加或移除射线投射器。

#### 使用示例

```csharp
IASystem system = GetComponent<IASystem>();

system.SetActive(true);

system.SetSelectedGameObject(myGameObject);

GameObject currentSelected = system.CurrentSelected;
```

---

## 事件接口说明

框架提供了丰富的事件接口，类似 Unity UI 的事件系统。

### 指针事件

#### IIAPointerEnterHandler
```csharp
void OnPointerEnter(IAPointerData pointerData)
```
指针（鼠标/触摸）进入物体时触发。

#### IIAPointerExitHandler
```csharp
void OnPointerExit(IAPointerData pointerData)
```
指针离开物体时触发。

#### IIAPointerMoveHandler
```csharp
void OnPointerMove(IAPointerData pointerData)
```
指针在物体上移动时触发。

#### IIAPointerDownHandler
```csharp
void OnPointerDown(IAPointerData pointerData)
```
在物体上按下按钮时触发。

#### IIAPointerUpHandler
```csharp
void OnPointerUp(IAPointerData pointerData)
```
在物体上释放按钮时触发。

#### IIAPointerClickHandler
```csharp
void OnPointerClick(IAPointerData pointerData)
```
点击物体时触发（按下和释放都在同一物体上）。

### 拖拽事件

#### IIAInitializePotentialDragHandler
```csharp
void OnInitializePotentialDrag(IAPointerData pointerData)
```
检测到潜在拖拽时触发（按下时）。

#### IIABeginDragHandler
```csharp
void OnBeginDrag(IAPointerData pointerData)
```
开始拖拽时触发（移动超过阈值）。

#### IIADragHandler
```csharp
void OnDrag(IAPointerData pointerData)
```
拖拽过程中每帧触发。

#### IIAEndDragHandler
```csharp
void OnEndDrag(IAPointerData pointerData)
```
拖拽结束时触发。

#### IIADropHandler
```csharp
void OnDrop(IAPointerData pointerData)
```
被拖拽物体放置到此物体上时触发。

### 选择事件

#### IIASelectHandler
```csharp
void OnSelect(IABaseData baseData)
```
物体被选中时触发。

#### IIADeselectHandler
```csharp
void OnDeselect(IABaseData baseData)
```
物体取消选中时触发。

#### IIAUpdateSelectedHandler
```csharp
void OnUpdateSelected(IABaseData baseData)
```
选中状态下每帧触发。

### 其他事件

#### IIAScrollHandler
```csharp
void OnScroll(IAPointerData pointerData)
```
鼠标滚轮滚动时触发。

#### IIAMoveHandler
```csharp
void OnMove(IAAxisData axisData)
```
轴向移动输入时触发。

#### IIASubmitHandler
```csharp
void OnSubmit(IABaseData baseData)
```
提交操作时触发。

#### IIACancelHandler
```csharp
void OnCancel(IABaseData baseData)
```
取消操作时触发。

### 事件数据类

#### IAPointerData
包含指针相关的所有数据：

```csharp
public class IAPointerData
{
    public Vector2 Position;              // 当前位置
    public Vector2 Delta;                 // 位置增量
    public Vector2 PressPosition;         // 按下时的位置
    public Vector2 ScrollDelta;           // 滚轮增量
    
    public GameObject PointerEnter;       // 当前进入的物体
    public GameObject PointerPress;       // 按下的物体
    public GameObject PointerDrag;        // 拖拽的物体
    public GameObject PointerClick;       // 点击的物体
    
    public IARaycastResult PointerCurrentRaycast;  // 当前射线结果
    public IARaycastResult PointerPressRaycast;    // 按下时的射线结果
    
    public InputButton Button;            // 鼠标按键
    public bool Dragging;                 // 是否正在拖拽
    public int ClickCount;                // 点击次数
    
    // ... 更多属性
}
```

#### IAAxisData
包含轴向输入数据：

```csharp
public class IAAxisData
{
    public Vector2 MoveVector;            // 移动向量
    public IAMoveDirection MoveDir;       // 移动方向
}
```

---

## 输入模块

输入模块负责处理输入并生成交互事件。

### IAPointerInputModule

基础指针输入模块，支持鼠标和触摸输入。

**特点：**
- 支持鼠标三键（左、右、中）
- 支持触摸输入
- 光标可见，可自由移动

**适用场景：** 策略游戏、模拟经营类游戏等需要自由光标的场景。

### IAFirstPersonInputModule

第一人称输入模块，专为 FPS 类游戏设计。

**特点：**
- 激活时自动锁定并隐藏光标
- 射线从屏幕中心发出
- 支持鼠标和触摸输入
- 拖拽阈值基于世界空间距离而非屏幕像素

**Inspector 参数：**
- `Horizontal Axis`: 水平轴名称（默认 "Horizontal"）
- `Vertical Axis`: 垂直轴名称（默认 "Vertical"）
- `Double Click Time`: 双击时间间隔
- `Drag Threshold Multi`: 拖拽阈值乘数

**适用场景：** FPS、TPS 等第一人称/第三人称游戏。

### IAFreeMouseInputModule

自由鼠标输入模块，光标不锁定。

**特点：**
- 光标自由移动
- 基于光标位置的射线投射

**适用场景：** RTS、MOBA 等需要自由光标的游戏。

### 自定义输入模块

继承 `IABaseInputModule` 或 `IAPointerInputModule` 创建自定义输入模块：

```csharp
using Interactable;
using UnityEngine;

public class MyInputModule : IAPointerInputModule
{
    public override void Process()
    {
        // 实现输入处理逻辑
    }

    public override bool ShouldActivateModule()
    {
        // 返回是否应该激活此模块
        return true;
    }
}
```

---

## 射线投射器

射线投射器负责检测射线命中的物体。

### IACameraForwardRaycaster

相机前向射线投射器，从相机位置沿前方发射射线。

**Inspector 参数：**
- `Ray Mask`: 射线检测的层级遮罩
- `Max Ray Intersections`: 最大射线交点数量（0 表示无限制）
- `Max Ray Length`: 最大射线长度（0 表示使用相机远裁剪面）
- `Allow Trigger Colliders`: 是否检测触发器碰撞体

**适用场景：** 第一人称游戏、VR 应用。

**示例：**
```csharp
GameObject systemObj = new GameObject("InteractionSystem");
var system = systemObj.AddComponent<IASystem>();
var raycaster = systemObj.AddComponent<IACameraForwardRaycaster>();

raycaster.RayMask = LayerMask.GetMask("Interactable");
raycaster.MaxRayLength = 10f;
raycaster.AllowTriggerColliders = false;
```

### IACursorRaycaster

光标射线投射器，从相机位置沿屏幕光标方向发射射线。

**适用场景：** RTS、策略游戏、任何需要光标交互的场景。

### 自定义射线投射器

继承 `IABaseRaycaster` 创建自定义射线投射器：

```csharp
using Interactable;
using System.Collections.Generic;
using UnityEngine;

public class MyRaycaster : IABaseRaycaster
{
    public override Camera RayCamera => Camera.main;

    public override void Raycast(IAPointerData pointerData, List<IARaycastResult> resultAppendList)
    {
        // 实现射线投射逻辑
        // 将结果添加到 resultAppendList
    }
}
```

### 射线结果排序

框架会自动对射线结果进行排序，排序规则：
1. 相机深度（depth）优先
2. 射线投射器的深度（depth）
3. 碰撞距离（distance）

---

## 高级用法

### 多交互系统管理

一个场景可以有多个交互系统，适用于不同的交互模式。

#### 示例：UI 和 3D 场景交互系统

```csharp
// UI 交互系统
GameObject uiSystem = new GameObject("UIInteractionSystem");
var uiIASystem = uiSystem.AddComponent<IASystem>();
var uiInputModule = uiSystem.AddComponent<IAPointerInputModule>();
var uiRaycaster = uiCanvas.AddComponent<IACursorRaycaster>();

// 3D 场景交互系统
GameObject sceneSystem = new GameObject("SceneInteractionSystem");
var sceneIASystem = sceneSystem.AddComponent<IASystem>();
var sceneInputModule = sceneSystem.AddComponent<IAFirstPersonInputModule>();
var sceneRaycaster = sceneSystem.AddComponent<IACameraForwardRaycaster>();

// 根据游戏状态切换系统
void EnterUIMode()
{
    uiIASystem.SetActive(true);
    sceneIASystem.SetActive(false);
}

void EnterGameMode()
{
    uiIASystem.SetActive(false);
    sceneIASystem.SetActive(true);
}
```

### 事件冒泡

事件默认会沿父级向上传播，直到找到可以处理的对象。

```csharp
// 使用 ExecuteHierarchy 执行层级事件
GameObject handler = ExecuteInteraction.ExecuteHierarchy<IIAPointerClickHandler>(
    targetObject, 
    pointerData, 
    ExecuteInteraction.PointerClickHandler
);
```

### 控制事件传播

可以通过 `IABaseInputModule.SendPointerHoverToParent` 属性控制 Enter/Exit 事件是否向父级传播。

```csharp
inputModule.SendPointerHoverToParent = false; // 不向父级传播
```

### 选中状态管理

```csharp
IASystem system = GetComponent<IASystem>();

// 设置选中物体
system.SetSelectedGameObject(myObject);

// 获取当前选中物体
GameObject selected = system.CurrentSelected;

// 获取上一次选中物体
GameObject lastSelected = system.LastSelected;
```

### 拖拽阈值调整

```csharp
IASystem system = GetComponent<IASystem>();

// 设置拖拽阈值（像素）
system.PixelDragThreshold = 5;
```

### 编程式触发事件

```csharp
using Interactable;

// 手动触发点击事件
IAPointerData pointerData = new IAPointerData(iaSystem);
ExecuteInteraction.Execute(
    targetObject, 
    pointerData, 
    ExecuteInteraction.PointerClickHandler
);
```

---

## 示例详解

### InteractableObject - 基础交互对象

`InteractableObject` 是一个抽象基类，演示了如何创建基础的可交互物体。

```csharp
public abstract class InteractableObject :
    MonoBehaviour,
    IIAPointerEnterHandler,
    IIAPointerExitHandler,
    IIAPointerDownHandler,
    IIAPointerUpHandler
{
    protected bool pointerOnObject;
    protected bool pointerDown;

    public virtual void OnPointerEnter(IAPointerData pointerData)
    {
        pointerOnObject = true;
        OnInteractionAvailable(true);
    }

    public virtual void OnPointerExit(IAPointerData pointerData)
    {
        pointerOnObject = false;
        if (!pointerDown)
            OnInteractionAvailable(false);
    }

    public virtual void OnPointerDown(IAPointerData pointerData)
    {
        if (pointerData.Button != IAPointerData.InputButton.Left)
            return;

        pointerDown = true;
        OnInteractionAvailable(false);
    }

    public virtual void OnPointerUp(IAPointerData pointerData)
    {
        if (pointerData.Button != IAPointerData.InputButton.Left)
            return;

        pointerDown = false;
        if (pointerOnObject)
            OnInteractionAvailable(true);
    }

    protected abstract void OnInteractionAvailable(bool available);
}
```

**关键点：**
- 跟踪指针是否在物体上
- 跟踪按钮是否按下
- 提供抽象方法 `OnInteractionAvailable` 供子类实现

### IAOutline - 带轮廓高亮的交互对象

`IAOutline` 继承自 `InteractableObject`，添加了物体轮廓高亮效果。

```csharp
public class IAOutline : InteractableObject
{
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 0.02f;
    
    private Material outlineMaterial;

    protected override void OnInteractionAvailable(bool available)
    {
        if (available)
        {
            outlineMaterial.EnableKeyword("ENABLE_OUTLINE");
        }
        else
        {
            outlineMaterial.DisableKeyword("ENABLE_OUTLINE");
        }
    }
}
```

**使用方法：**
1. 将 `IAOutline` 添加到需要交互的物体上
2. 确保物体有 Collider 组件
3. 鼠标悬停时会显示轮廓高亮

### InteractableDebug - 调试工具

`InteractableDebug` 实现了所有事件接口，用于调试和学习。

```csharp
public class InteractableDebug : MonoBehaviour, 
    IIAPointerEnterHandler,
    IIAPointerExitHandler,
    IIAPointerDownHandler,
    IIAPointerUpHandler,
    IIAPointerClickHandler,
    // ... 其他接口
{
    [SerializeField] private bool debug;

    public void OnPointerEnter(IAPointerData pointerData)
    {
        if (!debug) return;
        Debug.Log("On Pointer Enter");
    }
    
    // ... 其他事件实现
}
```

**使用方法：**
1. 添加 `InteractableDebug` 到任何物体上
2. 勾选 `Debug` 选项
3. 运行游戏，在 Console 中查看事件触发日志

### 示例场景设置

查看 `Assets/Plugins/Interactable/Example/Example.unity` 场景了解完整示例：

**场景结构：**
```
Example Scene
├── Player (FPController)
│   ├── IASystem
│   ├── IAFirstPersonInputModule
│   └── IACameraForwardRaycaster
├── InteractableMgr (IAManager)
├── InteractableObject1 (IAOutline)
├── InteractableObject2 (IAOutline)
└── InteractableObject3 (InteractableDebug)
```

---

## 常见问题

### Q: 为什么事件没有触发？

**检查清单：**
1. 物体是否有 Collider 组件？
2. Collider 是否在射线投射器的 Layer Mask 中？
3. IASystem 是否激活？
4. 输入模块是否正确配置？
5. 相机设置是否正确？

### Q: 如何实现物体拾取？

```csharp
public class PickupItem : MonoBehaviour, IIAPointerClickHandler
{
    public void OnPointerClick(IAPointerData pointerData)
    {
        // 拾取逻辑
        Inventory.AddItem(this);
        Destroy(gameObject);
    }
}
```

### Q: 如何实现拖拽物体？

```csharp
public class DraggableObject : MonoBehaviour, 
    IIABeginDragHandler, 
    IIADragHandler, 
    IIAEndDragHandler
{
    private Vector3 offset;

    public void OnBeginDrag(IAPointerData pointerData)
    {
        offset = transform.position - pointerData.PointerCurrentRaycast.worldPosition;
    }

    public void OnDrag(IAPointerData pointerData)
    {
        transform.position = pointerData.PointerCurrentRaycast.worldPosition + offset;
    }

    public void OnEndDrag(IAPointerData pointerData)
    {
        // 拖拽结束处理
    }
}
```

### Q: 如何区分左右键点击？

```csharp
public void OnPointerClick(IAPointerData pointerData)
{
    switch (pointerData.Button)
    {
        case IAPointerData.InputButton.Left:
            Debug.Log("左键点击");
            break;
        case IAPointerData.InputButton.Right:
            Debug.Log("右键点击");
            break;
        case IAPointerData.InputButton.Middle:
            Debug.Log("中键点击");
            break;
    }
}
```

### Q: 如何实现双击？

```csharp
public void OnPointerClick(IAPointerData pointerData)
{
    if (pointerData.ClickCount == 2)
    {
        Debug.Log("双击");
    }
}
```

---

## 性能优化建议

1. **合理使用射线投射层级遮罩**
   ```csharp
   raycaster.RayMask = LayerMask.GetMask("Interactable");
   ```

2. **限制射线投射距离**
   ```csharp
   raycaster.MaxRayLength = 10f;
   ```

3. **减少不必要的事件处理**
   - 只实现需要的接口
   - 在事件处理中避免复杂计算

4. **使用对象池管理频繁创建的对象**

5. **避免在 OnPointerMove 等高频事件中执行重操作**

---

## 扩展建议

### 1. 自定义射线投射器

可以创建特殊的射线投射器，例如：
- 球形投射
- 矩形区域投射
- 多射线投射

### 2. 增强事件数据

可以扩展 `IAPointerData` 或创建自定义数据类：

```csharp
public class ExtendedIAPointerData : IAPointerData
{
    public float HoldTime { get; set; }
    public bool IsLongPress { get; set; }
    
    public ExtendedIAPointerData(IASystem system) : base(system)
    {
    }
}
```

### 3. 添加新的输入源

继承 `IABaseInputModule` 支持新的输入设备：
- 手柄
- VR 控制器
- 手势识别

---

## 总结

Interactable 框架提供了一套完整、灵活的 3D 交互解决方案。通过本手册，您应该能够：

- 理解框架的核心架构
- 快速搭建交互系统
- 实现各种交互功能
- 扩展框架以满足项目需求

更多示例和最新更新，请参考 Example 文件夹中的示例场景和脚本。
