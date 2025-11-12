using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Interactable.Example
{
    public class FPController : MonoBehaviour
    {
        private CharacterController m_controller;
        public bool inputEnabled = true;

        #region Camera Movement Variables

        public Camera playerCamera;

        public float fov = 60f;
        public bool invertCamera;
        public bool cameraCanMove = true;
        public float mouseSensitivity = 2f;
        public float maxLookAngle = 50f;

        public bool lockCursor = true;


        // Internal Variables
        private float yaw;
        private float pitch;

        #region Camera Zoom Variables

        public bool enableZoom = true;
        public bool holdToZoom;
        public KeyCode zoomKey = KeyCode.Mouse1;
        public float zoomFOV = 30f;
        public float zoomStepTime = 5f;

        // Internal Variables
        private bool isZoomed;

        #endregion

        #endregion

        #region Movement Variables

        public bool playerCanMove = true;
        public float walkSpeed = 5f;
        public float maxVelocityChange = 10f;

        // Internal Variables
        private bool isWalking;

        #region Sprint

        public bool enableSprint = true;
        public bool unlimitedSprint;
        public KeyCode sprintKey = KeyCode.LeftShift;
        public float sprintSpeed = 7f;
        public float sprintDuration = 5f;
        public float sprintCooldown = .5f;
        public float sprintFOV = 80f;
        public float sprintFOVStepTime = 10f;

        // Internal Variables
        private bool isSprinting;
        private float sprintRemaining;
        private bool isSprintCooldown;
        private float sprintCooldownReset;

        #endregion

        #region Jump

        public bool enableJump = true;
        public KeyCode jumpKey = KeyCode.Space;

        public float jumpHeight = 1.2f;
        public float gravity = -15.0f;
        public float jumpTimeout = 0.1f;
        public float fallTimeout = 0.15f;

        // timeout delta time
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private bool _inputJump;

        // player
        private float _verticalVelocity;
        private readonly float _terminalVelocity = 53.0f;


        #endregion

        #region Crouch

        public bool enableCrouch = true;
        public bool holdToCrouch = true;
        public KeyCode crouchKey = KeyCode.LeftControl;
        public float crouchHeight = .75f;
        public float speedReduction = .5f;

        // Internal Variables
        private bool isCrouched;
        private Vector3 originalScale;

        #endregion

        #endregion


        public bool grounded;
        public float groundedOffset = -0.14f;
        public float groundedRadius = 0.5f;
        public LayerMask groundLayers;

        #region Head Bob

        public bool enableHeadBob = true;
        public Transform joint;
        public float bobSpeed = 10f;
        public Vector3 bobAmount = new(.15f, .05f, 0f);

        // Internal Variables
        private Vector3 jointOriginalPos;
        private float timer;

        #endregion

        private void Awake()
        {
            m_controller = GetComponent<CharacterController>();

            // Set internal variables
            playerCamera.fieldOfView = fov;
            originalScale = transform.localScale;
            jointOriginalPos = joint.localPosition;

            if (!unlimitedSprint)
            {
                sprintRemaining = sprintDuration;
                sprintCooldownReset = sprintCooldown;
            }
        }

        void Start()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;

        }

        float camRotation;

        private void Update()
        {
            if (!inputEnabled)
                return;

            #region Camera

            // Control camera movement
            if (cameraCanMove)
            {
                yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

                if (!invertCamera)
                {
                    pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
                }
                else
                {
                    // Inverted Y
                    pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
                }

                // Clamp pitch between lookAngle
                pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

                transform.localEulerAngles = new Vector3(0, yaw, 0);
                playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
            }

            #region Camera Zoom

            if (enableZoom)
            {
                // Changes isZoomed when key is pressed
                // Behavior for toggle zoom
                if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
                {
                    isZoomed = !isZoomed;
                }

                // Changes isZoomed when key is pressed
                // Behavior for hold to zoom
                if (holdToZoom && !isSprinting)
                {
                    if (Input.GetKeyDown(zoomKey))
                    {
                        isZoomed = true;
                    }
                    else if (Input.GetKeyUp(zoomKey))
                    {
                        isZoomed = false;
                    }
                }

                playerCamera.fieldOfView = isZoomed switch
                {
                    // Lerp camera.fieldOfView to allow for a smooth transition
                    true => Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime),
                    false when !isSprinting => Mathf.Lerp(playerCamera.fieldOfView, fov,
                        zoomStepTime * Time.deltaTime),
                    _ => playerCamera.fieldOfView
                };
            }

            #endregion

            #endregion

            #region Sprint

            if (enableSprint)
            {
                if (isSprinting)
                {
                    isZoomed = false;
                    playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV,
                        sprintFOVStepTime * Time.deltaTime);

                    // Drain sprint remaining while sprinting
                    if (!unlimitedSprint)
                    {
                        sprintRemaining -= 1 * Time.deltaTime;
                        if (sprintRemaining <= 0)
                        {
                            isSprinting = false;
                            isSprintCooldown = true;
                        }
                    }
                }
                else
                {
                    // Regain sprint while not sprinting
                    sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);
                }

                // Handles sprint cooldown 
                // When sprint remaining == 0 stops sprint ability until hitting cooldown
                if (isSprintCooldown)
                {
                    sprintCooldown -= 1 * Time.deltaTime;
                    if (sprintCooldown <= 0)
                    {
                        isSprintCooldown = false;
                    }
                }
                else
                {
                    sprintCooldown = sprintCooldownReset;
                }

            }

            #endregion

            #region Jump

            // Gets input and calls jump method
            if (enableJump && Input.GetKeyDown(jumpKey) && grounded)
            {
                Jump();
            }

            JumpAndGravity();

            #endregion

            #region Crouch

            if (enableCrouch)
            {
                if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
                {
                    Crouch();
                }

                if (Input.GetKeyDown(crouchKey) && holdToCrouch)
                {
                    isCrouched = false;
                    Crouch();
                }
                else if (Input.GetKeyUp(crouchKey) && holdToCrouch)
                {
                    isCrouched = true;
                    Crouch();
                }
            }

            #endregion

            GroundedCheck();

            if (enableHeadBob)
            {
                HeadBob();
            }

            ApplyMove();
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 position = transform.position;
            Vector3 spherePosition = new Vector3(position.x, position.y + groundedOffset, position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_inputJump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = jumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                // if we are not grounded, do not jump
                _inputJump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void Jump()
        {
            _inputJump = true;

            // When crouched and using toggle system, will uncrouch for a jump
            if (isCrouched && !holdToCrouch)
            {
                Crouch();
            }
        }

        private void Crouch()
        {
            // Stands player up to full height
            // Brings walkSpeed back up to original speed
            if (isCrouched)
            {
                transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
                walkSpeed /= speedReduction;

                isCrouched = false;
            }
            // Crouches player down to set height
            // Reduces walkSpeed
            else
            {
                transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
                walkSpeed *= speedReduction;

                isCrouched = true;
            }
        }

        private void HeadBob()
        {
            if (isWalking)
            {
                // Calculates HeadBob speed during sprint
                if (isSprinting)
                {
                    timer += Time.deltaTime * (bobSpeed + sprintSpeed);
                }
                // Calculates HeadBob speed during crouched movement
                else if (isCrouched)
                {
                    timer += Time.deltaTime * (bobSpeed * speedReduction);
                }
                // Calculates HeadBob speed during walking
                else
                {
                    timer += Time.deltaTime * bobSpeed;
                }

                // Applies HeadBob movement
                joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x,
                    jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y,
                    jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
            }
            else
            {
                // Resets when play stops moving
                timer = 0;
                var localPosition = joint.localPosition;
                localPosition = new Vector3(Mathf.Lerp(localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed),
                    Mathf.Lerp(localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed),
                    Mathf.Lerp(localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
                joint.localPosition = localPosition;
            }
        }

        private void ApplyMove()
        {
            #region Movement

            if (playerCanMove)
            {
                // Calculate how fast we should be moving
                Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

                // Checks if player is walking and isGrounded
                // Will allow head bob
                if ((targetVelocity.x != 0 || targetVelocity.z != 0) && grounded)
                {
                    isWalking = true;
                }
                else
                {
                    isWalking = false;
                }

                // All movement calculations shill sprint is active
                if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
                {
                    targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                    // Apply a force that attempts to reach our target velocity
                    Vector3 velocity = m_controller.velocity;
                    Vector3 velocityChange = (targetVelocity - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                    velocityChange.y = 0;

                    // Player is only moving when velocity change != 0
                    // Makes sure fov change only happens during movement
                    if (velocityChange.x != 0 || velocityChange.z != 0)
                    {
                        isSprinting = true;

                        if (isCrouched)
                        {
                            Crouch();
                        }
                    }

                    m_controller.Move((velocity + velocityChange + new Vector3(0.0f, _verticalVelocity, 0.0f)) *
                                      Time.deltaTime);
                }
                // All movement calculations while walking
                else
                {
                    isSprinting = false;

                    targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                    // Apply a force that attempts to reach our target velocity
                    Vector3 velocity = m_controller.velocity;
                    Vector3 velocityChange = (targetVelocity - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                    velocityChange.y = 0;

                    m_controller.Move((velocity + velocityChange + new Vector3(0.0f, _verticalVelocity, 0.0f)) *
                                      Time.deltaTime);
                }
            }

            #endregion
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = grounded ? transparentGreen : transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            var position = transform.position;
            Gizmos.DrawSphere(new Vector3(position.x, position.y - groundedOffset, position.z), groundedRadius);
        }

    }



// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(FPController))]
    public class FirstPersonControllerEditor : Editor
    {
        FPController fpc;
        SerializedObject serFpc;

        private void OnEnable()
        {
            fpc = (FPController)target;
            serFpc = new SerializedObject(fpc);
        }

        public override void OnInspectorGUI()
        {
            serFpc.Update();

            #region Camera Setup

            GUILayout.Label("Camera Setup",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(
                new GUIContent("Camera", "Camera attached to the controller."), fpc.playerCamera,
                typeof(Camera), true);
            fpc.fov = EditorGUILayout.Slider(
                new GUIContent("Field of View", "The camera’s view angle. Changes the player camera directly."),
                fpc.fov, fpc.zoomFOV, 179f);
            fpc.cameraCanMove = EditorGUILayout.ToggleLeft(
                new GUIContent("Enable Camera Rotation", "Determines if the camera is allowed to move."),
                fpc.cameraCanMove);

            GUI.enabled = fpc.cameraCanMove;
            fpc.invertCamera = EditorGUILayout.ToggleLeft(
                new GUIContent("Invert Camera Rotation", "Inverts the up and down movement of the camera."),
                fpc.invertCamera);
            fpc.mouseSensitivity =
                EditorGUILayout.Slider(
                    new GUIContent("Look Sensitivity", "Determines how sensitive the mouse movement is."),
                    fpc.mouseSensitivity, .1f, 10f);
            fpc.maxLookAngle =
                EditorGUILayout.Slider(
                    new GUIContent("Max Look Angle",
                        "Determines the max and min angle the player camera is able to look."), fpc.maxLookAngle, 40,
                    90);
            GUI.enabled = true;

            fpc.lockCursor = EditorGUILayout.ToggleLeft(
                new GUIContent("Lock and Hide Cursor",
                    "Turns off the cursor visibility and locks it to the middle of the screen."), fpc.lockCursor);

            EditorGUILayout.Space();

            #region Camera Zoom Setup

            GUILayout.Label("Zoom",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));

            fpc.enableZoom = EditorGUILayout.ToggleLeft(
                new GUIContent("Enable Zoom", "Determines if the player is able to zoom in while playing."),
                fpc.enableZoom);

            GUI.enabled = fpc.enableZoom;
            fpc.holdToZoom = EditorGUILayout.ToggleLeft(
                new GUIContent("Hold to Zoom",
                    "Requires the player to hold the zoom key instead if pressing to zoom and unzoom."),
                fpc.holdToZoom);
            fpc.zoomKey =
                (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Zoom Key", "Determines what key is used to zoom."),
                    fpc.zoomKey);
            fpc.zoomFOV =
                EditorGUILayout.Slider(new GUIContent("Zoom FOV", "Determines the field of view the camera zooms to."),
                    fpc.zoomFOV, .1f, fpc.fov);
            fpc.zoomStepTime =
                EditorGUILayout.Slider(
                    new GUIContent("Step Speed", "Determines how fast the FOV transitions while zooming in."),
                    fpc.zoomStepTime, .1f, 10f);
            GUI.enabled = true;

            #endregion

            #endregion

            #region Movement Setup

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Movement Setup",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            fpc.playerCanMove = EditorGUILayout.ToggleLeft(
                new GUIContent("Enable Player Movement", "Determines if the player is allowed to move."),
                fpc.playerCanMove);

            GUI.enabled = fpc.playerCanMove;
            fpc.walkSpeed =
                EditorGUILayout.Slider(
                    new GUIContent("Walk Speed", "Determines how fast the player will move while walking."),
                    fpc.walkSpeed, .1f, fpc.sprintSpeed);
            GUI.enabled = true;

            EditorGUILayout.Space();

            #region Sprint

            GUILayout.Label("Sprint",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));

            fpc.enableSprint = EditorGUILayout.ToggleLeft(
                new GUIContent("Enable Sprint", "Determines if the player is allowed to sprint."), fpc.enableSprint);

            GUI.enabled = fpc.enableSprint;
            fpc.unlimitedSprint = EditorGUILayout.ToggleLeft(
                new GUIContent("Unlimited Sprint",
                    "Determines if 'Sprint Duration' is enabled. Turning this on will allow for unlimited sprint."),
                fpc.unlimitedSprint);
            fpc.sprintKey =
                (KeyCode)EditorGUILayout.EnumPopup(
                    new GUIContent("Sprint Key", "Determines what key is used to sprint."), fpc.sprintKey);
            fpc.sprintSpeed =
                EditorGUILayout.Slider(
                    new GUIContent("Sprint Speed", "Determines how fast the player will move while sprinting."),
                    fpc.sprintSpeed, fpc.walkSpeed, 20f);

            //GUI.enabled = !fpc.unlimitedSprint;
            fpc.sprintDuration =
                EditorGUILayout.Slider(
                    new GUIContent("Sprint Duration",
                        "Determines how long the player can sprint while unlimited sprint is disabled."),
                    fpc.sprintDuration, 1f, 20f);
            fpc.sprintCooldown =
                EditorGUILayout.Slider(
                    new GUIContent("Sprint Cooldown",
                        "Determines how long the recovery time is when the player runs out of sprint."),
                    fpc.sprintCooldown, .1f, fpc.sprintDuration);
            //GUI.enabled = true;

            fpc.sprintFOV =
                EditorGUILayout.Slider(
                    new GUIContent("Sprint FOV", "Determines the field of view the camera changes to while sprinting."),
                    fpc.sprintFOV, fpc.fov, 179f);
            fpc.sprintFOVStepTime =
                EditorGUILayout.Slider(
                    new GUIContent("Step Speed", "Determines how fast the FOV transitions while sprinting."),
                    fpc.sprintFOVStepTime, .1f, 20f);

            GUI.enabled = true;

            EditorGUILayout.Space();

            #endregion

            #region Jump

            GUILayout.Label("Jump",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));

            fpc.enableJump =
                EditorGUILayout.ToggleLeft(
                    new GUIContent("Enable Jump", "Determines if the player is allowed to jump."), fpc.enableJump);

            GUI.enabled = fpc.enableJump;
            fpc.jumpKey =
                (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "Determines what key is used to jump."),
                    fpc.jumpKey);
            fpc.jumpHeight =
                EditorGUILayout.FloatField(new GUIContent("Jump Height", "The height the player can jump."),
                    fpc.jumpHeight);
            fpc.gravity = EditorGUILayout.FloatField(
                new GUIContent("Gravity", "The character uses its own gravity value. The engine default is -9.81f"),
                fpc.gravity);
            fpc.jumpTimeout = EditorGUILayout.FloatField(
                new GUIContent("Jump Timeout",
                    "Time required to pass before being able to jump again. Set to 0f to instantly jump again."),
                fpc.jumpTimeout);
            fpc.fallTimeout = EditorGUILayout.FloatField(
                new GUIContent("Fall Timeout",
                    "Time required to pass before entering the fall state. Useful for walking down stairs"),
                fpc.fallTimeout);

            GUI.enabled = true;

            EditorGUILayout.Space();

            #endregion

            #region Crouch

            GUILayout.Label("Crouch",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));

            fpc.enableCrouch = EditorGUILayout.ToggleLeft(
                new GUIContent("Enable Crouch", "Determines if the player is allowed to crouch."), fpc.enableCrouch);

            GUI.enabled = fpc.enableCrouch;
            fpc.holdToCrouch = EditorGUILayout.ToggleLeft(
                new GUIContent("Hold To Crouch",
                    "Requires the player to hold the crouch key instead if pressing to crouch and uncrouch."),
                fpc.holdToCrouch);
            fpc.crouchKey =
                (KeyCode)EditorGUILayout.EnumPopup(
                    new GUIContent("Crouch Key", "Determines what key is used to crouch."), fpc.crouchKey);
            fpc.crouchHeight =
                EditorGUILayout.Slider(
                    new GUIContent("Crouch Height", "Determines the y scale of the player object when crouched."),
                    fpc.crouchHeight, .1f, 1);
            fpc.speedReduction =
                EditorGUILayout.Slider(
                    new GUIContent("Speed Reduction",
                        "Determines the percent 'Walk Speed' is reduced by. 1 being no reduction, and .5 being half."),
                    fpc.speedReduction, .1f, 1);
            GUI.enabled = true;

            #endregion

            #endregion

            #region Head Bob

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Head Bob Setup",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            fpc.enableHeadBob = EditorGUILayout.ToggleLeft(
                new GUIContent("Enable Head Bob", "Determines if the camera will bob while the player is walking."),
                fpc.enableHeadBob);


            GUI.enabled = fpc.enableHeadBob;
            fpc.joint = (Transform)EditorGUILayout.ObjectField(
                new GUIContent("Camera Joint", "Joint object position is moved while head bob is active."), fpc.joint,
                typeof(Transform), true);
            fpc.bobSpeed =
                EditorGUILayout.Slider(new GUIContent("Speed", "Determines how often a bob rotation is completed."),
                    fpc.bobSpeed, 1, 20);
            fpc.bobAmount = EditorGUILayout.Vector3Field(
                new GUIContent("Bob Amount", "Determines the amount the joint moves in both directions on every axes."),
                fpc.bobAmount);
            GUI.enabled = true;

            #endregion

            #region Ground Check

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Ground Check Setup",
                new GUIStyle(GUI.skin.label)
                    { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 },
                GUILayout.ExpandWidth(true));
            EditorGUILayout.Space();

            fpc.groundedOffset =
                EditorGUILayout.FloatField(new GUIContent("Grounded Offset", "Useful for rough ground"),
                    fpc.groundedOffset);
            fpc.groundedRadius =
                EditorGUILayout.Slider(
                    new GUIContent("Grounded Radius",
                        "The radius of the grounded check. Should match the radius of the CharacterController"),
                    fpc.groundedRadius, 0.1f, 1.0f);
            fpc.groundLayers = EditorGUILayout.MaskField(
                new GUIContent("Ground Layers", "What layers the character uses as ground"), fpc.groundLayers,
                InternalEditorUtility.layers);
            EditorGUILayout.LabelField("On Ground", fpc.grounded.ToString());

            GUI.enabled = true;

            #endregion

            //Sets any changes from the prefab
            if (GUI.changed)
            {
                EditorUtility.SetDirty(fpc);
                Undo.RecordObject(fpc, "FPC Change");
                serFpc.ApplyModifiedProperties();
            }
        }

    }

#endif

}