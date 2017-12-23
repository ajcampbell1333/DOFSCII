using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This namespace is meant to house any code that enables 6DOF input agnostically across all platforms on the OpenVR standard.
/// </summary>
namespace CrossplatformVRInput {
    
    /// <summary>
    /// Inherit from this class and override any of the available functions to receive callbacks from 
    /// button presses for either Rift or Vive. 
    /// When deriving from this script, make sure this AND a VRInput script are both active in your scene at runtime. 
    /// </summary>
    public class VRInputSubscriber : MonoBehaviour
    {
        /// <summary>
        /// Oculus and Vive give opposite +- float values on the thumbstick/thumbpad Y axis. Use this to flip the values
        /// to your desired orientation.
        /// </summary>
        [SerializeField]
        bool upDownInvert;

        /// <summary>
        /// Subscribe all functions below to the VRInput event handler. This script or any scripts you derive from it
        /// will only be activated by an instance of VRInput.cs in your scene. Init() is called by VRInput.
        /// </summary>
        public void Init()
        {
            UnityAction[] actions = new UnityAction[] {
                () => OnMainLeftDown(), () => OnMainRightDown(), () => OnForceClickLeftDown(),() => OnForceClickRightDown(),
                () => OnGripLeftDown(), () => OnGripRightDown(), () => OnTriggerLeftDown(),() => OnTriggerRightDown(),
                () => OnMainLeftUp(), () => OnMainRightUp(), () => OnForceClickLeftUp(), () => OnForceClickRightUp(),
                () => OnGripLeftUp(), () => OnGripRightUp(), () => OnTriggerLeftUp(), () => OnTriggerRightUp()
            };

            for (int i = 0; i < VRInput.Instance.inputNames.Length; i++)
            {
                if (VRInput.Instance.inputs.ContainsKey(VRInput.Instance.inputNames[i]))
                    VRInput.Instance.SubscribeToInput(
                                                        VRInput.Instance.inputNames[i], 
                                                        actions[i], 
                                                        actions[i+VRInput.Instance.inputNames.Length]
                                                    );
                else
                {
                    InputType type = (i < 3) ? InputType.button : InputType.axis1D;
                    VRInput.Instance.SubscribeToInput(
                                                        VRInput.Instance.inputNames[i],
                                                        VRInput.Instance.inputIDs[i],
                                                        type,
                                                        actions[i],
                                                        actions[i + VRInput.Instance.inputNames.Length]
                                                    );
                }
            }
            
        }

        /// <summary>
        /// Track thumbstick/thumbpad float values for X & Y for either hand.
        /// </summary>
        void Update()
        {
            float leftThumbCheck = Input.GetAxis("xAxisLeft");
            if (leftThumbCheck != 0)
            {
                if (leftThumbCheck > 0.5f) LeftThumbXActive(true);
                else if (leftThumbCheck < -0.5f) LeftThumbXActive(false);
            }

            float rightThumbCheck = Input.GetAxis("xAxisRight");
            if (rightThumbCheck != 0)
            {
                if (rightThumbCheck > 0.5f) RightThumbXActive(true);
                else if (rightThumbCheck < -0.5f) RightThumbXActive(false);
            }

            float leftThumbCheckY = Input.GetAxis("yAxisLeft");
            if (leftThumbCheckY != 0)
            {
                if (leftThumbCheckY > 0.5f) LeftThumbYActive((upDownInvert) ? false : true);
                else if (leftThumbCheckY < -0.5f) LeftThumbYActive((upDownInvert) ? true : false);
            }

            float rightThumbCheckY = Input.GetAxis("yAxisRight");
            if (rightThumbCheckY != 0)
            {
                if (rightThumbCheckY > 0.5f) RightThumbYActive((upDownInvert) ? false : true);
                else if (rightThumbCheckY < -0.5f) RightThumbYActive((upDownInvert) ? true : false);
            }

        }

        protected virtual void OnMainRightDown()
        {
            Debug.Log("main right button pressed");
        }

        protected virtual void OnMainLeftDown()
        {
            Debug.Log("main left button pressed");
        }

        protected virtual void OnForceClickLeftDown()
        {
            Debug.Log("left thumb click button pressed");
        }

        protected virtual void OnForceClickRightDown()
        {
            Debug.Log("right thumb click button pressed");

        }

        protected virtual void OnGripLeftDown()
        {
            Debug.Log("left grip button pressed");
        }

        protected virtual void OnGripRightDown()
        {
            Debug.Log("right grip button pressed");
        }

        protected virtual void OnTriggerLeftDown()
        {
            Debug.Log("left trigger button pressed");
        }

        protected virtual void OnTriggerRightDown()
        {
            Debug.Log("right trigger button pressed");
        }

        protected virtual void OnMainRightUp()
        {
            Debug.Log("main right button released");
        }

        protected virtual void OnMainLeftUp()
        {
            Debug.Log("main left button released");
        }

        protected virtual void OnForceClickLeftUp()
        {
            Debug.Log("left thumb click button released");
        }

        protected virtual void OnForceClickRightUp()
        {
            Debug.Log("right thumb click button released");
        }

        protected virtual void OnGripLeftUp()
        {
            Debug.Log("left grip button released");
        }

        protected virtual void OnGripRightUp()
        {
            Debug.Log("right grip button released");
        }

        protected virtual void OnTriggerLeftUp()
        {
            Debug.Log("left trigger button released");
        }

        protected virtual void OnTriggerRightUp()
        {
            Debug.Log("right trigger button released");
        }

        protected virtual void LeftThumbXActive(bool right)
        {
            if (right) Debug.Log("Left thumb is pressing rightward.");
            else Debug.Log("Left thumb is pressing leftward.");
        }

        protected virtual void RightThumbXActive(bool right)
        {
            if (right) Debug.Log("Right thumb is pressing rightward.");
            else Debug.Log("Right thumb is pressing leftward.");
        }

        protected virtual void LeftThumbYActive(bool up)
        {
            if (up) Debug.Log("Left thumb is pressing upward.");
            else Debug.Log("Left thumb is pressing downward.");
        }

        protected virtual void RightThumbYActive(bool up)
        {
            if (up) Debug.Log("Right thumb is pressing upward.");
            else Debug.Log("Right thumb is pressing downward.");
        }

    }




}
