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
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CommercialOfferings
{
    public class RmmContract
    {
        public static void HandleContracts(Vessel vessel, bool arrive, bool depart)
        {
            //print("handling");

            List<ProtoCrewMember> crew = new List<ProtoCrewMember>();
            foreach (Part p in vessel.parts)
            {
                if (p.CrewCapacity > 0 && p.protoModuleCrew.Count > 0)
                {
                    for (int c = 0; c < p.protoModuleCrew.Count; c++)
                    {
                        crew.Add(p.protoModuleCrew[c]);
                    }
                }
            }

            if (crew.Count == 0) { return; }

            foreach (ProtoCrewMember c in crew)
            {
                bool contractsHandled = false;
                while (!contractsHandled)
                {
                    contractsHandled = handleContractsForCrew(vessel, arrive, depart, c);
                }
            }
        }


        private static bool handleContractsForCrew(Vessel vessel, bool arrive, bool depart, ProtoCrewMember crew)
        {
            //print("handling crew");
            foreach (Contract con in Contracts.ContractSystem.Instance.Contracts)
            {
                if (con.ContractState == Contract.State.Active)
                {
                    if (ReferenceEquals(con.GetType(), typeof(FinePrint.Contracts.TourismContract)))
                    {
                        for (int i = 0; i < con.ParameterCount; i++)
                        {
                            ContractParameter conpara1 = con.GetParameter(i);
                            if (ReferenceEquals(conpara1.GetType(), typeof(FinePrint.Contracts.Parameters.KerbalTourParameter)) && conpara1.State != Contracts.ParameterState.Complete)
                            {
                                FinePrint.Contracts.Parameters.KerbalTourParameter ktp = (FinePrint.Contracts.Parameters.KerbalTourParameter)conpara1;

                                if (crew.name == ktp.kerbalName)
                                {
                                    // complete destinations parameters on arrive for kerbals on vessel
                                    if (arrive)
                                    {
                                        for (int u = 0; u < conpara1.ParameterCount; u++)
                                        {
                                            ContractParameter conpara2 = conpara1.GetParameter(u);
                                            if (ReferenceEquals(conpara2.GetType(), typeof(FinePrint.Contracts.Parameters.KerbalDestinationParameter)) && conpara2.State != Contracts.ParameterState.Complete)
                                            {
                                                FinePrint.Contracts.Parameters.KerbalDestinationParameter kds = (FinePrint.Contracts.Parameters.KerbalDestinationParameter)conpara2;

                                                if (RmmUtil.AllowedBody(vessel.mainBody.name))
                                                {
                                                    if (RmmUtil.HomeBody(kds.targetBody.name) && (kds.targetType == FlightLog.EntryType.Orbit || kds.targetType == FlightLog.EntryType.Suborbit))
                                                    {
                                                        //print("complete1");
                                                        CompleteContractParameter(kds);
                                                        return false;
                                                    }
                                                }
                                                if (RmmUtil.AllowedBody(vessel.mainBody.name) && !RmmUtil.HomeBody(vessel.mainBody.name))
                                                {
                                                    if (kds.targetBody.name == vessel.mainBody.name && (kds.targetType == FlightLog.EntryType.Orbit || kds.targetType == FlightLog.EntryType.Flyby))
                                                    {
                                                        //print("complete2");
                                                        CompleteContractParameter(kds);
                                                        return false;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // complete kerbaltour parameters on depart which have all their destinations completed
                                    if (depart)
                                    {
                                        if (conpara1.State != Contracts.ParameterState.Complete)
                                        {
                                            bool allDestinationsSucceeded = true;
                                            for (int u = 0; u < conpara1.ParameterCount; u++)
                                            {
                                                ContractParameter conpara2 = conpara1.GetParameter(u);
                                                if (conpara2.State != Contracts.ParameterState.Complete)
                                                {
                                                    allDestinationsSucceeded = false;
                                                }
                                            }
                                            if (depart && allDestinationsSucceeded)
                                            {
                                                //print("complete3");
                                                CompleteContractParameter(ktp);
                                                HighLogic.CurrentGame.CrewRoster.Remove(crew);
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static void CompleteContractParameter(object objInstance)
        {
            System.Reflection.MethodInfo m;
            try
            {
                m = objInstance.GetType().GetMethod("SetComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (ReferenceEquals(m, null))
                {
                    //print("There is no method '" +
                    // "SetComplete" + "' for type '" + objInstance.GetType().ToString() + "'.");
                    return;
                }

                object objRet = m.Invoke(objInstance, null);
            }
            catch
            {
                throw;
            }
        }

        //        private static object CompleteContract(System.Type t, string
        //strMethod, object objInstance, object[] aobjParams, BindingFlags eFlags)
        //        {
        //            MethodInfo m;
        //            try
        //            {
        //                m = t.GetMethod(", eFlags);
        //                if (m == null)
        //                {
        //                    throw new ArgumentException("There is no method '" +
        //                     strMethod + "' for type '" + t.ToString() + "'.");
        //                }
        //
        //                object objRet = m.Invoke(objInstance, aobjParams);
        //                return objRet;
        //            }
        //            catch
        //            {
        //                throw;
        //            }
        //        } //end of method
    }
}
