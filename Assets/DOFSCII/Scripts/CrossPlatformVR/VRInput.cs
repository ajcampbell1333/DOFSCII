using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.Events;

/// <summary>
/// This namespace is meant to house any code that enables 6DOF input agnostically across all platforms on the OpenVR standard.
/// </summary>
namespace CrossplatformVRInput
{
    /// <summary>
    /// There are three different kinds of input coming from 6DOF controllers.
    /// Use this enum as an identifier to handle each type uniquely
    /// </summary>
    public enum InputType
    {
        button,
        axis1D,
        axis2D
    }

    /// <summary>
    /// This struct defines all the data needed to receive and update state for a single 6DOF input
    /// (e.g. buttons, thumbstick, pressure-sensitive trigger, etc.)
    /// The began and finished actions allow other scripts to receive callbacks when a given input is activated/deactivated.
    /// </summary>
    public struct VRInputData
    {
        public VRInputData(int newID, InputType newType, UnityAction began, UnityAction finished)
        {
            ID = newID;
            type = newType;
            isActive = false;
            Began = began;
            Finished = finished;
        }

        public int ID;
        public InputType type;
        public bool isActive { get; set; }
        public UnityAction Began, Finished;
    }

    /// <summary>
    /// A singleton that maps actions to all subscribing scripts, listens for 6DOF controller input,
    /// and executes "began" and "finished" actions at the right time.
    /// This class also optionally updates the transform positions of nodes representing the left and right hand.
    /// </summary>
    public class VRInput : CrossplatformSingleton<VRInput>
    {
        /// NOTE: This class relies heavily on Unity's Input class.
        /// The InputManager.asset file in ProjectSettings must remain intact in order for this class to work properly.
        /// If you export DOFSCII into any other project, don't forget to transfer the InputManager.asset file as well.
        /// See Edit->Project Settings->Input and scroll to the bottom to observe the necessary OpenVR-specific input mappings.
        
        [SerializeField]
        Transform rightHand, leftHand;

        [SerializeField]
        bool trackHands;

        [SerializeField]
        bool trackVRButtonInput;
        
        public Dictionary<string, VRInputData> inputs;

        public Dictionary<string, VRInputData> changeBuffer;

        [HideInInspector]
        public int[] inputIDs = new int[] { 2, 0, 8, 9, 11, 12, 9, 10 };

        [HideInInspector]
        public string[] inputNames = new string[] {
            "mainLeft", "mainRight", "forceClickLeft", "forceClickRight", "gripLeft", "gripRight", "triggerLeft", "triggerRight"
        };
       
        IEnumerator handTrackingLoop;

        #region Init

        /// <summary>
        /// The VRInputSubscriber class uses this function multiple times at runtime to connect to all its needed inputs.
        /// </summary>
        public void SubscribeToInput(string inputName, UnityAction began, UnityAction finished)
        {
            VRInputData updatedInputData = inputs[inputName];
            updatedInputData.Began += began;
            updatedInputData.Finished += finished;
            inputs[inputName] = updatedInputData;
        }

        /// <summary>
        /// This function needs an overload to allow a VRInputSubscriber to create a new input on the spot if the needed input doesn't yet exist.
        /// </summary>
        public void SubscribeToInput(string inputName, int idIndex, InputType type, UnityAction began, UnityAction finished)
        {
            inputs.Add(inputName, new VRInputData(idIndex, type, began, finished));           
        }
        
        void Start()
        {
            inputs = new Dictionary<string, VRInputData>();
            changeBuffer = new Dictionary<string, VRInputData>(); 

            VRInputSubscriber[] subscribers = FindObjectsOfType(typeof(VRInputSubscriber)) as VRInputSubscriber[];
            foreach (VRInputSubscriber subscriber in subscribers)
                subscriber.Init();
            Invoke("StartAfterDelay", 1.0f);
        }

        void StartAfterDelay()
        {
            StartHandTracking();
        }

        #endregion //Init

        #region Custom Update Loop

        public void StartHandTracking()
        {
            StopHandTracking();
            handTrackingLoop = HandTrackingLoop();
            StartCoroutine(handTrackingLoop);
        }

        public void StopHandTracking()
        {
            if (handTrackingLoop != null)
                StopCoroutine(handTrackingLoop);
        }

        /// <summary>
        /// This needs a little refactoring. It'll be better to have two separate loops: one for hand tracking,
        /// and another for button input listening. Currently, if you turn off the hand tracking loop, button input listening stops too.
        /// </summary>
        /// <param name="trackingDeltaTime">Leave this at 0 to sync this loop with Unity's default Update loop.</param>
        IEnumerator HandTrackingLoop()
        {
            while (true)
            {
                if (trackHands)
                {
                    UpdatePosition(rightHand, UnityEngine.XR.XRNode.RightHand);
                    UpdatePosition(leftHand, UnityEngine.XR.XRNode.LeftHand);
                }

                if (trackVRButtonInput)
                {
                    Dictionary<string, VRInputData> updatedDataList = new Dictionary<string, VRInputData>();

                    // store changes to inputs dictionary in changeBuffer so that we can iterate through it uninterrupted
                    foreach (KeyValuePair<string, VRInputData> input in inputs)
                        ButtonInputCheck(input.Key);

                    // apply all changes from the changeBuffer back to the inputs dictionary
                    foreach (KeyValuePair<string, VRInputData> change in changeBuffer)
                        inputs[change.Key] = changeBuffer[change.Key];

                    changeBuffer.Clear();
                }
                
                yield return null;
            }
        }

        #endregion // Custom Update Loop
        
        /// <summary>
        /// While tracking, observe if the specified button is pressed and update state accordingly
        /// </summary>
        /// <param name="buttonName">The agnostic name of the button</param>
        /// <param name="buttonIndex">The index of the button</param>
        void ButtonInputCheck(string inputName)
        {
            VRInputData updatedInputData = inputs[inputName];
            

            switch (inputs[inputName].type)
            {
                case InputType.button:
                    // Poll GettButtonDown only for binary buttons
                    if (Input.GetButtonDown(inputName) && !inputs[inputName].isActive)
                    {
                        inputs[inputName].Began();
                        updatedInputData.isActive = true;
                        StartCoroutine(UpdateStatusWhenInputBecomesInactive(inputName));
                    }
                    break;
                case InputType.axis1D:
                    // Poll GetAxis only for float inputs
                    if (Input.GetAxis(inputName) > 0 && !inputs[inputName].isActive)
                    {
                        inputs[inputName].Began();
                        updatedInputData.isActive = true;
                        StartCoroutine(UpdateStatusWhenInputBecomesInactive(inputName));
                    }
                    break;
                case InputType.axis2D:

                    break;
            }
            changeBuffer.Add(inputName,updatedInputData);
            
        }

        IEnumerator UpdateStatusWhenInputBecomesInactive(string inputName)
        {
            VRInputData updatedInputData = inputs[inputName];
            switch (inputs[inputName].type)
            {
                case InputType.button:
                    yield return new WaitUntil(() => Input.GetButtonUp(inputName));
                    updatedInputData.isActive = false;
                    break;
                case InputType.axis1D:
                    yield return new WaitUntil(() => Input.GetAxis(inputName) == 0);
                    updatedInputData.isActive = false;
                    break;
                
            }
            inputs[inputName].Finished();
            inputs[inputName] = updatedInputData;
        }

        void UpdatePosition(Transform hand, UnityEngine.XR.XRNode node)
        {
            hand.position = UnityEngine.XR.InputTracking.GetLocalPosition(node);
            hand.rotation = UnityEngine.XR.InputTracking.GetLocalRotation(node);
        }
    }
}


