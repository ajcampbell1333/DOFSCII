# DOFSCII
ASCII Keyboard Input for 6DOF Controllers
To run DOFSCII:

Open the "Main" scene and press play. Observe the keystroke mapping chevrons on screen to learn how to execute all the keystrokes. They should each highlight when you execute the shown button combo.

* On all platforms, trigger is the index finger.
* On all platforms, grip button is the ring finger.
* On Oculus Touch, Left Thumb is mapped to Y, and Right Thumb is mapped to B.
* On Vive, thumbs are mapped to the Menu Button.
* This is the current OpenVR default in Unity. See https://docs.unity3d.com/Manual/OpenVRControllers.html for more detail.

The Main scene runs in the background on Windows, so you can use it while in another app (Notepad, internet browser, etc.). 

You can build the Main scene as an executable for Windows Standalone if you'd like to use DOFSCII without having to open Unity.

NOTE: DOFSCII relies upon the OpenVR standard. Ensure the following settings are set correctly in Edit -> Project Settings -> Player Settings -> XR Settings:

1) "Virtual Reality Supported" is checked on.
2) OpenVR is set as the first SDK in the list of available Virtual Reality SDKs. 


To import DOFSCII into any new Unity project:

1) Copy the DOFSCII folder and paste it into the Assets directory of the new project.
2) Copy the InputManager.asset file from ProjectSettings and paste->replace it in the ProjectSettings directory of the new project.

NOTE: After importing DOFSCII into your new project, you may find errors regarding "inputFlag.HasFlag". This is because the "HasFlag" function is only available in .NET 4.0 and above. If you're getting these errors, go into Edit -> Project Settings -> Player -> Other Settings -> Configuration -> Scripting Runtime Version and switch the dropdown to anything above .NET 4.0. Unity will prompt you to restart the project. When it's done restarting, the "HasFlag" errors should be gone.

NOTE: Paste->replacing your InputManager.asset file will eliminate any custom changes you've made in Edit -> Project Settings -> Input beforehand. If you want to retain older custom input changes, you'll need to merge the InputManager.asset file.
