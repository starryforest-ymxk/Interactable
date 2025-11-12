# Interactable 框架 - 项目概览

## 项目基本信息

**项目名称**: Interactable  
**Unity 版本**: 2022.3  
**渲染管线**: Universal Render Pipeline (URP)  
**框架类型**: 3D 交互框架  
**设计理念**: 参考 Unity UI Event System 架构设计

---

## 框架简介

Interactable 是一个受 Unity UGUI 事件系统启发而开发的 3D 交互框架，旨在为 3D 场景提供类似 UI 事件系统的强大交互能力。该框架将成熟的 UI 事件处理机制迁移到 3D 空间，使开发者能够以熟悉的方式处理 3D 物体的交互逻辑。

### 核心价值

- **统一的交互范式**: 采用与 UGUI 一致的事件接口设计，降低学习成本
- **灵活的系统架构**: 支持多个交互系统共存并动态切换
- **强大的扩展性**: 模块化设计，易于自定义输入模块和射线投射器
- **完整的事件支持**: 涵盖指针、拖拽、选择等多种交互事件类型

### 适用场景

- 3D 环境中的物体交互（点击、拖拽、悬停高亮等）
- 第一人称/第三人称游戏的交互系统
- VR/AR 应用的交互实现
- 策略游戏、模拟经营类游戏的物体选择和操作
- 需要复杂交互逻辑的 3D 应用

---

## 技术架构

### 整体架构图

```
IAManager (交互管理器 - 单例)
    │
    ├─ 管理多个 IASystem 的注册/注销
    ├─ 控制活跃系统的切换
    └─ 驱动当前活跃系统的处理流程
         │
         ▼
    IASystem (交互系统)
         │
         ├─ IABaseInputModule (输入模块)
         │   ├─ IAPointerInputModule (指针输入)
         │   ├─ IAFirstPersonInputModule (第一人称输入)
         │   └─ IAFreeMouseInputModule (自由鼠标输入)
         │
         ├─ IABaseRaycaster (射线投射器)
         │   ├─ IACameraForwardRaycaster (相机前向射线)
         │   └─ IACursorRaycaster (光标射线)
         │
         └─ Selected GameObject (当前选中物体)
              │
              └─ IIAHandler 接口实现
                  ├─ 指针事件接口
                  ├─ 拖拽事件接口
                  └─ 选择事件接口
```

### 核心组件说明

#### 1. IAManager - 交互管理器

**职责**:
- 全局单例，管理整个框架的生命周期
- 维护所有交互系统的注册表
- 控制系统切换逻辑，确保同一时刻只有一个活跃系统
- 在 Update 中驱动当前活跃系统的处理流程

**关键特性**:
- 自动创建和 DontDestroyOnLoad
- 自动检测系统活跃状态并切换
- 提供手动系统切换接口

#### 2. IASystem - 交互系统

**职责**:
- 作为交互功能的核心容器
- 管理输入模块和射线投射器的生命周期
- 维护选中物体状态
- 协调事件分发流程

**核心功能**:
- 输入模块管理: 支持多个输入模块共存，自动激活优先级最高的模块
- 射线投射管理: 整合多个射线投射器的结果并排序
- 选择状态管理: 维护当前选中和上次选中的物体
- 事件调度: 将输入数据转换为事件并分发给目标物体

**可配置参数**:
- `Active`: 系统是否活跃
- `First Selected`: 系统激活时默认选中的物体
- `Send Navigation Events`: 是否发送导航事件
- `Drag Threshold`: 触发拖动的最小位移阈值（像素）
- `Keep Selection State`: 重新激活时是否保持上次的选择状态

#### 3. IABaseInputModule - 输入模块基类

**职责**:
- 处理各种输入源（鼠标、触摸、键盘等）
- 生成交互数据（IAPointerData、IAAxisData 等）
- 执行射线投射并获取命中结果
- 触发相应的事件接口

**内置实现**:
- `IAPointerInputModule`: 处理标准指针输入（鼠标/触摸）
- `IAFirstPersonInputModule`: 第一人称模式，相机中心点交互
- `IAFreeMouseInputModule`: 自由鼠标模式
- `IAInputSystemInputModule`: 基于新输入系统的实现（InputSystemSupport）

