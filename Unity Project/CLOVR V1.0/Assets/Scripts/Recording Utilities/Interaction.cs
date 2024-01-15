using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
namespace XRT_OVR_Grabber
{
    //A record for a controller interaction
    public class ControllerInteraction
    {

        //Section for Digital Action Data
        public InputDigitalActionData_t dataRecord = new InputDigitalActionData_t();
        public float timeCollected = 0.0f;
        public int frameCollected = 0;
        public string controllerIndex = "";
        public string controllerActionPath = "";
        public string headerActionSet = "";
        private string[] headerSet;
        private bool active = false;
        private string emptyRow = "";

        //Section for Analog Action Data
        InputAnalogActionData_t analogData = new InputAnalogActionData_t();
        float analogX = 0.0f;
        float analogY = 0.0f;
        float analogZ = 0.0f;
        float deltaAnalogX = 0.0f;
        float deltaAnalogY = 0.0f;
        float deltaAnalogZ = 0.0f;
        bool isLeft = false;
        ulong handle = 0;

        // Variable to differenciate between the two data types. 
        bool isAnalog = false; 


        public ControllerInteraction()
        {
            headerSet = new string[] { };
        }

        public bool IsActive()
        {
            return active;
        }

        public ControllerInteraction(InputDigitalActionData_t _data, float _time, int _frameCollected, string _controller)
        {
            dataRecord = _data;
            timeCollected = _time;
            controllerIndex = _controller;
            active = true;
             
        }

        public ControllerInteraction(InputDigitalActionData_t _data, float _time, int _frameCollected, string _controller, string _index)
        {
            dataRecord = _data;
            timeCollected = _time;
            controllerIndex = _controller;
            controllerActionPath = _index;
            active = true;
             
        }

        public ControllerInteraction(InputAnalogActionData_t data, float _time, int _frameCollected, string name, bool _isLeft, ulong _handle, string _controller, string _actionsetPath)
        {
            analogData = data;
            analogX = data.x;
            analogY = data.y;
            analogZ = data.z;
            isLeft = _isLeft;
            handle = _handle;
            deltaAnalogX = data.deltaX;
            deltaAnalogY = data.deltaY;
            deltaAnalogZ = data.deltaZ;
            timeCollected = _time;
            controllerIndex = _controller;
            controllerActionPath = _actionsetPath;
            isAnalog = true; 

        }

        public int GetFrameRecorded()
        {
            return (int)timeCollected;
        }

        public void SetHeaderActionSet(string[] input)
        {
            headerSet = input;
            bool first = true;
            headerActionSet = "";
            foreach (string action in input)
            {
                if (first)
                {
                    headerActionSet += action;
                    first = false;
                }
                else
                {
                    headerActionSet += "," + action;
                }
                emptyRow += ",";
            }
            emptyRow += ",,,,";
        }

        public static string PrintHeaderDefault()
        {
            string outValue = "";
            bool first = true;
            string[] input = System.Enum.GetNames(typeof(SteamVR_Input_Sources));
            foreach (string action in input)
            {
                if (first)
                {
                    outValue += action;
                    first = false;
                }
                else
                {
                    outValue += "," + action;
                }
            }
            return outValue + ",Active Origin #,Update Time,State Changed To,Current State,X,Y,Z,dX,dY,dZ";
        }

        public string PrintEmptyLine()
        {
            return emptyRow + ",,,,";
        }

        public static string PrintEmptyLineDefault()
        {
            string outValue = "";
            string[] input = System.Enum.GetNames(typeof(SteamVR_Input_Sources));
            foreach (string action in input)
            {
                outValue += ",";
            }
            return outValue + ",,,"+ ",,,,,,";
        }


        //Printing the header. 
        public string PrintHeader()
        {
            return headerActionSet + ",Active Origin #,Update Time,State Changed To,Current State"; //"Controller, Active Origin #, Update Time, State Changed To, Current State";
        }

