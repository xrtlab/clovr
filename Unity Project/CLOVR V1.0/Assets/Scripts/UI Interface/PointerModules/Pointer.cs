/// <summary>
/// Legacy/unused code. At some point there was a pointer class but this was dropped in favor of steamVR compliant pointers that are found
/// in SteamVR overlays. 
/// 
/// 
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace XRT_OVR_Grabber
{
    public class Pointer : MonoBehaviour
    {
        [SerializeField]
        public float _raycastLength = 5.0f;

        [SerializeField]
        GameObject _dot;


        [SerializeField]
        public SteamVR_Input_Sources targetSources;

        [SerializeField]
        LineRenderer _lineRenderer = null;

        [SerializeField]
        GameObject selectedObject;

        UnityAction<string> _InvokedAction;
        UnityAction _InvertControllers;
        Button lastButton; 


        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _InvokedAction += OnInvokePointer;
            _InvertControllers += InvertHandBinding;
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

        // Update is called once per frame
        [SerializeField]
        float pointerDelay = 0.5f;
        float timePassed = 0.0f;
        bool pointerDisable = false; 
        void FixedUpdate()
        {
            UpdateLine();
            if (selectedObject != null)
                OnHoverPointer();
            if (timePassed > pointerDelay)
            {
                pointerDisable = false;
                timePassed = 0.0f;
            }
            timePassed += Time.deltaTime; 
            
        }

        private void UpdateLine()
        {
            float targetLen = _raycastLength;
            RaycastHit hit = CreateRaycast(targetLen);
            Vector3 endPosition = transform.position + (transform.forward * targetLen);
            if (hit.collider != null)
            {
                endPosition = hit.point;
                selectedObject = hit.collider.gameObject;
            }
            else
            {
                selectedObject = null;
            }
            _dot.transform.position = endPosition;
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, endPosition);

        }

        //Hover button behavior. 
        private void OnHoverPointer()
        {
            try
            {            
                if (selectedObject == null)
                {
                    if (lastButton != null)
                    {
                        //lastButton.EvaluateAndTransitionToSelectionStateOut();
                        lastButton = null;
                    }
                    return;
                }
                    
                if (selectedObject.CompareTag("InteractableButton") && lastButton == null)
                {
                    var button = selectedObject.GetComponent<Button>();
                    //button.EvaluateAndTransitionToHighlighedStateOut();
                    lastButton = button;
                }
                else if (selectedObject.CompareTag("InteractableButton"))
                {
                    //Pass since we are hoveing over the item. 
                }
                else if (!selectedObject.CompareTag("InteractableButton") && lastButton != null)
                {
                    //lastButton.EvaluateAndTransitionToSelectionStateOut();
                    lastButton = null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
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
                if (selectedObject == null)
                    return;

                if (pointerDisable)
                    return;

                if (actionName == targetSources.ToString("G") && selectedObject.CompareTag("InteractableButton"))
                {
                    var button = selectedObject.GetComponent<Button>();
                    button.onClick.Invoke();
                    pointerDisable = true; 
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        private RaycastHit CreateRaycast(float len)
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = new Ray(transform.position, transform.forward);
            Physics.Raycast(ray, out hit, _raycastLength);
            return hit;
        }
    }
}