//MIT License

//Copyright (c) 2017 Ben Otter

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
using System.Collections;
using UnityEngine;
using Valve.VR;


public partial class OVR_Overlay 
{
    protected Vector2 _mousePos = new Vector2();
    public Vector2 overlayMousePosition { get { return _mousePos; } }

    protected bool _mouseLeftDown = false;
    public bool overlayMouseLeftDown { get { return _mouseLeftDown; } }

    protected bool _mouseRightDown = false;
    public bool overlayMouseRightDown { get { return _mouseRightDown; } }

    protected void UpdateMouseData(VREvent_Mouse_t mD)
    {
        _mousePos.x = mD.x;
        _mousePos.y = mD.y;
    }
    protected void UpdateMouseData(VREvent_Mouse_t mD, bool state)
    {
        UpdateMouseData(mD);

        switch((EVRMouseButton) mD.button)
        {
            case EVRMouseButton.Left:
                _mouseLeftDown = state;
            break;

            case EVRMouseButton.Right:
                _mouseRightDown = state;
            break;
        }
    }

}