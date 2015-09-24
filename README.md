# IoTSysInfo
A Universal Windows App that mimicks the web portal* to a Windows 10 IoT device. (Both versions now uploaded)

## Platforms:
Windows 10 IoT, Windows 10 Desktop, Windows 10 Phone(Version 2 of app)

# About
A *Windows 10 IoT* device has a *web portal* in which you can view various aspects of the running device. You can also set some aspects. The readable status items are accessed via the portal gain this access through *REST* calls from *JavaScript*. This app uses the same APIs and accesses them through *REST*. As the responses are in *JSON* format, the app **recursively** parses the respone to generate the inormation to be displayed.

# UI Versions
There are two UI versions of this app, both using the same *REST* and *JSON* processing code. They differ on the UI interface in that the first version is a simple interface meant for a standard desktop screen. The second is more complex in that the UI is devided into three sections and are subject to *State Triggers*. This vesrion is meant for smaller screens as well as the desktop as the triggers can auotmatically hide parts of the UI which can be manually trigggered for display.
Repository Versions 2.0 are all of the second UI format. **See VERSIONs: below**

# The Take Home
This app shows you how you access various system attributes of a Windows 10 IoT device from a Universal Windows App running on the device or on another Windows 10 device (IoT, Phone or Desktop). It exemplifies REST calls and recursive JSON parsing.

# Acknowledgement
The basis of this app (wrt REST and the Web Portal API) came for a blog by Bruce Eitman: 

* [Windows 10 IoT Core: Getting the MAC Address from Raspberry Pi.](http://www.embedded101.com/BruceEitman/entryid/676/Windows-10-IoT-Core-Getting-the-MAC-Address-from-Raspberry-Pi)
* [Windows 10 IoT Core: Stop A Running Package](http://www.embedded101.com/BruceEitman/entryid/686/windows-10-iot-core-stop-a-running-package)
* [Windows 10 IoT Core: Shutdown and Reboot the Raspberry Pi](http://www.embedded101.com/BruceEitman/entryid/685/windows-10-iot-core-shutdown-and-reboot-the-raspberry-pi)

### Footnote:
* The web portal of a Windows 10 IoT device is accessed at http://the_device_IP_address:8080
 
eg. http://192.168.0.28:8080

# VERSION: 3.0 
* A major rewrite of the UI codebehind. Now is a JSON file that list the commands and what URL to use. Can extend app simply now by added more command names and URLs

# VERSION: 3.2
* *startapp* now works: Drill into Packages and click on the Relative name of the app. It gets copied to the Params textbox. Then press startapp.
app command now fixed as well

# VERSION: 3.5
* *stopapp* now works AND *startapp* modified from Version 3.2:
* When you click on a specific package some of it's metadata gets copied to two textboxes in the Settings pane.
* When you then click on [startapp] or [stopapp] the required parameters are taken from these textboxes

# VERSION: 4.0
* **stopapp** now seeks confirmation and ForceStop option
* Added **shutdown** and **reset** system both with confirmation
* Added Confirmation Dialog for the above

# VERSION: 4.1
* Dialog boxes don't work on IoT, so in this version, the three commands that use them have Try-Catch around them, so are ignored.
* Relevant commands are stopapp, shutdown and reboot. Dialogs used for confirmation (and ForceStop option with stopapp)
* For stopapp there is a checkbox in the Settins to ForceStop if required.
* urls in the JSON file now have * at end if POST is to be used