**事件处理流程**:
1. UpdateModule(): 每帧更新模块状态
2. Process(): 处理输入并生成交互数据
3. Raycast: 执行射线投射获取目标物体
4. Execute Events: 触发目标物体上的事件接口

#### 4. IABaseRaycaster - 射线投射器基类

**职责**:
- 定义射线投射逻辑
- 检测射线命中的物体
- 返回射线投射结果（IARaycastResult）

**内置实现**:
- `IACameraForwardRaycaster`: 从相机前向发射射线
- `IACursorRaycaster`: 从鼠标/触摸位置发射射线
- `TrackedDeviceRaycaster`: VR 设备射线投射（InputSystemSupport）

**排序机制**:
- 优先比较相机深度（depth）
- 其次比较射线投射器深度（depth）
- 最后比较射线碰撞距离（distance）

---

## 事件系统

### 事件接口总览

框架提供了 17 个事件接口，覆盖了 3D 交互的各种场景：

#### 指针事件（7个）

| 接口 | 触发时机 | 用途 |
|------|---------|------|
| `IIAPointerEnterHandler` | 指针进入物体 | 高亮显示、UI 提示 |
| `IIAPointerExitHandler` | 指针离开物体 | 取消高亮、隐藏 UI |
| `IIAPointerMoveHandler` | 指针在物体上移动 | 跟踪移动、更新 UI |
| `IIAPointerDownHandler` | 在物体上按下按钮 | 触觉反馈、视觉反馈 |
| `IIAPointerUpHandler` | 在物体上释放按钮 | 恢复状态、触发动作 |
| `IIAPointerClickHandler` | 点击物体（按下+释放） | 执行交互动作 |
| `IIAScrollHandler` | 滚轮滚动 | 缩放、滚动列表 |

#### 拖拽事件（4个）

| 接口 | 触发时机 | 用途 |
|------|---------|------|
| `IIAInitializePotentialDragHandler` | 检测到潜在拖拽 | 初始化拖拽状态 |
| `IIABeginDragHandler` | 开始拖拽（超过阈值） | 生成拖拽对象、改变外观 |
| `IIADragHandler` | 拖拽中每帧触发 | 更新拖拽物体位置 |
| `IIAEndDragHandler` | 拖拽结束 | 清理拖拽状态 |
| `IIADropHandler` | 物体被放置到此物体上 | 处理放置逻辑 |

#### 选择事件（3个）

| 接口 | 触发时机 | 用途 |
|------|---------|------|
| `IIASelectHandler` | 物体被选中 | 显示选中框、激活功能 |
| `IIADeselectHandler` | 物体取消选中 | 隐藏选中框、停用功能 |
| `IIAUpdateSelectedHandler` | 选中状态下每帧触发 | 更新选中状态 UI |

#### 其他事件（3个）

| 接口 | 触发时机 | 用途 |
|------|---------|------|
| `IIAMoveHandler` | 轴向移动输入 | 导航、键盘控制 |
| `IIASubmitHandler` | 提交操作 | 确认选择、执行动作 |
| `IIACancelHandler` | 取消操作 | 取消操作、返回 |

### 事件数据类

#### IAPointerData
包含指针交互的所有数据：
- 位置信息: `Position`, `Delta`, `PressPosition`
- 滚轮信息: `ScrollDelta`
- 目标物体: `PointerEnter`, `PointerPress`, `PointerDrag`, `PointerClick`
- 射线结果: `PointerCurrentRaycast`, `PointerPressRaycast`
- 点击信息: `ClickCount`, `ClickTime`, `EligibleForClick`
- 拖拽信息: `Dragging`, `UseDragThreshold`
- 按钮信息: `Button` (Left/Right/Middle)
- 压感信息: `Pressure`, `TangentialPressure`, `Tilt`, `Twist` 等

#### IAAxisData
包含轴向输入数据：
- `MoveVector`: 移动向量
- `MoveDir`: 移动方向（Up/Down/Left/Right）

#### IABaseData
所有事件数据的基类：
- `IASystem`: 所属的交互系统
- `Used`: 是否已被使用（用于事件传播控制）

---

## 目录结构

