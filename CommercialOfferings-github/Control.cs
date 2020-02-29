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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace CommercialOfferings
{
    public partial class RMMModule : PartModule
    {
        [KSPField(isPersistant = false, guiActive = false)]
        public bool DevMode = false;

        public Tracking TrackingModule;

        public override void OnAwake()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (part != null) { part.force_activate(); }

                TrackingModule = new Tracking(this);

                ArrivalStage = 0;
                nextLogicTime = Planetarium.GetUniversalTime();

                if (DevMode) { OrderingEnabled = true; }
            }

        }

        public override void OnActive()
        {
            TrackingModule = new Tracking(this);
        }

        public override void OnFixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            setModule();
            if (nextLogicTime == 0 || nextLogicTime > Planetarium.GetUniversalTime()) { return; }
            if (vessel.packed || !vessel.loaded)
            {
                nextLogicTime = Planetarium.GetUniversalTime();
                return;
            }

            //if (!moduleSet) { setModule(); moduleSet = true; }

            if (trackingActive || trackingPrimary)
            {
                TrackingModule.handleTracking();
                nextLogicTime = Planetarium.GetUniversalTime() + 1;
            }
            else
            {
                if (!handleCommercialVehicleMode()) { return; }
                handleArrivalCompletion();
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
                // preDraw code
            }
            //DrawGUI();
        }

        private void setModule()
        {
            if (commercialvehiclemode && commercialvehicleOfferingLoaded)
            {
                Events["setAutoDepart"].guiActive = true;
            }
            else
            {
                Events["setAutoDepart"].guiActive = false;
            }
            if ((RmmUtil.IsPreLaunch(vessel) && !trackingActive && IsDockingPort()) || (returnMission && !trackingActive))
            {

                Events["tracking"].guiActive = true;
            }
            else
            {

                Events["tracking"].guiActive = false;
            }
            
            if (PortCode == "" && vessel.situation == Vessel.Situations.ORBITING && RmmUtil.AllowedBody(vessel.mainBody.name) && IsDockingPort())
                Events["register"].guiActive = true;
            else
                Events["register"].guiActive = false;
        }

        [KSPEvent(name = "tracking", isDefault = false, guiActive = false, guiActiveEditor = true, guiName = "Track Mission")]
        public void tracking()
        {
            TrackingModule.trackingEvent();
        }


        public bool IsDockingPort()
        {
            foreach (PartModule pm in part.Modules)
            {
                if (pm.ClassName == "ModuleDockingNode")
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawGUI()
        {
            //Tracking GUI rendering
            TrackingModule.DrawGUI();

            //Routine GUI rendering
            if (renderGUIMain)
            {
                drawGUIMain();
            }
            if (renderGUIOffering)
            {
                drawGUIOffering();
            }
            if (renderGUIMission)
            {
                drawGUIMission();
            }
            if (renderGUIPrefCrew)
            {
                drawGUIPrefCrew();
            }
            if (renderGUIRegister)
            {
                drawGUIRegister();
            }
        }

        public void OpenGUITracking()
        {
            TrackingModule.OpenGUITracking();
        }

        public void CloseGUITracking()
        {
            TrackingModule.CloseGUITracking();
        }
    }
}
