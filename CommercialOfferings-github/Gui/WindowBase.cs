using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ClickThroughFix;

namespace CommercialOfferings.Gui
{
    class WindowBase : IWindow
    {
        public string Title;

        public bool IsCloseButtonVisible = true;

        private Rect _windowPosition;
        private int _width;
        private int _height;

        public WindowBase(string title, Rect rect, int width, int height = 0)
        {
            Title = title;
            _width = width;
            _height = height;
            _windowPosition = rect;
        }

        public virtual void Close()
        {
            if (Manager != null) { Manager.CloseWindow(this); }
        }

        private void drawWindow()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<RMM>().useAlternateSkin)
                GUI.skin = HighLogic.Skin;
            _windowPosition = ClickThruBlocker.GUILayoutWindow(Id, _windowPosition, Window, Title, RmmStyle.Instance.WindowStyle);
        }

        public virtual void ChildWindowOnGUI()
        {
        }

        private void Window(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, _windowPosition.width - 25, 25));
            if (IsCloseButtonVisible)
            {
                Rect closeButtonPosition = new Rect(_windowPosition.width - 25, 1, 24, 24);
                if (GUI.Button(closeButtonPosition, "x")) { Close(); }
            }
            ControlWindowPosition();

            WindowUpdate();
            WindowUI();
        }

        public virtual void WindowUpdate()
        {

        }

        public virtual void WindowUI()
        {
        }

        private void InitializeWindowPosition(Rect parentRect, int width, int heigth)
        {
            _windowPosition = new Rect();

            if (width == 0)
                _windowPosition.width = 100;
            else
                _windowPosition.width = width;

            if (heigth == 0)
                _windowPosition.height = 100;
            else
                _windowPosition.height = heigth;

            int screenWidth = Screen.width;
            int screenHeigth = Screen.height;

            int separation = 50;

            // Opt to place window to the right
            if (parentRect.x + parentRect.width + separation + _windowPosition.width < screenWidth)
            {
                _windowPosition.x = parentRect.x + parentRect.width + separation;
                _windowPosition.y = parentRect.y;
            }
            // Opt to place window to the left
            else if (parentRect.x - separation - _windowPosition.width > 0)
            {
                _windowPosition.x = parentRect.x - separation - _windowPosition.width;
                _windowPosition.y = parentRect.y;
            }
            // Opt to place it at the same location
            else
            {
                _windowPosition.x = parentRect.x;
                _windowPosition.y = parentRect.y;
            }
        }

        private void ControlWindowPosition()
        {
            int screenWidth = Screen.width;
            int screenHeigth = Screen.height;

            if (_windowPosition.x < 0)
                _windowPosition.x = 0;

            if (_windowPosition.y < 0)
                _windowPosition.y = 0;

            if (_windowPosition.x + _windowPosition.width > screenWidth)
                _windowPosition.x = screenWidth - _windowPosition.width;

            if (_windowPosition.y + _windowPosition.height > screenHeigth)
                _windowPosition.y = screenHeigth - _windowPosition.height;
        }

        //private void initStyle()
        //{
        //    windowStyle = new GUIStyle(HighLogic.Skin.window);
        //    windowStyle.stretchWidth = true;
        //    windowStyle.stretchHeight = false;
        //
        //    labelStyle = new GUIStyle(HighLogic.Skin.label);
        //    labelStyle.stretchWidth = false;
        //    labelStyle.stretchHeight = false;
        //
        //    labelTextStyle = new GUIStyle(HighLogic.Skin.label);
        //    labelTextStyle.stretchWidth = false;
        //    labelTextStyle.stretchHeight = true;
        //    labelTextStyle.wordWrap = false;
        //
        //    redlabelStyle = new GUIStyle(HighLogic.Skin.label);
        //    redlabelStyle.stretchWidth = false;
        //    redlabelStyle.stretchHeight = false;
        //    redlabelStyle.normal.textColor = Color.red;
        //
        //    textFieldStyle = new GUIStyle(HighLogic.Skin.textField);
        //    textFieldStyle.stretchWidth = false;
        //    textFieldStyle.stretchHeight = false;
        //
        //    buttonStyle = new GUIStyle(HighLogic.Skin.button);
        //    buttonStyle.stretchHeight = false;
        //    buttonStyle.stretchWidth = false;
        //
        //    recordButtonStyle = new GUIStyle();
        //    recordButtonStyle.stretchHeight = false;
        //    recordButtonStyle.stretchWidth = false;
        //    recordButtonStyle.alignment = TextAnchor.MiddleLeft;
        //    recordButtonStyle.fixedHeight = 25;
        //    recordButtonStyle.normal.textColor = Color.cyan;
        //
        //    recordSelectionButtonStyle = new GUIStyle(HighLogic.Skin.button);
        //    recordSelectionButtonStyle.stretchHeight = false;
        //    recordSelectionButtonStyle.stretchWidth = false;
        //    recordSelectionButtonStyle.alignment = TextAnchor.MiddleCenter;
        //    recordSelectionButtonStyle.fixedHeight = 25;
        //    recordSelectionButtonStyle.fixedWidth = 25;
        //
        //    scrollViewStyle = new GUIStyle(HighLogic.Skin.scrollView);
        //    scrollViewStyle.stretchHeight = false;
        //    scrollViewStyle.stretchWidth = true;
        //    horiScrollbarStyle = new GUIStyle(HighLogic.Skin.horizontalScrollbar);
        //    vertiScrollbarStyle = new GUIStyle(HighLogic.Skin.verticalScrollbar);
        //}

        #region IWindow interface
        public int Id { get; set; }
        public WindowManager Manager { get; set; }
        IWindow IWindow.ParentWindow { get; set; }
        List<IWindow> IWindow.ChildWindows { get; set; }
        public void OnGUI()
        {
            drawWindow();
        }
        public Rect WindowPosition
        {
            get { return _windowPosition; }
            set { _windowPosition = value; }
        }
        public void InitializeRelativeWindowPosition(Rect parentRect)
        {
            InitializeWindowPosition(parentRect, _width, _height);
        }
        #endregion IWindow interface
    }

}