```
/Assets/Interactable
│
├── /Docs                              # 文档目录
│   └── Interactable_Manual.md        # 框架使用手册（完整文档）
│
├── /Scripts                           # 核心框架代码
│   ├── IAManager.cs                   # 交互管理器（单例）
│   ├── IASystem.cs                    # 交互系统核心类
│   ├── IAInterfaces.cs                # 所有事件接口定义
│   ├── ExecuteInteraction.cs          # 事件执行工具类
│   ├── IAUtilities.cs                 # 工具函数集合
│   ├── PointerId.cs                   # 指针 ID 定义
│   │
│   ├── /IAData                        # 事件数据类
│   │   ├── IABaseData.cs              # 基础事件数据
│   │   ├── IAPointerData.cs           # 指针事件数据
│   │   ├── IAAxisData.cs              # 轴向输入数据
│   │   └── IAMoveDirection.cs         # 移动方向枚举
│   │
│   ├── /InputModule                   # 输入模块
│   │   ├── IABaseInput.cs             # 输入包装类
│   │   ├── IABaseInputModule.cs       # 输入模块基类
│   │   ├── IAPointerInputModule.cs    # 指针输入模块
│   │   ├── IAFirstPersonInputModule.cs # 第一人称输入模块
│   │   └── IAFreeMouseInputModule.cs  # 自由鼠标输入模块
│   │
│   ├── /Raycast                       # 射线投射器
│   │   ├── IABaseRecaster.cs          # 射线投射器基类
│   │   ├── IACameraForwardRaycaster.cs # 相机前向射线投射器
│   │   ├── IACursorRaycaster.cs       # 光标射线投射器
│   │   ├── IARaycastResult.cs         # 射线投射结果结构
│   │   └── RaycastHitComparer.cs      # 射线结果排序器
│   │
│   └── /InputSystemSupport            # 新输入系统支持（可选）
│       ├── IAInputSystemInputModule.cs
│       ├── IAInputSystemFirstPersonInputModule.cs
│       ├── TrackedDeviceRaycaster.cs
│       ├── IAPointerModel.cs
│       ├── IANavigationModel.cs
│       └── /Utilities                 # 工具类
│
└── /Example                           # 示例场景
    ├── Example.unity                  # 示例场景
    ├── MyPlayer.prefab                # 第一人称玩家预制体
    │
    ├── /Scripts                       # 示例脚本
    │   ├── FPController.cs            # 第一人称控制器
    │   ├── InteractableObject.cs      # 可交互物体基类
    │   ├── IAOutline.cs               # 高亮轮廓效果实现
    │   └── InteractableDebug.cs       # 调试工具
    │
    ├── /Materials                     # 示例材质
    └── /Resources                     # 示例资源
        └── Outline.mat                # 轮廓材质
```

---

## 核心工作流程

### 1. 系统初始化流程

```
游戏启动
    ↓
IAManager 自动创建（单例）
    ↓
IASystem OnEnable → 注册到 IAManager
    ↓
IABaseInputModule OnEnable → 注册到 IASystem
    ↓
IABaseRaycaster OnEnable → 注册到 IASystem
    ↓
IAManager 选择第一个活跃的 IASystem
    ↓
IASystem 激活第一个支持的 InputModule
    ↓
系统就绪，开始处理交互
```

### 2. 每帧处理流程

```
IAManager.Update()
    ↓
检查并更新当前活跃系统（如需要则切换）
    ↓
调用 CurrentActiveSystem.SystemProcess()
    ↓
    ├─ TickInputModules() → 所有输入模块的 UpdateModule()
    ├─ CheckChangeInputModule() → 检查是否需要切换输入模块
    └─ ProcessInputModule() → 处理当前活跃的输入模块
         ↓
         CurrentInputModule.Process()
              ↓
              ├─ 读取输入状态（鼠标、键盘、触摸等）
              ├─ 创建/更新 IAPointerData
              ├─ 执行射线投射: IASystem.RaycastAll()
              │    ↓
              │    遍历所有 Raycaster 并收集结果
              │    ↓
              │    结果排序（相机深度 → 投射器深度 → 距离）
              ├─ 处理指针移动: HandlePointerMovement()
              │    ↓
              │    计算进入/退出的物体
              │    ↓
              │    触发 OnPointerExit 和 OnPointerEnter
              ├─ 处理按下/释放: ProcessMove/ProcessDrag
              └─ 触发相应事件接口
```

### 3. 事件触发流程

