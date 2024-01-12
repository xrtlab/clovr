## Capturing and Logging OpenVR (CLOVR)

Capturing and Logging OpenVR (CLOVR) is an application for collecting realtime data from VR equipment using OpenVR through the SteamVR runtime component. Pose and interaction data is collect in realtime as a secondary target application is being observed as a primary application. Meanwhile, CLOVR does not interface directly with an application and acts as a OpenVR overlay. Additonally, CLOVR can interface with external screen recording software such as Open Broadcasting Software (OBS) to record the in-VR session. 


## Key Features

CLOVR can perform the following: 

- Record poses and interactions from all connected VR equipment (HMD, lighthouses, controllers, trackers) as supported by SteamVR (This includes current Vive and Oculus systems)
- Support for different refresh resolutions (Dependent on VR refresh rate)
- Record image frames from the mirror VR view captured from SteamVR
- Record video using OBS as a service 
- Present standarized likert questionnaires and capture answers during recording or without recording
- Record microphone separately from OBS
- Record sessions with a timer 
- Pause recording sessions when headset is not active or on user's head 
- Sync pose recording with OBS recording
- Stand-alone process handling library to granularly control the OBS process to ensure exact start and stop of OBS

## Setup

Please take a look at the following video for setting up CLOVR.

[Super special video]


## Example Datasets (One-Shot Datasets)

We have included recordings from different applications to provide a diverse level of data that researchers can immediately use to test their CSV loading. 


## User Manual

The user manual for CLOVR can be downloaded in this repository. It is reccomended to look at the video below for a quick start version. 

[Video]

## Referencing CLOVR

CLOVR will be published in the future and will have an associated bibtex to reference from. This shall be placed here in the future. 


## License and External Libraries

This project is licensed under the **MIT license**. Some code snippets from other projects may have also been included, however they are also MIT open-source projects. 

The following are the potential sources of code:
 - Virtual Motion Capture [https://github.com/sh-akira/VirtualMotionCapture]
 - OVRLay [https://github.com/benotter/OVRLay]
 - SavWav [https://gist.github.com/darktable/2317063] 
 - OVRT [https://github.com/biosmanager/unity-openvr-tracking]
 - OpenVR [https://github.com/ValveSoftware/openvr]
 - Unity OpenVR Tracking [https://github.com/ValveSoftware/steamvr_unity_plugin]
 - Native Web Socket [https://github.com/endel/NativeWebSocket]

 ChatGPT was occasionally used for guessing or locating code API from OpenVR, however all code was written from scratch where it was used. 

## Pull Requests and Issues

Please file your pull requests and issues in their respective channels. As it might be expected that this project might wind down after publication, please submit your requests and concerns as early as possible. Code mantainers are welcomed. 

## Contribuitors

The supporting authors for this project have been the following: 
- **Esteban Segarra Martinez (University of Central Florida)** - Full development of the tool and writeup of the associated publication. 
- **Ryan P. McMahan (University of Central Florida)** - Advised and steered suggestions to improve the tool
- **Ayesha Malik (University of Central Florida)** - Wrote documentation, tested early prototypes, and edited the publication paper

## Papers that have used CLOVR

This section remains blank and to be updated in the future. 

## Supporting Grants

This project was supported by the National Science Foundation through the grant numbered 2232448 with the name **Capturing and Logging Ecological Virtual Experiences and Reality (CLEVER)**. 