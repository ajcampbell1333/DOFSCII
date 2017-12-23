using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrossplatformVRInput;
using UnityEngine.UI;
using WindowsInput;
using UnityEngine.EventSystems;

namespace DOFSCII
{

    public class DOFSCIIInputHandler : VRInputSubscriber
    {
        [SerializeField]
        Toggle doubleThumbToggle;

        [SerializeField]
        Toggle dofsciiActiveStatus;

        ButtonHighlight[] alphaButtons;
        ButtonHighlight[] bracketButtons;

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

        IEnumerator arrowStatusCheck;

        IEnumerator backspaceStatusCheck;

        [SerializeField]
        bool outputToDisplayText;

        /// <summary>
        /// A UI element to display the user's keystrokes
        /// </summary>
        [SerializeField]
        Text displayText;

        /// <summary>
        /// The character(s) that will get added to the active text field once the check loop is complete.
        /// </summary>
        string newCharacter = "";

        /// <summary>
        /// Is the input supposed to be capitalized?
        /// </summary>
        bool caps;

        public bool dofsciiOn;

        public void ToggleDOFSCII(bool on)
        {
            dofsciiOn = on;
            GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(null);
        }

        public void ToggleDOFSCIIViaUI()
        {
            dofsciiOn = dofsciiActiveStatus.isOn;
            GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(null);
        }
        /// <summary>
        /// This counter is distinct from the HammingWeight function in that it tracks the # of buttons currently pressed during the check loop.
        /// This number always reaches zero the moment the loop ends. Hamming Weight tallies the total # of flags tripped during the loop,
        /// but it does so after the loop is finished.
        /// This number needs to go to zero to allow the loop to end when all inputs are released,
        /// and then Hamming Weight tallies the results afterwards.
        /// </summary>
        int inputsActiveTally = 0;

