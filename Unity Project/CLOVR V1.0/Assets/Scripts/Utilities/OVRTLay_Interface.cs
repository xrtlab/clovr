using UnityEngine;

namespace XRT_OVR_Grabber
{
    public partial class LoggingManagerAPI : MonoBehaviour
    {
        [SerializeField]
        Unity_Overlay overlay;

        GameObject overlayGameObj;
        [SerializeField]
        public Unity_Overlay overlayPointerL;
        [SerializeField]
        public Unity_Overlay overlayPointerR;



        public void ToggleOverlayVisibility()
        {
            overlay.isVisible = !overlay.isVisible;
        }
        public void ToggleOverlayVisibility(bool toggle)
        {
            if(toggle)
            {
                overlayGameObj.transform.position = pivotSpawnLocation.transform.position;   //.SetParent(pivotSpawnLocation.transform);
                overlayGameObj.transform.rotation = pivotSpawnLocation.transform.rotation;
            }



            overlay.isVisible = toggle;
        }
        public void ToggleOverlayVisibility(bool toggle, Unity_Overlay _overlay)
        {
            _overlay.isVisible = toggle;
        }

    }
}