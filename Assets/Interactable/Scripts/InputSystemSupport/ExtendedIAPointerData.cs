#if ENABLE_INPUT_SYSTEM
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

namespace Interactable.InputSystemSupport
{

    public class ExtendedIAPointerData : IAPointerData
    {
        public ExtendedIAPointerData(IASystem system)
            : base(system)
        {
        }

        /// <summary>
        /// The <see cref="InputControl"/> that generated the pointer input.
        /// The device associated with this control should be the same as this event's device.
        /// </summary>
        /// <seealso cref="Device"/>
        public InputControl Control { get; set; }

        /// <summary>
        /// The <see cref="InputDevice"/> that generated the pointer input.
        /// </summary>
        /// <seealso cref="Pointer"/>
        /// <seealso cref="Touchscreen"/>
        /// <seealso cref="Mouse"/>
        /// <seealso cref="Pen"/>
        public InputDevice Device { get; set; }
        
        public int TouchId { get; set; }

        /// <summary>
        /// Type of pointer that generated the input.
        /// </summary>
        public UIPointerType PointerType { get; set; }

        public int UiToolkitPointerId { get; set; }

        /// <summary>
        /// For <see cref="UIPointerType.Tracked"/> type pointer input, this is the world-space position of
        /// the <see cref="TrackedDevice"/>.
        /// </summary>
        public Vector3 TrackedDevicePosition { get; set; }

        /// <summary>
        /// For <see cref="UIPointerType.Tracked"/> type pointer input, this is the world-space orientation of
        /// the <see cref="TrackedDevice"/>.
        /// </summary>
        public Quaternion TrackedDeviceOrientation { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.ToString());
            stringBuilder.AppendLine("button: " + Button); // Defined in PointerEventData but PointerEventData.ToString() does not include it.
            stringBuilder.AppendLine("clickTime: " + ClickTime); // Same here.
            stringBuilder.AppendLine("clickCount: " + ClickCount); // Same here.
            stringBuilder.AppendLine("device: " + Device);
            stringBuilder.AppendLine("pointerType: " + PointerType);
            stringBuilder.AppendLine("touchId: " + TouchId);
            stringBuilder.AppendLine("pressPosition: " + PressPosition);
            stringBuilder.AppendLine("trackedDevicePosition: " + TrackedDevicePosition);
            stringBuilder.AppendLine("trackedDeviceOrientation: " + TrackedDeviceOrientation);
            #if UNITY_2021_1_OR_NEWER
            stringBuilder.AppendLine("pressure" + Pressure);
            stringBuilder.AppendLine("radius: " + Radius);
            stringBuilder.AppendLine("azimuthAngle: " + AzimuthAngle);
            stringBuilder.AppendLine("altitudeAngle: " + AltitudeAngle);
            stringBuilder.AppendLine("twist: " + Twist);
            #endif
            #if UNITY_2022_3_OR_NEWER
            stringBuilder.AppendLine("displayIndex: " + DisplayIndex);
            #endif
            return stringBuilder.ToString();
        }

        internal static int MakePointerIdForTouch(int deviceId, int touchId)
        {
            unchecked
            {
                return (deviceId << 24) + touchId;
            }
        }

        internal static int TouchIdFromPointerId(int pointerId)
        {
            return pointerId & 0xff;
        }

        ////TODO: add pressure and tilt support (probably add after 1.0; probably should have separate actions)
        /*
        /// <summary>
        /// If supported by the input device, this is the pressure level of the pointer contact. This is generally
        /// only supported by <see cref="Pen"/> devices as well as by <see cref="Touchscreen"/>s on phones. If not
        /// supported, this will be 1.
        /// </summary>
        /// <seealso cref="Pointer.pressure"/>
        public float pressure { get; set; }

        /// <summary>
        /// If the pointer input is coming from a <see cref="Pen"/>, this is pen's <see cref="Pen.tilt"/>.
        /// </summary>
        public Vector2 tilt { get; set; }
        */

