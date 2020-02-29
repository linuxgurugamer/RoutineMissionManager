/*Copyright (c) 2014, Flip van Toly
 All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are 
permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of 
conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of 
conditions and the following disclaimer in the documentation and/or other materials provided with 
the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to 
endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
OF SUCH DAMAGE.*/

//Namespace Declaration 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings
{
    public class RmmStyle
    {
        private static RmmStyle _instance;

        private RmmStyle()
        {
            InitializeStyle();
        }

        public static RmmStyle Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new RmmStyle();
                }
                return _instance;
            }
        }


        public GUIStyle WindowStyle, LabelStyle, LabelTextStyle, LabelTextTitleStyle, LabelTextSubTitleStyle, RedLabelStyle, TextFieldStyle, ButtonStyle, HoriScrollBarStyle, VertiScrollBarStyle, RecordButtonStyle, RecordSelectionButtonStyle;



        private void InitializeStyle()
        {
            WindowStyle = new GUIStyle(HighLogic.Skin.window);
            WindowStyle.stretchWidth = false;
            WindowStyle.stretchHeight = false;

            LabelStyle = new GUIStyle(HighLogic.Skin.label);
            LabelStyle.stretchWidth = false;
            LabelStyle.stretchHeight = false;

            LabelTextStyle = new GUIStyle(HighLogic.Skin.label);
            LabelTextStyle.stretchWidth = false;
            LabelTextStyle.stretchHeight = true;
            LabelTextStyle.wordWrap = true;

            LabelTextTitleStyle = new GUIStyle(HighLogic.Skin.label);
            LabelTextTitleStyle.stretchWidth = false;
            LabelTextTitleStyle.stretchHeight = false;
            LabelTextTitleStyle.fontSize = 25;

            LabelTextSubTitleStyle = new GUIStyle(HighLogic.Skin.label);
            LabelTextSubTitleStyle.stretchWidth = false;
            LabelTextSubTitleStyle.stretchHeight = false;
            LabelTextSubTitleStyle.fontSize = 20;

            RedLabelStyle = new GUIStyle(HighLogic.Skin.label);
            RedLabelStyle.stretchWidth = false;
            RedLabelStyle.stretchHeight = false;
            RedLabelStyle.normal.textColor = Color.red;
                
            TextFieldStyle = new GUIStyle(HighLogic.Skin.textField);
            TextFieldStyle.stretchWidth = false;
            TextFieldStyle.stretchHeight = false;
                
            ButtonStyle = new GUIStyle(HighLogic.Skin.button);
            ButtonStyle.stretchHeight = false;
            ButtonStyle.stretchWidth = false;

            RecordButtonStyle = new GUIStyle();
            RecordButtonStyle.stretchHeight = false;
            RecordButtonStyle.stretchWidth = false;
            RecordButtonStyle.alignment = TextAnchor.MiddleLeft;
            RecordButtonStyle.fixedHeight = 25;
            RecordButtonStyle.normal.textColor = Color.cyan;

            RecordSelectionButtonStyle = new GUIStyle(HighLogic.Skin.button);
            RecordSelectionButtonStyle.stretchHeight = false;
            RecordSelectionButtonStyle.stretchWidth = false;
            RecordSelectionButtonStyle.alignment = TextAnchor.MiddleCenter;
            RecordSelectionButtonStyle.fixedHeight = 25;
            RecordSelectionButtonStyle.fixedWidth = 25;

            HoriScrollBarStyle = new GUIStyle(HighLogic.Skin.horizontalScrollbar);
            VertiScrollBarStyle = new GUIStyle(HighLogic.Skin.verticalScrollbar);
        }
    }
}
