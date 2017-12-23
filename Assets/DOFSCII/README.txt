To run DOFSCII:

Open the "Main" scene and press play. Observe the keystroke mapping chevrons on screen to learn how to execute all the keystrokes. They should each highlight when you execute the shown button combo.

* On all platforms, trigger is the index finger.
* On all platforms, grip button is the ring finger.
* On Oculus Touch, Left Thumb is mapped to Y, and Right Thumb is mapped to B.
* On Vive, thumbs are mapped to the Menu Button.
* This is the current OpenVR default in Unity. See https://docs.unity3d.com/Manual/OpenVRControllers.html for more detail.

NOTE: Unity's Input mappings for OpenVR appear to be unstable at this time. I've seen the current settings in InputManager working fine,
and then I fire up the same scene the following day with no changes only to find that certain buttons suddenly don't work at all.
Then, I'll try again a few hours later and those same buttons are somehow working fine again. 
The buttons that consistently give me trouble are "mainRight" and "mainLeft" which is the DOFSCII name for the Menu buttons for Vive or 
"Button.One" and "Button.Three" for Oculus Touch.
It's hard to say why InputManager is inconsistent. I have seen the mappings suddenly working for different buttons than the ones I chose. 
For example, currently I have mapped "mainLeft" to Button.One according to Unity's documentation, but
"mainLeft" now activates when I press Button.Two, not Button.One. This mapping was working as expected just days ago. 
I didn't change it, but it changed somehow.
Why are there gremlins in my copy of your engine, Unity? 
Hopefully, this will get sorted out at Unity. The community can't do anything about it without source access to the Input class.

Long story short, if you're having trouble getting any given button to map properly, just know that I have done battle with Unity's
InputManager for dozens of hours. I wish I could say the end results are reliable, but I fully expect the mappings to work differently tomorrow.

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