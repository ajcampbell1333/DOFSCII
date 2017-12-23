using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DOFSCII
{
    public enum KeyMode
    {
        Alphanumeric,
        Bracket
    }

    [Flags]
    public enum DOFSCIIMap 
    {
        // bitwise flags
        none = 0,
        mainLeft = 1 << 0,
        mainRight = 1 << 1,
        forceClickLeft = 1 << 2,
        forceClickRight = 1 << 3,
        gripLeft = 1 << 4,
        gripRight = 1 << 5,
        triggerLeft = 1 << 6,
        triggerRight = 1 << 7,
        thumbMoveLeft = 1 << 8,
        thumbMoveRight = 1 << 9,
        thumbMoveUp = 1 << 10,
        thumbMoveDown = 1 << 11,
        

        // Special chars (active for every category)
        Backspace = forceClickLeft,
        Newline = forceClickRight,
        Delete = forceClickLeft | triggerLeft,
        
        DOFSCIIToggle = forceClickLeft | forceClickRight,
        arrowLeft = thumbMoveLeft,
        arrowRight = thumbMoveRight,
        arrowUp = thumbMoveUp,
        arrowDown = thumbMoveDown,

        #region Alphanumeric mode

        // single-button combos (all six of the following are reused in bracket mode)
        A = triggerLeft,
        E = triggerRight,
        I = mainLeft,
        O = mainRight,
        U = gripLeft,
        Y = gripRight,

        // two-button combos
        F = triggerLeft | mainLeft,     // reused in bracket mode 
        N = triggerRight | mainRight,   // reused in bracket mode
        D = triggerLeft | gripLeft,     // reused in bracket mode
        C = triggerRight | gripRight,   // reused in bracket mode

        Space = triggerLeft | triggerRight,
        S = mainLeft | mainRight,
        T = gripLeft | gripRight,
        H = mainLeft | gripLeft,
        L = mainRight | gripRight,
        M = triggerRight | gripLeft,
        W = triggerLeft | gripRight,
        G = triggerLeft | mainRight,
        R = triggerRight | mainLeft,
        

        // three-button combos
        B = triggerLeft | mainLeft | gripLeft,
        P = triggerRight | mainRight | gripRight,
        V = triggerLeft | triggerRight | mainRight,
        K = triggerRight | triggerLeft | mainLeft,
        J = triggerLeft | triggerRight | gripRight,
        Q = triggerRight | triggerLeft | gripLeft,
        Zero = gripLeft | triggerLeft | mainRight,                                  // 0
        One = mainRight | triggerLeft | mainLeft,                                   // 1
        Two = mainRight | triggerRight | mainLeft,                                  // 2
        Three = mainRight | gripLeft | mainLeft,                                    // 3
        Four = mainRight | gripRight | mainLeft,                                    // 4
        Five = gripRight | mainRight | triggerLeft,                                 // 5
        Six = gripLeft | mainLeft | triggerRight,                                   // 6
        Seven = gripLeft | triggerRight | mainRight,                                // 7
        Eight = gripRight | triggerLeft | mainLeft,                                 // 8
        Nine = mainLeft | triggerRight | gripRight,                                 // 9
        Plus =  gripLeft | gripRight | triggerRight,                                // +
        Minus = gripRight | gripLeft | triggerLeft,                                 // -
        Multiply = gripRight | gripLeft | mainRight,                                // *
        Divide = gripRight | gripLeft | mainLeft,                                   // /

        // four-button combos
        Z = triggerRight | triggerLeft | mainRight | mainLeft,
        X = triggerRight | triggerLeft | gripRight | gripLeft,
        At = mainRight | mainLeft | gripRight | gripLeft,                           // @
        Semicolon = triggerLeft | triggerRight | mainRight | gripRight,             // ;
        Colon = triggerRight | triggerLeft | mainLeft | gripLeft,                   // :
        Period = mainLeft | triggerRight | mainRight |gripRight,                    // .
        Quote = mainRight | triggerLeft | mainLeft | gripLeft,                      // "
        QuestionMark = gripLeft | triggerRight | mainRight | gripRight,             // ?
        Exclamation = gripRight | triggerLeft | mainLeft | gripLeft,                // !
        Underscore = triggerLeft | mainLeft | triggerRight | gripRight,             // _
        Tilde = triggerLeft | gripLeft | triggerRight | mainRight,                  // ~
        Apostrophe = mainLeft | triggerLeft | mainRight | gripRight,                // '
        Comma = mainLeft | gripLeft | mainRight | triggerRight,                     // ,
        AccentGrave = gripLeft | mainLeft | gripRight | triggerRight,               // `
        // unused = gripLeft | triggerLeft | gripRight | mainRight,  
        

        // five-button combos
        Hashtag = mainRight | gripRight | triggerLeft | mainLeft | gripLeft,        // #
        Ampersand = triggerLeft | gripLeft | triggerRight | mainRight | gripRight,  // &
        Percent = triggerRight | gripRight | triggerLeft | mainLeft | gripLeft,     // %
        DollarSign = triggerLeft | mainLeft | triggerRight |mainRight | gripRight,  // $
        Caret = triggerRight | mainRight | triggerLeft | mainLeft | gripLeft,       // ^
        CapsToggle = mainLeft | gripLeft | triggerRight | mainRight | gripRight,

        #endregion Alpha/Punctuation category

        modeSwitch = triggerLeft | triggerRight | mainLeft | mainRight | gripLeft | gripRight,

    

        #region Bracket mode
        
        // single-button
        OpenParinthesis = triggerLeft,                      // (
        CloseParinthesis = triggerRight,                    // )
        OpenAngleBracket = mainLeft,                        // <
        CloseAngleBracket = mainRight,                      // >
        OpenSquigglyBracket = gripLeft,                     // {
        CloseSquigglyBracket = gripRight,                   // }

        // two-button combos
        OpenStraightBracket = triggerLeft | mainLeft,       // [
        CloseStraightBracket = triggerRight | mainRight,    // ]
        StraightSlash = triggerLeft | gripLeft,             // |
        BackSlash = triggerRight | gripRight,               // \
        
        WindowsKey = triggerRight | triggerLeft | gripRight | gripLeft,
        Copy = mainLeft | gripLeft,
        Paste = mainRight | gripRight,
        Undo = triggerLeft | mainLeft | gripLeft,
        Redo = triggerRight | mainRight | gripRight,
        Cut = mainLeft | gripLeft | triggerRight,
        BrowserBack = triggerLeft | mainLeft | gripLeft | triggerRight,
        BrowserForward = triggerRight | mainRight | gripRight | triggerLeft,
        VolumeDown = triggerLeft | mainLeft | gripLeft | mainRight,
        VolumeUp = triggerRight | mainRight | gripRight | mainLeft,

        #endregion Bracket Mode
    }
}