```
输入模块检测到交互
    ↓
执行射线投射获取目标物体
    ↓
ExecuteInteraction.Execute<T>(target, eventData, functor)
    ↓
    ├─ 获取目标物体上所有实现了接口 T 的组件
    ├─ 检查组件是否激活且启用
    └─ 调用 functor 执行事件回调
         ↓
         组件中实现的事件方法被调用
              ↓
              执行自定义交互逻辑
```

---

## 使用示例

### 基础设置

#### 1. 创建交互系统

在场景中创建一个 GameObject，命名为 `InteractionSystem`，添加以下组件：

```
GameObject: "InteractionSystem"
├─ IASystem
├─ IAFirstPersonInputModule (或其他输入模块)
└─ IACameraForwardRaycaster (或其他射线投射器)
```

#### 2. 配置 IASystem

在 Inspector 中配置：
- Active: ✓ (勾选)
- Drag Threshold: 10 (像素)
- Send Navigation Events: 根据需要

#### 3. 创建可交互物体

在需要交互的 3D 物体上添加：
- Collider 组件（必需，用于射线检测）
- 实现事件接口的脚本

### 代码示例

#### 示例 1: 简单点击交互

```csharp
using Interactable;
using UnityEngine;

public class ClickableObject : MonoBehaviour, IIAPointerClickHandler
{
    public void OnPointerClick(IAPointerData pointerData)
    {
        Debug.Log($"{gameObject.name} 被点击了!");
    }
}
```

#### 示例 2: 鼠标悬停高亮

```csharp
using Interactable;
using UnityEngine;

public class HighlightObject : MonoBehaviour, 
    IIAPointerEnterHandler, 
    IIAPointerExitHandler
{
    private Renderer objectRenderer;
    private Color originalColor;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
    }

    public void OnPointerEnter(IAPointerData pointerData)
    {
        objectRenderer.material.color = Color.yellow;
    }

    public void OnPointerExit(IAPointerData pointerData)
    {
        objectRenderer.material.color = originalColor;
    }
}
```

#### 示例 3: 拖拽物体

```csharp
using Interactable;
using UnityEngine;

public class DraggableObject : MonoBehaviour,
    IIABeginDragHandler,
    IIADragHandler,
    IIAEndDragHandler
{
    private Vector3 offset;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(IAPointerData pointerData)
    {
        Vector3 worldPos = GetWorldPosition(pointerData);
        offset = transform.position - worldPos;
    }

    public void OnDrag(IAPointerData pointerData)
    {
        Vector3 worldPos = GetWorldPosition(pointerData);
        transform.position = worldPos + offset;
    }

    public void OnEndDrag(IAPointerData pointerData)
    {
        Debug.Log("拖拽结束");
    }

    private Vector3 GetWorldPosition(IAPointerData pointerData)
    {
        Ray ray = mainCamera.ScreenPointToRay(pointerData.Position);
        Plane plane = new Plane(Vector3.up, transform.position);
        
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        return transform.position;
    }
}
```

#### 示例 4: 完整的可交互物体（参考框架示例）

```csharp
using Interactable;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour,
    IIAPointerEnterHandler,
    IIAPointerExitHandler,
    IIAPointerDownHandler,
    IIAPointerUpHandler,
    IIAPointerClickHandler
{
    [SerializeField] private Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objectRenderer;
    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        originalMaterial = objectRenderer.material;
    }

    public void OnPointerEnter(IAPointerData pointerData)
    {
        isHovered = true;
        UpdateVisual();
    }

    public void OnPointerExit(IAPointerData pointerData)
    {
        isHovered = false;
        UpdateVisual();
    }

    public void OnPointerDown(IAPointerData pointerData)
    {
        if (pointerData.Button == IAPointerData.InputButton.Left)
        {
            isPressed = true;
            UpdateVisual();
        }
    }

    public void OnPointerUp(IAPointerData pointerData)
    {
        if (pointerData.Button == IAPointerData.InputButton.Left)
        {
            isPressed = false;
            UpdateVisual();
        }
    }

    public void OnPointerClick(IAPointerData pointerData)
    {
        Debug.Log($"{gameObject.name} 被点击!");
        OnInteract();
    }

    private void UpdateVisual()
    {
        if (isPressed)
        {
            transform.localScale = Vector3.one * 0.9f;
        }
        else if (isHovered)
        {
            transform.localScale = Vector3.one * 1.1f;
            objectRenderer.material = highlightMaterial;
        }
        else
        {
            transform.localScale = Vector3.one;
            objectRenderer.material = originalMaterial;
        }
    }

    protected virtual void OnInteract()
    {
        // 子类重写此方法实现具体交互逻辑
    }
}
```