        public string SendToString()
        {
            string outValue = "";
            if (headerActionSet.Length > 0)
            {
                foreach (string action in headerSet)
                {
                    if (controllerActionPath == action)
                    {
                        outValue += controllerIndex + ",";
                    }
                    else
                    {
                        outValue += ",";
                    }
                }
                outValue += dataRecord.activeOrigin.ToString() + "," + dataRecord.fUpdateTime.ToString() + "," + dataRecord.bChanged.ToString() + "," + dataRecord.bState.ToString();

                //Analog Interaction intersection?
                if(isAnalog)
                {
                    outValue += "," + analogX.ToString() + ","+ analogY.ToString() + "," + analogZ.ToString() + "," 
                        + deltaAnalogX.ToString() + "," + deltaAnalogY.ToString() + "," + deltaAnalogZ.ToString();
                }
                else
                {
                    outValue += ",,,,,,";
                }

                return outValue;
            }
            else
            {
                return PrintEmptyLineDefault();
            }
        }
    }

    public class AnalogInteraction
    {
        InputAnalogActionData_t analogData = new InputAnalogActionData_t();
        string actionName = "";
        float analogX = 0.0f;
        float analogY = 0.0f;
        float analogZ = 0.0f;
        float deltaAnalogX = 0.0f;
        float deltaAnalogY = 0.0f;
        float deltaAnalogZ = 0.0f;


        bool isLeft = false;
        ulong handle = 0; 

        public AnalogInteraction()
        {

        }
        public AnalogInteraction(InputAnalogActionData_t data, string name, bool _isLeft, ulong _handle)
        {
            analogData = data;
            actionName = name;
            analogX = data.x;
            analogY = data.y;
            analogZ = data.z;
            isLeft = _isLeft;
            handle = _handle;
            deltaAnalogX = data.deltaX;
            deltaAnalogY = data.deltaY;
            deltaAnalogZ = data.deltaZ;
        }

        public static string PrintHeader()
        {
            return ""; 
        }

        public string SendToString()
        {
            string s = "";


            return s;
        }


    }


    public class InteractionPackager: MonoBehaviour
    {
        public List<Interaction> interactions;

        private void Awake()
        {
            interactions = new List<Interaction>(); 
        }

        public void AddInteractionRecord(InputDigitalActionData_t _data, float _time, int _frameCollected, string _controller, string _index, string[] _headerSet)
        {
            interactions.Add(new Interaction(_data, _time, _frameCollected, _controller, _index, _headerSet));
        }

        public void ClearRecords()
        {
            interactions.Clear(); 
        }

        public string ExportToString()
        {
            string outString = ""; 
            foreach (Interaction i in interactions)
            {

            }
            return outString; 
        }
    }

    //Interaction structure. 
    public struct Interaction
    {
        public InputDigitalActionData_t dataRecord;
        public float timeCollected;
        public int frameCollected;
        public string controllerIndex;
        public string controllerActionPath;
        public string[] headerSet;
        public bool active;

        public Interaction(InputDigitalActionData_t _data, float _time, int _frameCollected, string _controller, string _index, string[] _headerSet)
        {
            dataRecord = _data;
            timeCollected = _time;
            frameCollected = _frameCollected;
            controllerIndex = _controller;
            controllerActionPath = _index;
            active = true;
            headerSet = _headerSet;
        }

        public string SendToString()
        {
            string outValue = "";
            if (headerSet.Length > 0)
            {
                foreach (string action in headerSet)
                {
                    if (controllerActionPath == action)
                    {
                        outValue += controllerIndex + ",";
                    }
                    else
                    {
                        outValue += ",";
                    }
                }

                outValue += dataRecord.activeOrigin.ToString() + "," + dataRecord.fUpdateTime.ToString() + "," + dataRecord.bChanged.ToString() + "," + dataRecord.bState.ToString();
                return outValue;
            }
            else
            {
                string[] input = System.Enum.GetNames(typeof(SteamVR_Input_Sources));
                foreach (string action in input)
                {
                    outValue += ",";
                }
                return outValue + ",,,";
            }
        }

    }




}