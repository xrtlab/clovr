using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonTransition : MonoBehaviour
{
    public Color32 _NormalColor = Color.white;
    public Color32 _HoverColor = Color.gray;
    public Color32 _SelectionColor = Color.green;
    public Image _image = null;

    private void Awake()
    {
        _image = GetComponent<Image>(); 
    }

    public void SelectionHover()
    {
        _image.color = _HoverColor;
    }

    public void SelectionDown()
    {
        _image.color = _SelectionColor;
    }

    public void SelectionUp()
    {
        _image.color = _NormalColor; 
    }
}
