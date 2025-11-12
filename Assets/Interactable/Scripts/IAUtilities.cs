using UnityEngine;

namespace Interactable
{
    public static class IAUtilities
    {
        //From UnityEngine.UI.MultipleDisplayUtilities
        
        /// <summary>
        /// A version of Display.RelativeMouseAt that scales the position when the main display has a different rendering resolution to the system resolution.
        /// By default, the mouse position is relative to the main render area, we need to adjust this so it is relative to the system resolution
        /// in order to correctly determine the position on other displays.
        /// </summary>
        /// <returns></returns>
        public static Vector3 RelativeMouseAtScaled(Vector2 position, int displayIndex)
        {
            #if !UNITY_EDITOR && !UNITY_WSA
            // If the main display is not the same resolution as the system then we need to scale the mouse position. (case 1141732)
            var display = Display.main;

#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && UNITY_ANDROID
            // On Android with the new input system passed position are always relative to a surface and scaled accordingly to the rendering resolution.
            display = Display.displays[displayIndex];
            if (!Screen.fullScreen)
            {
                // If not in fullscreen, assume UaaL multi-view multi-screen multi-touch scenario, where the position is already in the correct scaled coordinates for the displayIndex
                return new Vector3(position.x, position.y, displayIndex);
            }
            // in full screen, we need to account for some padding which may be added to the rendering area ? (Behavior unchanged for main display, untested for non-main displays)
#endif
            if (display.renderingWidth != display.systemWidth || display.renderingHeight != display.systemHeight)
            {
                // The system will add padding when in full-screen and using a non-native aspect ratio. (case UUM-7893)
                // For example Rendering 1920x1080 with a systeem resolution of 3440x1440 would create black bars on each side that are 330 pixels wide.
                // we need to account for this or it will offset our coordinates when we are not on the main display.
                var systemAspectRatio = display.systemWidth / (float)display.systemHeight;

                var sizePlusPadding = new Vector2(display.renderingWidth, display.renderingHeight);
                var padding = Vector2.zero;
                if (Screen.fullScreen)
                {
                    var aspectRatio = Screen.width / (float)Screen.height; // This assumes aspectRatio is the same for all displays
                    if (display.systemHeight * aspectRatio < display.systemWidth)
                    {
                        // Horizontal padding
                        sizePlusPadding.x = display.renderingHeight * systemAspectRatio;
                        padding.x = (sizePlusPadding.x - display.renderingWidth) * 0.5f;
                    }
                    else
                    {
                        // Vertical padding
                        sizePlusPadding.y = display.renderingWidth / systemAspectRatio;
                        padding.y = (sizePlusPadding.y - display.renderingHeight) * 0.5f;
                    }
                }

                var sizePlusPositivePadding = sizePlusPadding - padding;

                // If we are not inside of the main display then we must adjust the mouse position so it is scaled by
                // the main display and adjusted for any padding that may have been added due to different aspect ratios.
                if (position.y < -padding.y || position.y > sizePlusPositivePadding.y ||
                     position.x < -padding.x || position.x > sizePlusPositivePadding.x)
                {
                    var adjustedPosition = position;

                    if (!Screen.fullScreen)
                    {
                        // When in windowed mode, the window will be centered with the 0,0 coordinate at the top left, we need to adjust so it is relative to the screen instead.
                        adjustedPosition.x -= (display.renderingWidth - display.systemWidth) * 0.5f;
                        adjustedPosition.y -= (display.renderingHeight - display.systemHeight) * 0.5f;
                    }
                    else
                    {
                        // Scale the mouse position to account for the black bars when in a non-native aspect ratio.
                        adjustedPosition += padding;
                        adjustedPosition.x *= display.systemWidth / sizePlusPadding.x;
                        adjustedPosition.y *= display.systemHeight / sizePlusPadding.y;
                    }

#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && UNITY_ANDROID
                    var relativePos = new Vector3(adjustedPosition.x, adjustedPosition.y, displayIndex);
#else
                    var relativePos = Display.RelativeMouseAt(adjustedPosition);
#endif

                    // If we are not on the main display then return the adjusted position.
                    if (relativePos.z != 0)
                        return relativePos;
                }

                // We are using the main display.
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && UNITY_ANDROID
                // On Android, in all cases, it is a surface associated to a given displayIndex, so we need to use the display index
                return new Vector3(position.x, position.y, displayIndex);
#else
                return new Vector3(position.x, position.y, 0);
#endif
            }
            #endif
#if ENABLE_INPUT_SYSTEM && PACKAGE_INPUTSYSTEM && UNITY_ANDROID
            return new Vector3(position.x, position.y, displayIndex);
#else
            return Display.RelativeMouseAt(position);
#endif
        }
    }
}