---

## 高级特性

### 1. 多系统共存与切换

场景中可以存在多个 `IASystem`，通过设置 `Active` 属性或调用 `SetActive()` 方法来控制哪个系统处于活跃状态。

**应用场景**:
- 游戏状态切换（正常游戏 ↔ 菜单界面）
- 控制模式切换（第一人称 ↔ 第三人称）
- 多玩家分屏（每个玩家有独立的交互系统）

**示例**:
```csharp
public class GameStateManager : MonoBehaviour
{
    [SerializeField] private IASystem gameplaySystem;
    [SerializeField] private IASystem menuSystem;

    public void EnterMenu()
    {
        gameplaySystem.SetActive(false);
        menuSystem.SetActive(true);
    }

    public void ExitMenu()
    {
        menuSystem.SetActive(false);
        gameplaySystem.SetActive(true);
    }
}
```

### 2. 自定义输入模块

通过继承 `IABaseInputModule` 可以实现自定义的输入处理逻辑。

**示例**:
```csharp
public class CustomInputModule : IABaseInputModule
{
    public override void Process()
    {
        // 实现自定义的输入处理逻辑
    }
}
```

### 3. 自定义射线投射器

通过继承 `IABaseRaycaster` 可以实现自定义的射线投射逻辑。

**应用场景**:
- 特殊形状的射线（如锥形、扇形检测区域）
- 基于物理层级的过滤
- VR 手柄射线

**示例**:
```csharp
public class CustomRaycaster : IABaseRaycaster
{
    [SerializeField] private Camera rayCamera;
    [SerializeField] private LayerMask raycastMask;
    [SerializeField] private float maxDistance = 100f;

    public override Camera RayCamera => rayCamera;

    public override void Raycast(IAPointerData pointerData, List<IARaycastResult> resultAppendList)
    {
        Ray ray = rayCamera.ScreenPointToRay(pointerData.Position);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastMask))
        {
            var result = new IARaycastResult
            {
                gameObject = hit.collider.gameObject,
                raycaster = this,
                distance = hit.distance,
                worldPosition = hit.point,
                worldNormal = hit.normal
            };
            
            resultAppendList.Add(result);
        }
    }
}
```

### 4. 事件冒泡机制

框架支持事件沿着 GameObject 层级向上传播（类似 UGUI）。通过 `sendPointerHoverToParent` 参数控制是否将指针悬停事件传递给父物体。

### 5. 新输入系统支持

框架提供了对 Unity 新输入系统（Input System Package）的支持，位于 `/Scripts/InputSystemSupport` 目录。

**包含的模块**:
- `IAInputSystemInputModule`: 基于新输入系统的标准输入模块
- `IAInputSystemFirstPersonInputModule`: 基于新输入系统的第一人称输入模块
- `TrackedDeviceRaycaster`: VR 追踪设备射线投射器

---

## 技术亮点

### 1. 架构设计

- **单一职责原则**: 每个类都有明确的职责划分
- **开闭原则**: 通过继承扩展功能，无需修改核心代码
- **依赖倒置**: 依赖抽象而非具体实现
- **模块化设计**: 输入模块、射线投射器、事件处理器相互独立

### 2. 性能优化

- **对象池模式**: 使用 `UnityEngine.Pool` 减少 GC 压力
- **缓存机制**: 缓存组件引用和计算结果
- **延迟初始化**: 按需创建对象
- **智能排序**: 高效的射线结果排序算法

### 3. 灵活的事件系统

- **接口驱动**: 基于接口的事件系统，松耦合
- **类型安全**: 编译时类型检查，避免运行时错误
- **完整的事件类型**: 覆盖所有常见交互场景
- **事件数据传递**: 丰富的事件数据类，包含所有必要信息

### 4. 扩展性

- **自定义输入模块**: 支持任意输入设备和输入方式
- **自定义射线投射**: 支持各种射线检测需求
- **自定义事件接口**: 可以扩展新的事件类型
- **多系统支持**: 场景中可以有多个独立的交互系统

