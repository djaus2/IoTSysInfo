# IoTSysInfo
A Universal Windows App that mimicks the web portal* to a Windows 10 IoT device. (Both versions now uploaded)

## Platforms:
Windows 10 IoT, Windows 10 Desktop, Windows 10 Phone(Version 2 of app)

# About
A *Windows 10 IoT* device has a *web portal* in which you can view various aspects of the running device. You can also set some aspects. The readable status items are accessed via the portal gain this access through *REST* calls from *JavaScript*. This app uses the same APIs and accesses them through *REST*. As the responses are in *JSON* format, the app **recursively** parses the respone to generate the inormation to be displayed.

# Versions
There are two versions, both using the same *REST* and *JSON* processing code. They differ on the UI interface in that the first version is simple interface meant for a standard desktop screen. The second is more complex in that the UI is devided into three sections and are subject to *State Triggers*. This vesrion is meant for smaller screens as well as the desktop as the triggers can auotmatically hide parts of the UI which can be manually trigggered for display.

# The Take Home
This app shows you how you access various system attributes of a Windows 10 IoT device from a Universal Windows App running on the device or on another Windows 10 device (IoT, Phone or Desktop). It exemplifies REST calls and recursive JSON parsing.

### Footnote:
* The web portal of a Windows 10 IoT device is accessed at http://the_device_IP_address:8080
 eg. http://192.168.0.28:8080
