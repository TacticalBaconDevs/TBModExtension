using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBModExtension.APIs
{
    public class TBModExtensionAPI_v1
    {
        public const int apiVersion = 1;
        /// <summary>
        /// Die Version der anbietenden API DLL
        /// </summary>
        public readonly static string version = "1.0.0";

        /// <summary>
        /// Wird aufgerufen wenn syncrone Functionen ausgelöst werden. Nichts was Lange dauert oder pausiert verwenden, nur schnelle Sachen
        /// </summary>
        /// <param name="fnc">Funktion die aufgerufen wird (IMMER LOWERCASE)</param>
        /// <param name="args">Argumente aus Arma3</param>
        /// <param name="execCallback">der Callback zurück zu Arma3 -  Parameter1: fncName, Parameter2: args</param>
        /// <returns>0 - nichts passiert, 1 = erfolgreich, -1 = fehlgeschlagen</returns>
        public static int syncFncs(string fnc, object[] args, Action<string, object[]> execCallback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Wid aufgerufen, wenn keine syncrone Aufgabe gefunden wird. Läuft im Threadpool, kann also mit sleep usw arbeiten
        /// </summary>
        /// <param name="fnc">Funktion die aufgerufen wird (IMMER LOWERCASE)</param>
        /// <param name="args">Argumente aus Arma3</param>
        /// <param name="taskId">die Audgabennummer</param>
        /// <param name="execCallback">der Callback zurück zu Arma3 -  Parameter1: fncName, Parameter2: args</param>
        /// <param name="setTaskStatus">setzt den Status in der Laufzeit der Ausführung - Parameter1: taskId, Parameter2: Status (UPPERCASE)</param>
        /// <returns>0 - nichts passiert (Status: DONE), 1 = erfolgreich (Status: DONE), -1 = fehlgeschlagen (Status: ERROR)</returns>
        public static int assyncFncs(string fnc, object[] args, long taskId, Action<string, object[]> execCallback, Action<long, string> setTaskStatus)
        {
            throw new NotImplementedException();
        }

    }
}
