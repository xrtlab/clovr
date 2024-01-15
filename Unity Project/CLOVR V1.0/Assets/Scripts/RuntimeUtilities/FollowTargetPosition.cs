using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is for moving the overlay in front of the user. 
/// </summary>
public class FollowTargetPosition : MonoBehaviour
{
    [SerializeField]
    GameObject target;

    [SerializeField]
    GameObject offsettedPanel;

    [SerializeField]
    float forwardOffset = 1.25f; 


    // Update is called once per frame
    void Update()
    {
        transform.position = target.transform.position;
        var newEuler = target.transform.eulerAngles;
        newEuler = new Vector3(0, newEuler.y, 0);
        transform.eulerAngles = newEuler;
        //offsettedPanel.transform.position = new Vector3(0, 0, forwardOffset);
    }

    private void Start()
    {
        offsettedPanel.transform.SetParent(this.transform);
        offsettedPanel.transform.position = new Vector3(0,0,forwardOffset); 
    }
}