        void Awake()
        {
            Application.runInBackground = true;
            alphaButtons = GameObject.Find("AlphaGroup").GetComponentsInChildren<ButtonHighlight>();
            bracketButtons = GameObject.Find("BracketGroup").GetComponentsInChildren<ButtonHighlight>();
            caps = (InputSimulator.IsTogglingKeyInEffect(VirtualKeyCode.CAPITAL)) ? true : false;
            if (caps)
            {
                alphaButtons[48].HighlightToggle(caps);
                //inputFlags |= DOFSCIIMap.CapsToggle;
            }
            dofsciiOn = true;
            //inputFlags |= DOFSCIIMap.DOFSCIIToggle;
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
            // Avoid duplicate checks by only running the check if the loop inenumerator is not null
            if (combinationCheckLoop != null)
            {

                if (doubleThumbToggle.isOn && inputFlags.HasFlag(DOFSCIIMap.DOFSCIIToggle))
                {
                    dofsciiOn = !dofsciiOn;
                    dofsciiActiveStatus.isOn = dofsciiOn;
                    inputFlags = 0;
                    return;
                }

                if (dofsciiOn)
                {
                    if (inputFlags.HasFlag(DOFSCIIMap.arrowLeft))
                    {
                        if (caps)
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.LEFT);
                        else InputSimulator.SimulateKeyPress(VirtualKeyCode.LEFT);
                    }
                    else if (inputFlags.HasFlag(DOFSCIIMap.arrowRight)) {
                        if (caps)
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.RIGHT);
                        else InputSimulator.SimulateKeyPress(VirtualKeyCode.RIGHT);
                    }
                    else if (inputFlags.HasFlag(DOFSCIIMap.arrowUp))
                    {
                        if (caps)
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.UP);
                        else InputSimulator.SimulateKeyPress(VirtualKeyCode.UP);
                    }
                    else if (inputFlags.HasFlag(DOFSCIIMap.arrowDown))
                    {
                        if (caps)
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.DOWN);
                        else InputSimulator.SimulateKeyPress(VirtualKeyCode.DOWN);
                    }
                    else if (inputFlags.HasFlag(DOFSCIIMap.Backspace))
                    {
                        if (displayText != null && displayText.text.Length > 0)
                            displayText.text = displayText.text.Substring(0, displayText.text.Length - 1);

                        InputSimulator.SimulateKeyPress(VirtualKeyCode.BACK);
                    }
                    else if (inputFlags.HasFlag(DOFSCIIMap.Delete))
                    {
                        if (displayText.text.Length > 0)
                            displayText.text = displayText.text.Substring(0, displayText.text.Length - 1);

                        InputSimulator.SimulateKeyPress(VirtualKeyCode.DELETE);
                    }
                    else if (inputFlags.HasFlag(DOFSCIIMap.modeSwitch))
                    {
                        int newMode = (currentKeyMode == KeyMode.Alphanumeric) ? 1 : -1;
                        currentKeyMode = (KeyMode)((int)currentKeyMode + newMode);
                        alphaButtons[49].Highlight();
                        Debug.Log("Mode: " + currentKeyMode);
                    }
                    //else if (inputFlags.HasFlag(DOFSCIIMap.CapsToggle)) caps = !caps;
                    else if (inputFlags.HasFlag(DOFSCIIMap.CapsToggle))
                    {
                        caps = !caps;
                        InputSimulator.SimulateKeyPress(VirtualKeyCode.CAPITAL);
                        alphaButtons[48].HighlightToggle(caps);
                    }
                    else
                    {
                        // No special keys were pressed. The only other possibilities are combinations from either alpha mode or bracket mode.
                        // Here, we determine how many keys were pressed simultaneously to narrow down the number of checks, 
                        // and then we check the possible mappings for either alpha mode or bracket mode.

                        switch (HammingWeight((int)inputFlags))
                        {
                            case 5: // Five simultaneous keys were pressed
                                if (inputFlags.HasFlag(DOFSCIIMap.Hashtag))
                                {
                                    HandleInput("#");
                                    alphaButtons[43].Highlight();
                                }
                                else if (inputFlags.HasFlag(DOFSCIIMap.Ampersand))
                                {
                                    HandleInput("&");
                                    alphaButtons[44].Highlight();
                                }
                                else if (inputFlags.HasFlag(DOFSCIIMap.Percent))
                                {
                                    HandleInput("%");
                                    alphaButtons[45].Highlight();
                                }
                                else if (inputFlags.HasFlag(DOFSCIIMap.DollarSign))
                                {
                                    HandleInput("$");
                                    alphaButtons[46].Highlight();
                                }
                                else if (inputFlags.HasFlag(DOFSCIIMap.Caret))
                                {
                                    HandleInput("^");
                                    alphaButtons[47].Highlight();
                                } 
                                break;
                            case 4: // Four simultaneous keys were pressed
                                if (currentKeyMode == KeyMode.Alphanumeric)
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.Z))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_Z, "z");
                                        alphaButtons[25].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.X))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_X, "x");
                                        alphaButtons[23].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Semicolon))
                                    {
                                        HandleInput(";");
                                        alphaButtons[35].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Colon))
                                    {
                                        HandleInput(":");
                                        alphaButtons[36].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Period))
                                    {
                                        HandleInput(".");
                                        alphaButtons[26].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Quote))
                                    {
                                        HandleInput("\"");
                                        alphaButtons[37].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.QuestionMark))
                                    {
                                        HandleInput("?");
                                        alphaButtons[28].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Exclamation))
                                    {
                                        HandleInput("!");
                                        alphaButtons[38].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Tilde))
                                    {
                                        HandleInput("~");
                                        alphaButtons[40].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Apostrophe))
                                    {
                                        HandleInput("'");
                                        alphaButtons[41].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Comma))
                                    {
                                        HandleInput(",");
                                        alphaButtons[27].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Underscore))
                                    {
                                        HandleInput("_");
                                        alphaButtons[39].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.AccentGrave))
                                    {
                                        HandleInput("`");
                                        alphaButtons[42].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.At))
                                    {
                                        HandleInput("@");
                                        alphaButtons[34].Highlight();
                                    }
                                }
                                else
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.WindowsKey))
                                    {
                                        currentKeyMode = KeyMode.Alphanumeric;
                                        InputSimulator.SimulateKeyPress(VirtualKeyCode.LWIN);
                                        bracketButtons[19].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.BrowserBack))
                                    {
                                        InputSimulator.SimulateKeyPress(VirtualKeyCode.BROWSER_BACK);
                                        bracketButtons[7].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.BrowserForward))
                                    {
                                        InputSimulator.SimulateKeyPress(VirtualKeyCode.BROWSER_FORWARD);
                                        bracketButtons[17].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.VolumeDown))
                                    {
                                        InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_DOWN);
                                        bracketButtons[8].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.VolumeUp))
                                    {
                                        InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_UP);
                                        bracketButtons[18].Highlight();
                                    }
                                }
                                break;
                            case 3: // Three simultaneous keys were pressed
                                if (currentKeyMode == KeyMode.Alphanumeric)
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.B))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_B, "b");
                                        alphaButtons[1].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.P))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_P, "p");
                                        alphaButtons[15].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.V))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_V, "v");
                                        alphaButtons[21].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.K))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_K, "k");
                                        alphaButtons[10].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.J))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_J, "j");
                                        alphaButtons[9].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Q))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_Q, "q");
                                        alphaButtons[16].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Zero))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD0, "0");
                                        alphaButtons[50].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.One))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD1, "1");
                                        alphaButtons[51].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Two))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD2, "2");
                                        alphaButtons[52].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Three))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD3, "3");
                                        alphaButtons[53].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Four))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD4, "4");
                                        alphaButtons[54].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Five))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD5, "5");
                                        alphaButtons[55].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Six))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD6, "6");
                                        alphaButtons[56].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Seven))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD7, "7");
                                        alphaButtons[57].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Eight))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD8, "8");
                                        alphaButtons[58].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Nine))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.NUMPAD9, "9");
                                        alphaButtons[19].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Plus))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.ADD, "+");
                                        alphaButtons[30].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Minus))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.SUBTRACT, "-");
                                        alphaButtons[31].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Multiply))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.MULTIPLY, "*");
                                        alphaButtons[32].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Divide))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.DIVIDE, "/");
                                        alphaButtons[33].Highlight();
                                    }
                                }
                                else
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.Undo))
                                    {
                                        InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Z);
                                        bracketButtons[6].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Redo))
                                    {
                                        InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Y);
                                        bracketButtons[16].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Cut))
                                    {
                                        InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_X);
                                        bracketButtons[9].Highlight();
                                    }
                                }
                                
                                break;
                            case 2: // Two simultaneous keys were pressed
                                if (currentKeyMode == KeyMode.Bracket)
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.OpenStraightBracket))
                                    {
                                        HandleInput("[");
                                        bracketButtons[3].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.CloseStraightBracket))
                                    {
                                        HandleInput("]");
                                        bracketButtons[13].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.StraightSlash))
                                    {
                                        HandleInput("|");
                                        bracketButtons[4].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.BackSlash))
                                    {
                                        HandleInput("\\");
                                        bracketButtons[14].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Copy))
                                    {
                                        InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                                        bracketButtons[5].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Paste))
                                    {
                                        InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                                        bracketButtons[15].Highlight();
                                    }
                                }
                                else
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.F))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_F, "f");
                                        alphaButtons[6].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.N))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_N, "n");
                                        alphaButtons[13].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.D))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_D, "d");
                                        alphaButtons[3].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.C))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_C, "c");
                                        alphaButtons[2].Highlight();
                                    } 
                                    else if (inputFlags.HasFlag(DOFSCIIMap.R))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_R, "r");
                                        alphaButtons[17].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.S))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_S, "s");
                                        alphaButtons[18].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.T))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_T, "t");
                                        alphaButtons[19].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.H))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_H, "h");
                                        alphaButtons[7].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.L))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_L, "l");
                                        alphaButtons[11].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.M))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_M, "m");
                                        alphaButtons[12].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.W))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_W, "w");
                                        alphaButtons[22].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.G))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_G, "g");
                                        alphaButtons[6].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Space))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.SPACE, " ");
                                        alphaButtons[29].Highlight();
                                    }
                                }
                                break;
                            case 1: // Only one key was pressed
                                if (currentKeyMode == KeyMode.Alphanumeric)
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.A))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_A, "a");
                                        alphaButtons[0].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.E))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_E, "e");
                                        alphaButtons[4].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.I))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_I, "i");
                                        alphaButtons[8].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.O))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_O, "o");
                                        alphaButtons[14].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.U))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_U, "u");
                                        alphaButtons[20].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Y))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.VK_Y, "y");
                                        alphaButtons[24].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Newline))
                                    {
                                        HandleAlphaKey(VirtualKeyCode.RETURN, "\n");
                                    } 
                                }
                                else
                                {
                                    if (inputFlags.HasFlag(DOFSCIIMap.OpenParinthesis))
                                    {
                                        HandleInput("(");
                                        bracketButtons[2].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.CloseParinthesis))
                                    {
                                        HandleInput(")");
                                        bracketButtons[12].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.OpenAngleBracket))
                                    {
                                        HandleInput("<");
                                        bracketButtons[1].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.CloseAngleBracket))
                                    {
                                        HandleInput(">");
                                        bracketButtons[11].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.OpenSquigglyBracket))
                                    {
                                        HandleInput("{");
                                        bracketButtons[0].Highlight();
                                    }
                                    else if (inputFlags.HasFlag(DOFSCIIMap.CloseSquigglyBracket))
                                    {
                                        HandleInput("}");
                                        bracketButtons[10].Highlight();
                                    } 
                                    else if (inputFlags.HasFlag(DOFSCIIMap.Newline)) HandleAlphaKey(VirtualKeyCode.RETURN, "\n");
                                }

                                break;
                        }

                        if (outputToDisplayText && displayText != null)
                            displayText.text += newCharacter;
                    }

                    StopCoroutine(combinationCheckLoop);
                    combinationCheckLoop = null;
                }
            }
                
            inputFlags = 0;
        }

        void HandleInput(string key)
        {
            InputSimulator.SimulateTextEntry(key);
            newCharacter = (caps) ? key.ToUpper() : key;
        }

        void HandleAlphaKey(VirtualKeyCode code, string key)
        {
            InputSimulator.SimulateKeyPress(code);
            newCharacter = (caps) ? key.ToUpper() : key;
        }

        string[] relevantUncappedKeys = new string[] { ";", ".", ",", "/", "=", "`", "[","]","\\" };
        
        bool KeyNeedsToBeUncapped(string key)
        {
            foreach (string relevantKey in relevantUncappedKeys)
                if (relevantKey == key) return true;
            return false;
        }

        string[] relevantCappedKeys = new string[] {
            ":", ">", "<", "?", "_", "~", "!", "@", "#", "$", "%", "^", "&", "(", ")","\"","{","}","|"
        };

        bool KeyNeedsToBeCapped(string key)
        {
            foreach (string relevantKey in relevantCappedKeys)
                if (relevantKey == key) return true;
            return false;
        }
        
        /// <summary>
        /// Returns the number of 1s in a binary set.
        /// Use this to determine how many buttons were pressed during the check loop 
        /// so we don't have to check all the possibilities every time a check loop finishes.
        /// </summary>
        public int HammingWeight(int value)
        {
            value = value - ((value >> 1) & 0x55555555);
            value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
            return (((value + (value >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        /// <summary>
        /// The check loop exists simply to await keystrokes that were intended as simultaneous.
        /// If a second keystroke happens soon enough after the first, it will reset timeCheckLoopBegan 
        /// and allow the loop to continue waiting.
        /// Assign the checkLoopDuration at a small fraction of a second for proper performance.
        /// If duration is too long, some keystrokes intended as separate will be interpreted as simultaneous.
        /// If duration is too short, some keystrokes intended as simultaneous will be interpreted as separate.
        /// </summary>
        IEnumerator CheckLoop()
        {
            yield return new WaitUntil(() => Time.time > timeCheckLoopBegan + checkLoopDuration);

            EndCheckLoop();
        }

        /// <summary>
        /// Allow rapid repeat of arrow key press if the direction is held
        /// </summary>
        /// <param name="right">Is the right direction the one in question? If false, it's left.</param>
        /// <param name="rightHand">Is the right hand the one in question? If false, it's left.</param>
        IEnumerator CheckXArrowStatus(bool right, bool rightHand)
        {
            yield return new WaitForSeconds(0.5f);

            while ((rightHand) ? Input.GetAxis("xAxisRight") != 0 : Input.GetAxis("xAxisLeft") != 0)
            {
                if (caps)
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.SHIFT, (right) ? VirtualKeyCode.RIGHT : VirtualKeyCode.LEFT);
                else InputSimulator.SimulateKeyPress((right) ? VirtualKeyCode.RIGHT : VirtualKeyCode.LEFT);
                yield return new WaitForSeconds(0.05f);
            }
            arrowStatusCheck = null;
        }

        /// <summary>
        /// Allow rapid repeat of Backspace if the Backspace command is held.
        /// </summary>
        IEnumerator CheckBackspaceStatus()
        {
            yield return new WaitForSeconds(0.5f);

            while (Input.GetButton("forceClickLeft"))
            {
                if (displayText != null && displayText.text.Length > 0)
                    displayText.text = displayText.text.Substring(0, displayText.text.Length - 1);

                InputSimulator.SimulateKeyPress(VirtualKeyCode.BACK);
                yield return new WaitForSeconds(0.05f);
            }
            backspaceStatusCheck = null;
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
        protected override void OnMainRightDown() { if (dofsciiOn) InputDownEventHelper("main right", DOFSCIIMap.mainRight); }
        protected override void OnMainLeftDown() { if (dofsciiOn) InputDownEventHelper("main left", DOFSCIIMap.mainLeft); }
        protected override void OnForceClickLeftDown() {
            
            InputDownEventHelper("left thumb click", DOFSCIIMap.forceClickLeft);
            if (backspaceStatusCheck == null)
            {
                backspaceStatusCheck = CheckBackspaceStatus();
                StartCoroutine(backspaceStatusCheck);
            }
        }
        protected override void OnForceClickRightDown() { InputDownEventHelper("right thumb click", DOFSCIIMap.forceClickRight); }
        protected override void OnGripLeftDown() { if (dofsciiOn) InputDownEventHelper("left grip", DOFSCIIMap.gripLeft); }
        protected override void OnGripRightDown() { if (dofsciiOn) InputDownEventHelper("right grip", DOFSCIIMap.gripRight); }
        protected override void OnTriggerLeftDown() { if (dofsciiOn) InputDownEventHelper("left trigger", DOFSCIIMap.triggerLeft); }
        protected override void OnTriggerRightDown() { if (dofsciiOn) InputDownEventHelper("right trigger", DOFSCIIMap.triggerRight); }

        void BeginXArrowStatusCheck(bool right, bool rightHand)
        {
            if (arrowStatusCheck == null)
            {
                arrowStatusCheck = CheckXArrowStatus(right, rightHand);
                StartCoroutine(arrowStatusCheck);
            }
        }

        protected override void LeftThumbXActive(bool right)
        {
            if (dofsciiOn)
            {
                if (right) InputDownEventHelper("thumb move right", DOFSCIIMap.thumbMoveRight);
                else InputDownEventHelper("thumb move left", DOFSCIIMap.thumbMoveLeft);
                BeginXArrowStatusCheck(right, false);
            }
        }
        protected override void RightThumbXActive(bool right)
        {
            if (dofsciiOn)
            {
                if (right) InputDownEventHelper("thumb move right", DOFSCIIMap.thumbMoveRight);
                else InputDownEventHelper("thumb move left", DOFSCIIMap.thumbMoveLeft);
                BeginXArrowStatusCheck(right, true);
            }
        }
        protected override void LeftThumbYActive(bool up)
        {
            if (dofsciiOn)
            {
                if (up) InputDownEventHelper("thumb move up", DOFSCIIMap.thumbMoveUp);
                else InputDownEventHelper("thumb move down", DOFSCIIMap.thumbMoveDown);
            }
        }
        protected override void RightThumbYActive(bool up)
        {
            if (dofsciiOn)
            {
                if (up) InputDownEventHelper("thumb move up", DOFSCIIMap.thumbMoveUp);
                else InputDownEventHelper("thumb move down", DOFSCIIMap.thumbMoveDown);
            }
        }
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
        protected override void OnMainRightUp() { if (dofsciiOn) InputUpEventHelper("main right"); }
        protected override void OnMainLeftUp() { if (dofsciiOn) InputUpEventHelper("left right"); }
        protected override void OnForceClickLeftUp() { InputUpEventHelper("left thumb click"); }
        protected override void OnForceClickRightUp() { InputUpEventHelper("right thumb click"); }
        protected override void OnGripLeftUp() { if (dofsciiOn) InputUpEventHelper("left grip"); }
        protected override void OnGripRightUp() { if (dofsciiOn) InputUpEventHelper("right grip"); }
        protected override void OnTriggerLeftUp() { if (dofsciiOn) InputUpEventHelper("left trigger"); }
        protected override void OnTriggerRightUp() { if (dofsciiOn) InputUpEventHelper("right trigger"); }
#endregion
    }


}

