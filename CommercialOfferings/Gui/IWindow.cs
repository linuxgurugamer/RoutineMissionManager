using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    interface IWindow
    {
        int Id { get; set; }
        WindowManager Manager { get; set; }
        IWindow ParentWindow { get; set; }
        List<IWindow> ChildWindows { get; set; }
        void OnGUI();
        Rect WindowPosition { get; set; }
        void InitializeRelativeWindowPosition(Rect parentRec);
    }
}