---

## 与 Unity UGUI EventSystem 的对比

| 特性 | UGUI EventSystem | Interactable 框架 |
|------|-----------------|------------------|
| **应用场景** | 2D UI 交互 | 3D 场景交互 |
| **射线检测** | GraphicRaycaster | IABaseRaycaster（3D 物理射线） |
| **事件接口** | IPointerHandler 等 | IIAHandler 等（命名一致） |
| **输入模块** | StandaloneInputModule | IAPointerInputModule 等 |
| **系统管理** | EventSystem（单例） | IAManager + IASystem（多系统） |
| **目标对象** | UI 元素 | 带 Collider 的 3D 物体 |
| **事件冒泡** | UI 层级 | GameObject 层级 |
| **扩展性** | 需修改源码 | 完全模块化，易扩展 |

**设计思路一致性**:
- 相同的事件接口命名规范
- 相同的输入处理流程
- 相同的事件分发机制
- 相同的模块化架构思想

---

## 开发建议

### 1. 最佳实践

- **合理设置拖拽阈值**: 根据目标设备调整 `Drag Threshold`，移动端建议更大的值
- **优化射线检测**: 使用 Layer Mask 限制射线检测范围
- **避免过多事件**: 不需要的事件接口不要实现，减少不必要的回调
- **使用事件数据**: 充分利用 `IAPointerData` 中的信息，避免重复计算
- **合理使用多系统**: 只在确实需要独立交互逻辑时才创建多个系统

### 2. 性能优化建议

- 减少每帧射线投射的数量
- 使用简化的碰撞体（Box/Sphere Collider）
- 合理设置射线检测的最大距离
- 避免在事件回调中进行复杂计算
- 使用对象池管理频繁创建/销毁的对象

### 3. 调试技巧

- 使用 `InteractableDebug.cs` 查看系统状态
- 在事件回调中添加 Debug.Log 追踪事件流
- 使用 Scene 视图的 Gizmos 显示射线
- 检查 IAManager 的 CurrentActiveSystem
- 验证物体是否有 Collider 组件

### 4. 常见问题

**问题**: 物体没有响应交互  
**解决**:
- 确保物体有 Collider 组件
- 确保 IASystem 处于活跃状态
- 确保有正确的射线投射器
- 检查 Layer 设置和射线 Mask

**问题**: 拖拽不触发  
**解决**:
- 检查 `Drag Threshold` 设置是否过大
- 确保实现了 `IIABeginDragHandler` 和 `IIADragHandler`
- 检查 `UseDragThreshold` 是否为 true

**问题**: 事件触发顺序不对  
**解决**:
- 检查射线投射器的优先级设置
- 理解射线结果的排序规则
- 考虑使用事件的 `Used` 属性阻止传播

---

## 未来扩展方向

### 可能的增强功能

1. **更多输入支持**
   - 游戏手柄输入模块
   - VR 手柄输入模块
   - AR 手势识别模块

2. **高级射线投射**
   - 曲线射线（抛物线）
   - 多重射线（霰弹枪式）
   - 区域检测（球形、锥形）

3. **性能优化**
   - 空间划分加速结构
   - 异步射线投射
   - LOD 系统集成

4. **工具链增强**
   - 可视化调试工具
   - 编辑器扩展
   - 交互配置工具

5. **功能扩展**
   - 多点触控支持
   - 手势识别系统
   - 力反馈支持
   - 可访问性功能

---

## 总结

Interactable 框架是一个设计优雅、功能完整的 3D 交互解决方案。它成功地将 Unity UGUI 的成熟设计理念应用到 3D 场景中，为开发者提供了：

- **熟悉的 API**: 与 UGUI 一致的接口设计
- **强大的功能**: 完整的交互事件支持
- **灵活的架构**: 易于扩展和定制
- **优秀的性能**: 经过优化的事件处理流程

无论是开发第一人称游戏、策略游戏，还是 VR/AR 应用，该框架都能提供可靠的交互基础。通过合理使用框架提供的各种功能，开发者可以快速实现丰富的 3D 交互体验。

---

**文档版本**: 1.0  
**最后更新**: 2024  
**相关文档**: 请参阅 `/Assets/Interactable/Docs/Interactable_Manual.md` 获取详细的使用手册
