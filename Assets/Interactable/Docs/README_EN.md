<div align="center">

# Interactable Framework

<p>
	<a href="https://github.com/starryforest-ymxk/Interactable/blob/main/LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue.svg" title="license-mit" /></a>
	<a href="https://github.com/starryforest-ymxk/Interactable/releases"><img src="https://img.shields.io/github/v/release/starryforest-ymxk/Interactable?color=green"/></a>
</p>
</div>


|[中文](README.md)|[English](README_EN.md)|

---

## 📖 Introduction

**Interactable** is a powerful 3D interaction framework inspired by Unity's UI Event System architecture, designed to bring UI-like event handling capabilities to 3D scenes.

### ✨ Core Features

- 🎯 **Familiar API Design** - Event interfaces consistent with Unity UGUI EventSystem
- 🔄 **Flexible System Architecture** - Support for multiple interaction systems coexisting and dynamic switching
- 🎮 **Multiple Input Support** - Mouse, touch, first-person, VR, and more
- 📡 **Powerful Raycast System** - Flexible raycasters with custom extension support
- 🎨 **Complete Event Support** - 17+ event interfaces covering all common interaction scenarios
- 🚀 **High Performance** - Object pooling, caching mechanisms, intelligent sorting
- 🔧 **Easy to Extend** - Modular design for effortless custom functionality

### 🎯 Use Cases

- 3D object interactions (clicking, dragging, hover highlighting)
- First-person/third-person game interaction systems
- VR/AR interaction implementation
- Strategy game object selection and manipulation
- Any 3D application requiring UI-like event systems

---

## 🎬 Quick Demo

```csharp
using Interactable;
using UnityEngine;

// Simple click interaction
public class ClickableObject : MonoBehaviour, IIAPointerClickHandler
{
    public void OnPointerClick(IAPointerData pointerData)
    {
        Debug.Log($"{gameObject.name} was clicked!");
    }
}

// Mouse hover highlight
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

## 📦 Installation

### Method 1: Download UnityPackage

1. Go to the [Releases](https://github.com/starryforest-ymxk/Interactable/releases) page
2. Download the latest `.unitypackage` file
3. Import in Unity: `Assets > Import Package > Custom Package...`

### Method 2: Clone Repository

```bash
git clone https://github.com/starryforest-ymxk/Interactable.git
```

Then copy the `Assets/Interactable` folder to your Unity project.

---

## 🚀 Quick Start

### Step 1: Create Interaction System

Create an empty GameObject in your scene named `InteractionSystem`:

1. Add `IASystem` component
2. Add an input module (e.g., `IAFirstPersonInputModule`)
3. Add a raycaster (e.g., `IACameraForwardRaycaster`)

```
GameObject: "InteractionSystem"
├─ IASystem
├─ IAFirstPersonInputModule
└─ IACameraForwardRaycaster
```

### Step 2: Configure Interaction System

Configure `IASystem` in the Inspector:

- ✅ **Active** - Check to activate the system
- **Drag Threshold** - Set drag threshold in pixels, default 10
- **Send Navigation Events** - Whether to send navigation events

### Step 3: Create Interactable Objects

On any 3D object you want to make interactable:

1. Ensure it has a `Collider` component (required)
2. Add a script implementing interaction interfaces

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
        Debug.Log("Pointer entered");
    }

    public void OnPointerExit(IAPointerData pointerData)
    {
        Debug.Log("Pointer exited");
    }

    public void OnPointerClick(IAPointerData pointerData)
    {
        Debug.Log("Object clicked");
    }
}
```

### Step 4: Run and Test

Press the Play button, move your mouse over the object, and click to see the interaction in action!

---

## 📚 Core Architecture

```
IAManager (Interaction Manager - Singleton)
    │
    ├─ Manages multiple IASystems
    ├─ Controls active system switching
    └─ Drives system processing
         │
         ▼
    IASystem (Interaction System)
         │
         ├─ IABaseInputModule (Input Module)
         │   ├─ IAPointerInputModule
         │   ├─ IAFirstPersonInputModule
         │   └─ IAFreeMouseInputModule
         │
         ├─ IABaseRaycaster (Raycaster)
         │   ├─ IACameraForwardRaycaster
         │   └─ IACursorRaycaster
         │
         └─ Selected GameObject
              └─ IIAHandler Interface Implementations
```

---

## 🎮 Supported Event Interfaces

### Pointer Events
- `IIAPointerEnterHandler` - Pointer enter
- `IIAPointerExitHandler` - Pointer exit
- `IIAPointerMoveHandler` - Pointer move
- `IIAPointerDownHandler` - Pointer down
- `IIAPointerUpHandler` - Pointer up
- `IIAPointerClickHandler` - Pointer click
- `IIAScrollHandler` - Scroll wheel

### Drag Events
- `IIAInitializePotentialDragHandler` - Initialize potential drag
- `IIABeginDragHandler` - Begin drag
- `IIADragHandler` - During drag
- `IIAEndDragHandler` - End drag
- `IIADropHandler` - Drop

### Selection Events
- `IIASelectHandler` - Object selected
- `IIADeselectHandler` - Object deselected
- `IIAUpdateSelectedHandler` - Update selected state

### Other Events
- `IIAMoveHandler` - Axis movement
- `IIASubmitHandler` - Submit
- `IIACancelHandler` - Cancel

---

## 📖 Documentation

- 📘 [Complete Manual](Interactable_Manual.md) - Detailed feature descriptions and API documentation
- 📗 [Project Overview](../../Overview/Project_Overview.md) - Framework architecture and technical details
- 💡 [Example Scenes](../Example) - Multiple practical examples included

---

## 💬 Support & Feedback

- 📖 Read the [Complete Documentation](Interactable_Manual.md)
- 💡 Check out [Example Scenes](../Example)
- ❓ Submit an [Issue](https://github.com/starryforest-ymxk/Interactable/issues)

