using UnityEngine;
using UnityEngine.Events;

public class SteamVRNotInitialized : MonoBehaviour
{

    private UnityAction NotInitializedWarning;
    [SerializeField]
    GameObject warningPanel;
    // Start is called before the first frame update
    void Start()
    {
        NotInitializedWarning += ShowWarning;
        Unity_SteamVR_Handler.SteamVRNotInitializedEvent.AddListener(NotInitializedWarning);
    }

    void ShowWarning()
    {
        warningPanel.SetActive(true);
    }
}
