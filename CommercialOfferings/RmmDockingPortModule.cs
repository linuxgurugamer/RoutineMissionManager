using CommercialOfferings.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    class RmmDockingPortModule : PartModule
    {
        [KSPField(isPersistant = false, guiActive = false)]
        public string Name = "";

        private double _nextLogicTime = 0;

        private bool _registrationAllowed = false;
        private bool _trackingAllowed = false;


        public bool _refreshedRequired = true;

        Action _onFixedUpdateCallback = null;

        public override void OnAwake()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (part != null) { part.force_activate(); }
                _nextLogicTime = Planetarium.GetUniversalTime() + 1;
            }
        }

        public override void OnFixedUpdate()
        {
            if (_onFixedUpdateCallback != null) { _onFixedUpdateCallback.Invoke(); }

            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (_nextLogicTime == 0 || _nextLogicTime > Planetarium.GetUniversalTime()) { return; }
            if (vessel.packed || !vessel.loaded) 
            {
                _nextLogicTime = Planetarium.GetUniversalTime() + 1;
                return;
            }

            if (vessel.situation == Vessel.Situations.ORBITING && RmmUtil.AllowedBody(vessel.mainBody.name) && _registrationEnabled)
            {
                if (!_registrationAllowed)
                {
                    _registrationAllowed = true;
                    _refreshedRequired = true;
                }
            }
            else
            {
                if (_registrationAllowed)
                {
                    _registrationAllowed = false;
                    _refreshedRequired = true;
                }
            }

            if (vessel.situation == Vessel.Situations.ORBITING && RmmUtil.AllowedBody(vessel.mainBody.name) && _trackingEnabled)
            {
                if (!_trackingAllowed)
                {
                    _trackingAllowed = true;
                    _refreshedRequired = true;
                }
            }
            else
            {
                if (_trackingAllowed)
                {
                    _trackingAllowed = false;
                    _refreshedRequired = true;
                }
            }

            if (_refreshedRequired)
            {
                RefreshModule();
            }

            _nextLogicTime = Planetarium.GetUniversalTime() + 10;
        }

        private void RefreshModule()
        {
            name = RmmScenario.Instance.GetRegisteredDockingPort(part.flightID);
            if (name != null) { Name = name; }

            if (_registrationAllowed)
            {
                Events["register"].guiActive = true;
            }
            else
            {
                Events["register"].guiActive = false;
            }

            if (_trackingAllowed)
            {
                Events["tracking"].guiActive = true;
            }
            else
            {
                Events["tracking"].guiActive = false;
            }

            if (String.IsNullOrEmpty(Name))
            {
                Fields["Name"].guiActive = false;
            }
            else
            {
                Fields["Name"].guiActive = true;
            }

            _refreshedRequired = false;
        }

        public bool RegistrationEnabled
        {
            get
            {
                return _registrationEnabled;
            }
            set
            {
                _registrationEnabled = value;
                _refreshedRequired = true;
            }
        }
        private bool _registrationEnabled = false;

        public bool TrackingEnabled
        {
            get
            {
                return _trackingEnabled;
            }
            set
            {
                _trackingEnabled = value;
                _refreshedRequired = true;
            }
        }
        private bool _trackingEnabled = false;

        [KSPEvent(name = "register", isDefault = false, guiActive = true, guiName = "Register Docking Port")]
        public void register()
        {
            RegisterDockingPortWindow registerWindow = new RegisterDockingPortWindow(this);
            WindowManager.Open(registerWindow);
        }


        public void RegisterDockingPort(string name)
        {
            RmmScenario.Instance.SetRegisteredDockingPort(part.flightID, name);
            _refreshedRequired = true;
            _nextLogicTime = Planetarium.GetUniversalTime() + 1;
        }


        [KSPEvent(name = "tracking", isDefault = false, guiActive = true, guiName = "Start Tracking")]
        public void tracking()
        {
            RmmMonoBehaviour.Instance.CreateDepartureTracking(part);
        }

        public void SetOnFixedUpdateCallback(Action onFixedUpdateCallback)
        {
            _onFixedUpdateCallback = onFixedUpdateCallback;
        }

        public static void AddToPart(Part part)
        {
            var dockingModule = part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
            if (dockingModule != null)
            {
                var rmmDockingPortModule = part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
                if (rmmDockingPortModule == null)
                {
                    part.AddModule(typeof(RmmDockingPortModule).Name);
                }
            }
        }

        public static void AddToVessel(Vessel vessel)
        {
            foreach (Part p in vessel.parts)
            {
                var dockingModule = p.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                if (dockingModule != null)
                {
                    var rmmDockingPortModule = p.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
                    if (rmmDockingPortModule == null)
                    {
                        p.AddModule(typeof(RmmDockingPortModule).Name);
                    }
                }
            }
        }

        public static void SetEnableRegistration(Vessel vessel, bool enable)
        {
            foreach (Part p in vessel.parts)
            {
                var rmmDockingPortModule = p.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
                if (rmmDockingPortModule != null)
                {
                    rmmDockingPortModule.RegistrationEnabled = enable;
                }
            }
        }

        public static void SetEnableTracking(Vessel vessel, bool enable)
        {
            foreach (Part p in vessel.parts)
            {
                var rmmDockingPortModule = p.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
                if (rmmDockingPortModule != null)
                {
                    rmmDockingPortModule.TrackingEnabled = enable;
                }
            }
        }

    }
}