        internal void ReadDeviceState()
        {
            if (Control.parent is Pen pen)
            {
                UiToolkitPointerId = GetPenPointerId(pen);
                #if UNITY_2021_1_OR_NEWER
                Pressure = pen.pressure.magnitude;
                AzimuthAngle = (pen.tilt.value.x + 1) * Mathf.PI / 2;
                AltitudeAngle = (pen.tilt.value.y + 1) * Mathf.PI / 2;
                Twist = pen.twist.value * Mathf.PI * 2;
                #endif
                #if UNITY_2022_3_OR_NEWER
                DisplayIndex = pen.displayIndex.ReadValue();
                #endif
            }
            else if (Control.parent is TouchControl touchControl)
            {
                UiToolkitPointerId = GetTouchPointerId(touchControl);
                #if UNITY_2021_1_OR_NEWER
                Pressure = touchControl.pressure.magnitude;
                Radius = touchControl.radius.value;
                #endif
                #if UNITY_2022_3_OR_NEWER
                DisplayIndex = touchControl.displayIndex.ReadValue();
                #endif
            }
            else if (Control.parent is Touchscreen touchscreen)
            {
                UiToolkitPointerId = GetTouchPointerId(touchscreen.primaryTouch);
                #if UNITY_2021_1_OR_NEWER
                Pressure = touchscreen.pressure.magnitude;
                Radius = touchscreen.radius.value;
                #endif
                #if UNITY_2022_3_OR_NEWER
                DisplayIndex = touchscreen.displayIndex.ReadValue();
                #endif
            }
            else
            {
                UiToolkitPointerId = Interactable.PointerId.mousePointerId;
            }
        }

        private static int GetPenPointerId(Pen pen)
        {
            var n = 0;
            foreach (var otherDevice in InputSystem.devices)
                if (otherDevice is Pen otherPen)
                {
                    if (pen == otherPen)
                    {
                        return Interactable.PointerId.penPointerIdBase +
                               Mathf.Min(n, Interactable.PointerId.penPointerCount - 1);
                    }
                    n++;
                }
            return Interactable.PointerId.penPointerIdBase;
        }

        private static int GetTouchPointerId(TouchControl touchControl)
        {
            var i = ((Touchscreen)touchControl.device).touches.IndexOfReference(touchControl);
            return Interactable.PointerId.touchPointerIdBase +
                   Mathf.Clamp(i, 0, Interactable.PointerId.touchPointerCount - 1);
        }
    }

    /// <summary>
    /// General type of pointer that generated a <see cref="PointerEventData"/> pointer event.
    /// </summary>
    public enum UIPointerType
    {
        None,

        /// <summary>
        /// A <see cref="Mouse"/> or <see cref="Pen"/> or other general <see cref="Pointer"/>.
        /// </summary>
        MouseOrPen,

        /// <summary>
        /// A <see cref="Touchscreen"/>.
        /// </summary>
        Touch,

        /// <summary>
        /// A <see cref="TrackedDevice"/>.
        /// </summary>
        Tracked,
    }

    /// <summary>
    /// Determine how the UI behaves in the presence of multiple pointer devices.
    /// </summary>
    /// <remarks>
    /// While running, an application may, for example, have both a <see cref="Mouse"/> and a <see cref="Touchscreen"/> device
    /// and both may end up getting bound to the actions of <see cref="InputSystemUIInputModule"/> and thus both may route
    /// input into the UI. When this happens, the pointer behavior decides how the UI input module resolves the ambiguity.
    /// </remarks>
    public enum UIPointerBehavior
    {
        /// <summary>
        /// Any input that isn't <see cref="Touchscreen"/> or <see cref="TrackedDevice"/> input is
        /// treated as a single unified pointer.
        ///
        /// This is the default behavior based on the expectation that mice and pens will generally drive a single on-screen
        /// cursor whereas touch and tracked devices have an inherent ability to generate multiple pointers.
        ///
        /// Note that when input from touch or tracked devices is received, the combined pointer for mice and pens (if it exists)
        /// will be removed. If it was over UI objects, <c>IPointerExitHandler</c>s will be invoked.
        /// </summary>
        SingleMouseOrPenButMultiTouchAndTrack,

        /// <summary>
        /// All input is unified to a single pointer. This means that all input from all pointing devices (<see cref="Mouse"/>,
        /// <see cref="Pen"/>, <see cref="Touchscreen"/>, and <see cref="TrackedDevice"/>) is routed into a single pointer
        /// instance. There is only one position on screen which can be controlled from any of these devices.
        /// </summary>
        SingleUnifiedPointer,

        /// <summary>
        /// Any pointing device, whether it's <see cref="Mouse"/>, <see cref="Pen"/>, <see cref="Touchscreen"/>,
        /// or <see cref="TrackedDevice"/> input, is treated as its own independent pointer and arbitrary many
        /// such pointers can be active at any one time.
        /// </summary>
        AllPointersAsIs,
    }
}
#endif
