using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace XRT_OVR_Grabber
{
    public class SteamVRInteractionsReader : MonoBehaviour
    {
        bool initialized = false;
        [SerializeField]
        LoggingManagerAPI loggerManager;
        //Handles the interactions and puts them into a queue. 
        Queue interactionQueue = new Queue();
        Queue skeletonHands = new Queue();
        SkeletonHandData skeletonData;

        /* A very special note for bindings: Bindings are included with the running application or externally
         * defined. Unfortunately I thought that OpenVR provides some sort of way of getting to them but...
         * 
         * https://github.com/ValveSoftware/steamvr_unity_plugin/blob/c9d9539f00090328854f1d93bd577d02c4f7062f/Assets/SteamVR/Input/SteamVR_Input.cs#L1505
         * 
         * Looking at the code that defines where to get the "actions" from, it seems that this only points to locally
         * sourced bindings for SteamVR, which would by default come prepackaged with SteamVR.
         * 
         * I believe that SteamVR inside the SteamVR runtime could be accessed but this could be a stretch in 
         * getting the correct name or cuerrently running bindings folder. 
         */

        //Some OpenVR Specific memory locations
        private static uint activeActionSetSize = 0;
        private static uint skeletalActionData_size = 0;
        private static uint digitalActionData_size = 0;
        private static uint analogActionData_size = 0;

        public EVRSkeletalMotionRange rangeOfMotion;
        public EVRSkeletalTransformSpace skeletalTransformSpace;

        SteamVRActions ActionDataList;
        private List<DeviceActionSet> ActionSetList = new List<DeviceActionSet>();
        private VRActiveActionSet_t[] rawActiveActionSetArray;

        /// <summary>
        /// Initiates the connection between SteamVR and this script. 
        /// </summary>
        /// <returns></returns>
        bool InitiateSteamVRConnection()
        {
            bool init = false;

            //Begin by loading the bindings.
            string fullPath = "";

            //Default loading scheme if path is provided. 
            if (loggerManager.openVRRuntimeConfig != "")
            {
                fullPath = loggerManager.openVRRuntimeConfig;

                if (!fullPath.Contains("actions.json"))
                {
                    fullPath = System.IO.Path.Combine(fullPath, "actions.json");
                }
            }
            else
                fullPath = Path.Combine(Application.streamingAssetsPath, "Config/actions.json");
            
            
            /*
            //If no path was found or given, attempt to use 
            if (fullPath == "" || loggerManager.useDefaultSteamVRBindings)
            {
                if(loggerManager.useDefaultSteamVRBindings)
                {
                    string currDrive = System.IO.Path.GetPathRoot(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System));
                    var location = currDrive + "\\Program Files (x86)\\Steam\\steamapps\\common\\SteamVR\\config";

                    if (System.IO.Directory.Exists(location))
                    {
                        fullPath = location;
                    }
                    else //In case the user decided to not install in the usual location...
                    {
                        fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Config/actions.json");
                    }
                }
                else //Default location
                {
                    fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Config/actions.json");  
                }
            }*/

            // Next Check if OpenVR Exists, if it doesn't something wrong happened
            // and steamVR runtime does not exist.
            if(OpenVR.Input == null)
            {
                Debug.LogError("Error with OpenVR initialization");
                return false;
            }

            // Setup SteamVR memory pointers.
            if (activeActionSetSize == 0)
                activeActionSetSize = (uint)(Marshal.SizeOf(typeof(VRActiveActionSet_t)));
            if (skeletalActionData_size == 0)
                skeletalActionData_size = (uint)Marshal.SizeOf(typeof(InputSkeletalActionData_t));
            if (digitalActionData_size == 0)
                digitalActionData_size = (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t));
            if (analogActionData_size == 0)
                analogActionData_size = (uint)Marshal.SizeOf(typeof(InputAnalogActionData_t));

            rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
            skeletalTransformSpace = EVRSkeletalTransformSpace.Parent;


            // Set SteamVR  action manifests

            Debug.Log(fullPath);
            var err = OpenVR.Input.SetActionManifestPath(fullPath);
            if (err != EVRInputError.None)
            {
                Debug.LogError($"<b>[SteamVR]</b> Error loading action manifest into SteamVR: {err}");
                return false; 
            }


            // Get SteamVR ActionHandle -> pointer
            var json = System.IO.File.ReadAllText(fullPath);
            ActionDataList = JsonUtility.FromJson<SteamVRActions>(json);

            // Get SteamVR ActionSetHandle -> pointer
            foreach (var action in ActionDataList.actions)
            {
                err = OpenVR.Input.GetActionHandle(action.name, ref action.handle);
                if (err != EVRInputError.None)
                {
                    Debug.LogError($"<b>[SteamVR]</b> GetActionHandle error ({action.name}): {err}");
                    return false;
                }
            }

            // Load SteamVR device types. 
            var actionSetPath = ActionDataList.action_sets.First().name;
            ulong actionSetHandle = 0;
            err = OpenVR.Input.GetActionSetHandle(actionSetPath, ref actionSetHandle);
            if (err != EVRInputError.None)
                Debug.LogError($"<b>[SteamVR]</b> GetActionSetHandle error ({actionSetPath}): {err}");


            // Determine which Input Source Handles would work
            var inputSourceNames = System.Enum.GetNames(typeof(SteamVR_Input_Sources));
            foreach (var inputSourceName in inputSourceNames)
            {
                ulong inputSourceHandle = 0;
                var inputSourcePath = GetPath(inputSourceName); // Any,LeftHand,RightHand,...
                err = OpenVR.Input.GetInputSourceHandle(inputSourcePath, ref inputSourceHandle);
                if (err != EVRInputError.None)
                    Debug.LogError($"<b>[SteamVR]</b> GetInputSourceHandle error ({inputSourcePath}): {err}");
                else
                {
                    ActionSetList.Add(new DeviceActionSet
                    {
                        actionSetPath = inputSourceName,
                        ulActionSet = actionSetHandle,
                        ulRestrictedToDevice = inputSourceHandle,
                        InputSourcePath = inputSourcePath,
                        IsLeft = inputSourcePath.Contains("left") != true,
                    });
                }
            }

            // Setup the raw active action array
            rawActiveActionSetArray = ActionSetList.Select(d => new VRActiveActionSet_t
            {
                ulActionSet = d.ulActionSet,
                nPriority = 0,
                ulRestrictedToDevice = d.ulRestrictedToDevice
            }).ToArray();

            //Done. 
            return true; 
        }

        //A developer-side variable. 
        [SerializeField]
        bool debugLogging = false;


        void RecordCycle()
        {
            //Checkpoint.
            if (!initialized)
            {
                initialized = InitiateSteamVRConnection();
            }

            //Collects the Updates Action State
            var err = OpenVR.Input.UpdateActionState(rawActiveActionSetArray, activeActionSetSize);
            if (err != EVRInputError.None)
            {
                Debug.LogError($"<b>[SteamVR]</b> UpdateActionState error: {err}");
                return;
            }


            //string record = "";
            int countActionSet = 0;
            //Scroll through all data inputs. 
            foreach (var actionset in ActionSetList)
            {
                if (!keepIndexList.Contains(countActionSet))
                {
                    countActionSet++;
                    continue;
                }
                countActionSet++; 
                foreach (var action in ActionDataList.actions)
                {
                    //record += actionset.actionSetPath + "_" + actionset.ulRestrictedToDevice + "_" + action.name + "\n";
                    //Debug.Log(action.type + "_" + action.name);

                    switch (action.type)
                    {
                        case "boolean":
                            if (firstPass)
                                analogDigitalPattern += "d";
                            if (actionPatternSetFirstPass)
                                actionNamePattern.Add(action.ShortName);
                            err = OpenVR.Input.GetDigitalActionData(action.handle, ref action.digitalActionData, digitalActionData_size, actionset.ulRestrictedToDevice);
                            if (err != EVRInputError.None)
                            {
                                Debug.LogWarning($"<b>[SteamVR]</b> GetDigitalActionData error ({action.name}): {err} handle: {action.handle}");
                                continue;
                            }

                            //Only record if a change was effected in the device state.
                            if (action.digitalActionData.bChanged)
                            {
                                if (action.handle == 0)
                                {
                                    continue;
                                }

                                if(debugLogging)
                                    Debug.Log($"<b>[SteamVR]</b> GetDigitalActionData IsKeyDown ({action.name}): {err} handle: {action.handle} : {action.ShortName}");
                                //Creates and records a digital interaction.
                                var realName = action.name.Split("/");
                                var nameOut = realName[realName.Length - 1];
                                var interaction = new ControllerInteraction(action.digitalActionData, 
                                    Time.realtimeSinceStartup, 
                                    Time.frameCount, 
                                    nameOut, 
                                    actionset.actionSetPath);
                                interaction.SetHeaderActionSet(System.Enum.GetNames(typeof(SteamVR_Input_Sources)));
                                //interaction.SetNumOfActions(keepIndexList.Count);
                                interaction.SetActionsDigitalAnalog(analogDigitalPattern, deviceTypesPattern, actionNamePattern);
                                interactionQueue.Enqueue(interaction);

                            }
                            else
                            {
                                var interaction = new ControllerInteraction(false);
                                interaction.SetActionsDigitalAnalog(analogDigitalPattern, deviceTypesPattern, actionNamePattern);
                                interactionQueue.Enqueue(interaction);
                            }

                            break;
                        case "vector1" or "vector2" or "vector3":
                            if (firstPass)
                                analogDigitalPattern += "a";
                            if (actionPatternSetFirstPass)
                                actionNamePattern.Add(action.ShortName);
                            //Adquires and validates analog data. 
                            err = OpenVR.Input.GetAnalogActionData(action.handle, ref action.analogActionData, analogActionData_size, actionset.ulRestrictedToDevice);
                            if (err != EVRInputError.None)
                            {
                                Debug.LogWarning($"<b>[SteamVR]</b> GetAnalogActionData error ({action.name}): {err} handle: {action.handle}");
                                continue;
                            }


                            if (action.handle == 0)
                            {
                                continue;
                            }

                            if (debugLogging)
                                Debug.Log($"<b>[SteamVR]</b> GetAnalogData IsKeyDown ({action.name}): {err} handle: {action.handle} : {action.ShortName}");
                            //Checks if the analog vector has had any action and creates an analog interaction data struct. 
                            var axis = new Vector3(action.analogActionData.x, action.analogActionData.y, action.analogActionData.z);
                            if (axis != Vector3.zero)
                            {

                                var realName = action.name.Split("/");
                                var nameOut = realName[realName.Length - 1];
                                //Composition of an analog interaction.
                                ControllerInteraction analogInteraction = new ControllerInteraction(
                                    action.analogActionData,
                                    Time.realtimeSinceStartup,
                                    Time.frameCount,
                                    action.name,
                                    actionset.IsLeft,
                                    action.handle,
                                    nameOut,
                                    actionset.actionSetPath
                                );
                                analogInteraction.SetHeaderActionSet(System.Enum.GetNames(typeof(SteamVR_Input_Sources)));
                                analogInteraction.SetActionsDigitalAnalog(analogDigitalPattern, deviceTypesPattern, actionNamePattern);
                                interactionQueue.Enqueue(analogInteraction);
                            }
                            else
                            {
                                var interaction = new ControllerInteraction(true);
                                interaction.SetActionsDigitalAnalog(analogDigitalPattern, deviceTypesPattern, actionNamePattern);
                                interactionQueue.Enqueue(interaction);
                            }
                            break;
                        //Disabling until we get around to supporting this.
                        //case "skeleton":
                        //    // Checks and loads skeleton data. 
                        //    var tempBoneTransforms = new VRBoneTransform_t[31];
                        //    err = OpenVR.Input.GetSkeletalBoneData(action.handle, skeletalTransformSpace, rangeOfMotion, tempBoneTransforms);
                        //    //OpenVR.Input.GetSkeletalSummaryData()
                        //    //VRSkeletalSummaryData_t

                        //    if (err != EVRInputError.None)
                        //    {
                        //        Debug.LogWarning($"<b>[SteamVR]</b> GetDigitalActionData error ({action.name}): {err} handle: {action.handle}");
                        //        continue;
                        //    }

                        //    //Set the data to our custom data structure thingy. 
                        //    skeletonData = new SkeletonHandData(action.name.Contains("Left") != true, tempBoneTransforms);
                        //    break;
                    }
                    

                    //Debug.Log(analogDigitalPattern);
                }
                if(devicePatternFirstPass)
                {
                    deviceTypesPattern.Add(actionset.actionSetPath);
                    actionPatternSetFirstPass = false;
                }
            }

            if (firstPass)
            {
                //Debug.Log(record);
            }
            devicePatternFirstPass = false;
            firstPass = false;
        }

        bool firstPass = true;
        bool actionPatternSetFirstPass = true;
        bool devicePatternFirstPass = true;

        string analogDigitalPattern = "";
        List<string> actionNamePattern = new List<string>();
        List<string> deviceTypesPattern = new List<string>();
        //List<string> skipList = new List<string>() {""};
        //int[] keepIndexList = {0,1,2,9};
        int skipIndex = 2; 
        /*For future reference, actionsetlist remains constant unless whatever actions file that is imported changes this.
         * These are the same values used by SteamVR devices so technically this should remain consistent. 
         * Maybe if it is a serious issue, I will change how it works, but for now the pattern of the following: 
            
            Any
            LeftHand
            RightHand
            LeftFoot
            RightFoot
            LeftShoulder
            RightShoulder
            Waist
            Chest
            Head,
            GamePad,
            Camera,
            Keyboard,
            Threadmill
         */
        List<int> keepIndexList = new List<int>() { 0, 1, 2, 9 };


        /////////////Utilities
        ///
        private static string GetPath(string inputSourceEnumName)
        {
            var enumType = typeof(SteamVR_Input_Sources);
            var descType = typeof(DescriptionAttribute);
            return ((DescriptionAttribute)enumType.GetMember(inputSourceEnumName)[0].GetCustomAttributes(descType, false)[0]).Description;
        }

        ///<summary>
        ///
        /// Queues get filled up with garbage data collected prior recording. 
        /// 
        /// </summary>
        public void DumpStartingDataAndStartRecording()
        {
            interactionQueue.Clear();
        }

        /// <summary> Setting up a function for pulling information using the queue instead of the flipflop interaction. </summary> 
        public List<ControllerInteraction> GetLastInteractions()
        {
            //Returns interactions. 
            List<ControllerInteraction> outInteractions = new List<ControllerInteraction>();
            while (interactionQueue.Count > 0)
            {
                ControllerInteraction c = (ControllerInteraction)interactionQueue.Dequeue();
                outInteractions.Add(c);
            }
            return outInteractions;
        }

        /// <summary>
        /// Returns active skeleton data. Should "on paper" be both hands but I wouldn't be surprised. 
        /// </summary>
        /// <returns></returns>
        public SkeletonHandData GetSkeletonData()
        {
            //Return back the skeleton data. 
            if (skeletonData != null)
            {
                return skeletonData;
            }
            else
            {
                return new SkeletonHandData(); 
            }
        }

        /// <summary>
        /// This is a coroutined process caller for RecordCycle that can be used to call 
        /// at a rate every second. 
        /// </summary>
        [SerializeField]
        public int updateRateHz = 120; 
        IEnumerator CallRecordCycle()
        {
            float localDeltaTime = 0.0f;
            localDeltaTime = Time.time;
            while (true)
            {
                yield return new WaitForSecondsRealtime(localDeltaTime - Time.time);
                RecordCycle();
                localDeltaTime += updateRate;

                if (stopUpdateThread)
                    break;
            }

            lockProcess = false;
        }

        int rateHz = 72;
        float updateRate = 1.0f;
        int frameCollected = 0;
        bool stopUpdateThread = false;
        bool lockProcess = false;

        /// <summary>
        /// Update loop; always runs. 
        /// </summary>
        float timePassed = 0.0f;
        float rateLimit = 1.0f;
        private void Update()
        {
            //updateRateHz = loggerManager.recordRateInterval;
            updateRate = 1.0f / loggerManager.recordRateInterval;
            if (!lockProcess)
            {
                StartCoroutine(CallRecordCycle());
                lockProcess = true;
            }


            //if (timePassed > rateLimit)
            //{
            //    StartCoroutine(CallRecordCycle());
            //    timePassed = 0.0f;
            //}
            //else
            //{
            //    timePassed += Time.deltaTime;
            //    return;
            //}
        }
    }
}
