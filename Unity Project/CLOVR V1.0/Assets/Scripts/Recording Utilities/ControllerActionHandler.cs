using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRT_OVR_Grabber;
using UnityEngine.UI; 

using UnityEngine.Events;
namespace XRT_OVR_Grabber
{
    public class ControllerActionHandler : MonoBehaviour
    {
        XRT_OVR_Grabber.SteamVR2Input controllerWhisperer;
        [SerializeField]
        bool isRightHandController = false;
        UnityAction _handAction;
        Collider ownCollider;

        private void OnCollisionStay(Collision collision)
        {
            var g = collision.gameObject;

            if (g.CompareTag("QuestionnaireButton"))
            {
                var buttonObj = collision.gameObject.GetComponent<UIAnswerButton>();
                if (controllerWhisperer.rightController && isRightHandController)
                {
                    controllerWhisperer.rightController = false;

                    //If enabled then action on this. 
                    buttonObj.ButtonAction();
                }
                if (controllerWhisperer.leftController && !isRightHandController)
                {
                    controllerWhisperer.leftController = false;

                    //If enabled then action on this. 
                    buttonObj.ButtonAction();
                }
            }else if (g.CompareTag("QuestionnaireButton"))
            {
                var buttonObject = g.GetComponent<Button>(); 

                if (controllerWhisperer.rightController && isRightHandController)
                {
                    controllerWhisperer.rightController = false;

                    //If enabled then action on this. 
                    buttonObject.onClick.Invoke();
                }
                if (controllerWhisperer.leftController && !isRightHandController)
                {
                    controllerWhisperer.leftController = false;

                    //If enabled then action on this. 
                    buttonObject.onClick.Invoke();
                }
            }          
        }
        private void Awake()
        {
            controllerWhisperer = GameObject.Find("LoggingManger").GetComponent<XRT_OVR_Grabber.SteamVR2Input>();
        }
    }
}