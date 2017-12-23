using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CrossplatformVRInput;
using UnityEngine.UI;
using System;

namespace DOFSCII {

    public class SendKeystrokes : VRInputSubscriber
    {
        [SerializeField]
        int port;
        
        KeyMode currentKeyMode = KeyMode.Alphanumeric;

        /// <summary>
        /// the inputs collected during a given check loop
        /// </summary>
        DOFSCIIMap inputFlags;

        /// <summary>
        /// The time buffer from the beginning of the last intended simultaneous input to the time the input combination is 
        /// interpreted as a keystroke. We need a small buffer to compensate for user latency between multiple inputs that
        /// have simultaneous intent, but we also need to keep the buffer very short to prevent it from overlapping a future keystroke.
        /// </summary>
        [SerializeField]
        float checkLoopDuration;

        /// <summary>
        /// The clock time at which a keystroke is received from the user. If a new stroke is received within the checkLoopDuration of the previous
        /// stroke, the current stroke is considered to be simultaneous with the previous.
        /// </summary>
        float timeCheckLoopBegan;

        /// <summary>
        /// A coroutine which runs while keyboard input is happening and stops when keyboard input is done. The loop waits for the duration,
        /// and if a new stroke occurs during the wait, the wait starts over. When the wait is over, the loop calls back to interpret the key stroke.
        /// </summary>
        IEnumerator combinationCheckLoop;
        
        /// <summary>
        /// Is the input supposed to be capitalized?
        /// </summary>
        bool caps;

        /// <summary>
        /// This counter is distinct from the HammingWeight function in that it tracks the # of buttons currently pressed during the check loop.
        /// This number always reaches zero the moment the loop ends. Hamming Weight tallies the total # of flags tripped during the loop,
        /// but it does so after the loop is finished.
        /// This number needs to go to zero to allow the loop to end when all inputs are released,
        /// and then Hamming Weight tallies the results afterwards.
        /// </summary>
        int inputsActiveTally = 0;

        /// <summary>
        /// The client through which to send input to the keystroke server
        /// </summary>
        UdpClient client;

        void Awake()
        {
            Application.runInBackground = true;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(Network.player.ipAddress), port);
            client = new UdpClient();
            client.Connect(endPoint);
        }

        void StartCheckLoop()
        {
            if (combinationCheckLoop != null)
                StopCoroutine(combinationCheckLoop);
            combinationCheckLoop = CheckLoop();
            timeCheckLoopBegan = Time.time;
            StartCoroutine(combinationCheckLoop);
        }

        void EndCheckLoop()
        {
            // Avoid duplicate checks by only running the check if the loop iEnumerator is not null
            if (combinationCheckLoop != null)
            {
                Send();
                StopCoroutine(combinationCheckLoop);
                combinationCheckLoop = null;
            }
            inputFlags = 0;
        }

        /// <summary>
        /// Send the desired input to the keystroke server
        /// </summary>
        public void Send()
        {
            byte[] packetData = BitConverter.GetBytes((Int32)inputFlags);
            client.Send(packetData, packetData.Length);
        }
        
        /// <summary>
        /// The check loop exists simply to await keystrokes that were intended as simultaneous.
        /// If a second keystroke happens soon enough after the first, it will reset timeCheckLoopBegan 
        /// and allow the loop to continue waiting.
        /// Assign the checkLoopDuration at a small fraction of a second for proper performance.
        /// If duration is too long, some keystrokes intended as separate will be interpreted as simultaneous.
        /// If duration is too short, some keystrokes intended as simultaneous will be interpreted as separate.
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckLoop()
        {
            yield return new WaitUntil(() => Time.time > timeCheckLoopBegan + checkLoopDuration);
            EndCheckLoop();
        }

        /// <summary>
        /// For each input activation event, update the input flags to show the given button was pressed and reset the starting time of the loop.
        /// </summary>
        void InputDownEventHelper(string inputLogName, DOFSCIIMap inputFlag)
        {
            //Debug.Log(inputLogName + " button pressed");
            inputFlags |= inputFlag;
            timeCheckLoopBegan = Time.time;
            inputsActiveTally++;
            if (combinationCheckLoop == null)
                StartCheckLoop();
        }

        #region Input Activation Events
        protected override void OnMainRightDown() { InputDownEventHelper("main right", DOFSCIIMap.mainRight); }
        protected override void OnMainLeftDown() { InputDownEventHelper("main left", DOFSCIIMap.mainLeft); }
        protected override void OnForceClickLeftDown() { InputDownEventHelper("left thumb click", DOFSCIIMap.forceClickLeft); }
        protected override void OnForceClickRightDown() { InputDownEventHelper("right thumb click", DOFSCIIMap.forceClickRight); }
        protected override void OnGripLeftDown() { InputDownEventHelper("left grip", DOFSCIIMap.gripLeft); }
        protected override void OnGripRightDown() { InputDownEventHelper("right grip", DOFSCIIMap.gripRight); }
        protected override void OnTriggerLeftDown() { InputDownEventHelper("left trigger", DOFSCIIMap.triggerLeft); }
        protected override void OnTriggerRightDown() { InputDownEventHelper("right trigger", DOFSCIIMap.triggerRight); }
        #endregion

        /// <summary>
        /// For each input deactivation event, check to see if there are still any buttons pressed. If not, end the check loop.
        /// </summary>
        /// <param name="inputLogName">the name of the input to be printed in the log</param>
        void InputUpEventHelper(string inputLogName)
        {
            //Debug.Log(inputLogName + " button released");
            inputsActiveTally--;
            if (inputsActiveTally <= 0)
                inputsActiveTally = 0;

            if (inputsActiveTally == 0)
                EndCheckLoop();
        }

        #region Input Release Events
        protected override void OnMainRightUp() { InputUpEventHelper("main right"); }
        protected override void OnMainLeftUp() { InputUpEventHelper("left right"); }
        protected override void OnForceClickLeftUp() { InputUpEventHelper("left thumb click"); }
        protected override void OnForceClickRightUp() { InputUpEventHelper("right thumb click"); }
        protected override void OnGripLeftUp() { InputUpEventHelper("left grip"); }
        protected override void OnGripRightUp() { InputUpEventHelper("right grip"); }
        protected override void OnTriggerLeftUp() { InputUpEventHelper("left trigger"); }
        protected override void OnTriggerRightUp() { InputUpEventHelper("right trigger"); }
        #endregion
    }





}

