using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings
{
    public class LoggerRmm
    {
        public List<string> Warnings;

        public static void Error(string message)
        {
            MonoBehaviour.print("RMM error: " + message);
        }

        public static void Warning(string message)
        {
            MonoBehaviour.print("RMM warning: " + message);
        }


        public static void Debug(string message)
        {
            MonoBehaviour.print("RMM debug: " + message);
        }
    }


}
