using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace XRT_OVR_Grabber
{
    public class OverlayPointer : MonoBehaviour
    {
        UnityAction<string> _InvokedAction;
        UnityAction _InvertControllers;
        Button lastButton;

        [SerializeField]
        public SteamVR_Input_Sources targetSources;

        [SerializeField]
        GameObject selectedObject;

        [SerializeField]
        GameObject _dot;


        // Update is called once per frame
        [SerializeField]
        float pointerDelay = 0.5f;
        float timePassed = 0.0f;
        bool pointerDisable = false;

        private void Awake()
        {
            //_lineRenderer = GetComponent<LineRenderer>();
            _InvokedAction += OnInvokePointer;
            _InvertControllers += InvertHandBinding;
            _dot.SetActive(false);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //Anti-ghosting
            if (timePassed > pointerDelay)
            {
                pointerDisable = false;
                timePassed = 0.0f;
            }
            timePassed += Time.deltaTime;
        }



        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Potato");

            GameObject g = other.gameObject;/*
            if (!g.CompareTag("InteractableButton"))
            {
                return;
            }*/
            lastButton = g.GetComponent<Button>();
            //lastButton.EvaluateAndTransitionToHighlighedStateOut();
            _dot.SetActive(true);
        }



        private void OnTriggerStay(Collider other)
        {
            //other.
         /*
            var col = collision.GetContact(0);
            _dot.transform.position = col.point;*/
        }

        private void OnTriggerExit(Collider other)
        {
            /*if (!collision.gameObject.CompareTag("InteractableButton"))
            {
                return;
            }*/
            //lastButton.EvaluateAndTransitionToSelectionStateOut();
            lastButton = null;
            _dot.SetActive(false);
        }

        public void InvertHandBinding()
        {
            if (targetSources == SteamVR_Input_Sources.LeftHand)
                targetSources = SteamVR_Input_Sources.RightHand;
            else
                targetSources = SteamVR_Input_Sources.LeftHand;

        }

        //Technically will be invoked when the button is pressed.
        public void OnInvokePointer(string actionName)
        {
            Debug.Log(actionName);
            try
            {
                if (pointerDisable)
                    return;

                if (actionName == targetSources.ToString("G") && lastButton != null)
                {
                    lastButton.onClick.Invoke();
                    pointerDisable = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnEnable()
        {
            XRT_OVR_Grabber.QuestionnaireEvents.TriggerAction.AddListener(_InvokedAction);
            XRT_OVR_Grabber.QuestionnaireEvents.InvertControllerBiding.AddListener(_InvertControllers);
        }

        private void OnDisable()
        {
            XRT_OVR_Grabber.QuestionnaireEvents.TriggerAction.RemoveListener(_InvokedAction);
            XRT_OVR_Grabber.QuestionnaireEvents.InvertControllerBiding.RemoveListener(_InvertControllers);
        }
    }
}