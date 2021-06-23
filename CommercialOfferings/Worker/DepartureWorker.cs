using System;
using System.Collections.Generic;
using KSP.Localization;
using System.Linq;
using System.Text;

namespace CommercialOfferings.Worker
{
    class DepartureWorker
    {
        private double _nextLogicTime = 0;

        //arrival transaction 
        public bool CompleteDeparture = false;
        private int _departureStage = 0;
        private Vessel _departureVessel = null;
        private Part _departurePart = null;

        private RoutineDepartureMission _mission;
        private Vessel _vessel = null;
        private Part _part = null;

        public void StartDeparture(RoutineDepartureMission mission, Vessel vessel )
        {
            _mission = mission;
            _vessel = vessel;
            _part = RmmUtil.GetVesselPart(vessel, mission.flightIdDepartureDockPart);
            _departurePart = RmmUtil.GetDockedPart(FlightGlobals.ActiveVessel, _part);
            CompleteDeparture = true;
            _nextLogicTime = Planetarium.GetUniversalTime();
            _departureStage = 0;

            RmmDockingPortModule.AddToPart(_part);
            var rmmDockingPortModule = _part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
            rmmDockingPortModule.SetOnFixedUpdateCallback(OnFixedUpdateDepartureCompletion);
        }


        public void OnFixedUpdateDepartureCompletion()
        {
            if (!CompleteDeparture) { return; }
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (_nextLogicTime == 0 || _nextLogicTime > Planetarium.GetUniversalTime()) { return; }

            if (_departureStage == 0)
            {
                if (_vessel != null && _part != null && _departurePart != null)
                {
                    _departureStage = 1;
                    _nextLogicTime = Planetarium.GetUniversalTime();
                }
                else
                {
                    abortDeparture();
                }
            }

            if (_vessel.packed || !_vessel.loaded)
            {
                _nextLogicTime = Planetarium.GetUniversalTime();
                return;
            }

            if (CompleteDeparture)
            {
                switch (_departureStage)
                {
                    case 1:
                        departureStage1();
                        break;
                    case 2:
                        departureStage2();
                        break;
                    case 3:
                        departureStage3();
                        break;
                }
            }
        }

        private void departureStage1()
        {
            if (RmmUtil.IsDocked(_vessel, _part))
            {
                ModuleDockingNode DockNode = _part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                DockNode.Undock();
            }
            _departureStage = 2;
            _nextLogicTime = Planetarium.GetUniversalTime() + 2;
        }

        private void departureStage2()
        {
            if (RmmUtil.IsDocked(_vessel, _departurePart))
            {
                abortDeparture();
            }
            else
            {
                _departureVessel = _departurePart.vessel;
            }

            if (_departureVessel.isActiveVessel)
            {
                foreach (Vessel ves in FlightGlobals.Vessels)
                {
                    if (!ves.packed && ves.loaded && ves.id != _departureVessel.id)
                    {
                        FlightGlobals.SetActiveVessel(ves);
                        _departureStage = 3;
                        _nextLogicTime = Planetarium.GetUniversalTime() + 1;
                        return ;
                    }
                }
            }
            else
            {
                _departureStage = 3;
                _nextLogicTime = Planetarium.GetUniversalTime() + 1;
            }
        }

        private void departureStage3()
        {
            DepartureCompletion();
        }


        private void DepartureCompletion()
        {
            RmmUtil.ToMapView();
            RmmContract.HandleContracts(_departureVessel, false, true);
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                Funding.Instance.AddFunds(_mission.Price + cargoFee(), TransactionReasons.VesselRecovery);
            }

            handleUnloadCrew(_departureVessel, true);
            _departureVessel.Unload();
            _departureVessel.Die();

            ScreenMessages.PostScreenMessage(Localizer.Format(_mission.VesselName) + " returned to " + RmmUtil.HomeBodyName(), 4, ScreenMessageStyle.UPPER_CENTER);
            finishDeparture();
        }
    
    
        private void handleUnloadCrew(Vessel ves, bool savereturn)
        {
            foreach (Part p in ves.parts)
            {
                if (p.CrewCapacity > 0 && p.protoModuleCrew.Count > 0)
                {
                    for (int i = p.protoModuleCrew.Count - 1; i >= 0; i--)
                    {
                        unloadCrew(p.protoModuleCrew[i], p, savereturn);
                    }
                }
            }
            ves.DespawnCrew();
        }
    
        private void unloadCrew(ProtoCrewMember crew, Part p, bool savereturn)
        {
            p.RemoveCrewmember(crew);
    
            if (savereturn)
            {
                crew.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            }
            else
            {
                if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                {
                    crew.rosterStatus = ProtoCrewMember.RosterStatus.Missing;
                }
                else
                {
                    crew.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
                }
            }
        }

        private double cargoFee()
        {
            double fee = 0.0;

            if (_mission.CargoMass == 0) { return 0; }

            double cargoMass = _mission.CargoMass;

            List<string> cargoArray = RmmUtil.GetCargoArray(_departureVessel, _mission.Proppelants);

            orderCargoArray(ref cargoArray);

            foreach (String s in cargoArray)
            {
                foreach (Part p in _departureVessel.parts)
                {
                    foreach (PartResource r in p.Resources)
                    {
                        if (r.info.name == s)
                        {
                            if (r.amount != 0)
                            {
                                if (RmmUtil.Mass(r.info.name, r.amount) <= cargoMass)
                                {
                                    fee = fee + RmmUtil.Cost(r.info.name, r.amount);
                                    cargoMass = cargoMass - RmmUtil.Mass(r.info.name, r.amount);
                                }
                                else
                                {
                                    fee = fee + ((cargoMass / RmmUtil.Mass(r.info.name, r.amount)) * RmmUtil.Cost(r.info.name, r.amount));
                                    return fee;
                                }
                            }
                        }
                    }
                }
            }
            return fee;
        }


        private void orderCargoArray(ref List<string> cargoArray)
        {
            string[] unorderCargoArray = new string[cargoArray.Count];
            double[] costPerMass = new double[cargoArray.Count];

            for (int i = 0; i < cargoArray.Count; i++)
            {
                unorderCargoArray[i] = cargoArray[i];
                PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(cargoArray[i]);
                costPerMass[i] = prd.unitCost / prd.density;
            }

            for (int u = 0; u < cargoArray.Count; u++)
            {
                int highestCargoResource = -1;

                for (int i = 0; i < cargoArray.Count; i++)
                {
                    if (unorderCargoArray[i] != "")
                    {
                        if (highestCargoResource != -1)
                        {
                            if (costPerMass[i] > costPerMass[highestCargoResource])
                                highestCargoResource = i;
                        }
                        else
                        {
                            highestCargoResource = i;
                        }
                    }
                }

                if (highestCargoResource != -1)
                {
                    cargoArray[u] = unorderCargoArray[highestCargoResource];
                    unorderCargoArray[highestCargoResource] = "";
                }
            }
        }

        private void finishDeparture()
        {
            CompleteDeparture = false;

            _nextLogicTime = 0;
            _departureStage = -1;

            var rmmDockingPortModule = _part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
            rmmDockingPortModule.SetOnFixedUpdateCallback(null);
        }

        private void abortDeparture()
        {
            CompleteDeparture = false;

            _nextLogicTime = 0;
            _departureStage = -1;

            var rmmDockingPortModule = _part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
            rmmDockingPortModule.SetOnFixedUpdateCallback(null);
        }

        public RoutineDepartureMission Mission
        {
            get { return _mission; }
        }
    }
}
