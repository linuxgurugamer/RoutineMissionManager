using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class WindowManager
    {
        private static WindowManager _instance;

        private List<IWindow> _windows = new List<IWindow>();

        private const int ID_PREFIX = 88823000;

        public WindowManager()
        {
            _instance = this;
        }


        public void OnGUI()
        {
            foreach (IWindow w in _windows)
            {
                w.OnGUI();
            }
        }

        public void OpenWindow(IWindow window, IWindow parent = null)
        {
            window.Id = GetNewId();
            window.Manager = this;
            window.ChildWindows = new List<IWindow>();

            if (parent != null)
            {
                window.InitializeRelativeWindowPosition(parent.WindowPosition);
                parent.ChildWindows.Add(window);
            }

            _windows.Add(window);
        }

        public void CloseWindow(IWindow window)
        {
            CloseChildren(window);
            _windows.Remove(window);
        }

        public bool IsOpenWindow(IWindow window)
        {
            int index =  _windows.IndexOf(window);
            return (index != -1);
        }

        private void CloseChildren(IWindow window)
        {
            foreach (IWindow childWindow in window.ChildWindows)
            {
                CloseChildren(childWindow);
                _windows.Remove(childWindow);
            }
        }

        private int GetNewId()
        {
            for (int id = 0; id < 1000; id++)
            {
                bool exists = false;
                foreach (IWindow window in _windows)
                {
                    if (window.Id == id)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    return id;
                }
            }
            return 0;
        }

        public static void Open(IWindow window, IWindow parent = null)
        {
            if (_instance == null) { return; }

            _instance.OpenWindow(window, parent);
        }


        public static void Close(IWindow window)
        {
            if (_instance == null) { return; }

            _instance.CloseWindow(window);
        }

        public static bool IsOpen(IWindow window)
        {
            if (_instance == null) { return false; }

            return _instance.IsOpenWindow(window);
        }
    }
}
