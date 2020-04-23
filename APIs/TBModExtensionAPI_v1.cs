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
        /// SQF Code (unscheduled) wird ausgeführt, wenn die Extension erfolgreich geladen wurde
        /// </summary>
        /// <returns></returns>
        public static string init()
        {
            return @"";
        }

        /// <summary>
        /// Wird aufgerufen wenn syncrone Functionen ausgelöst werden. Nichts was Lange dauert oder pausiert verwenden, nur schnelle Sachen
        /// </summary>
        /// <param name="fnc">Funktion die aufgerufen wird (IMMER LOWERCASE)</param>
        /// <param name="args">Argumente aus Arma3</param>
        /// <param name="execCallback">der Callback zurück zu Arma3 - Erklärung unten
        /// <returns>0 - nichts passiert, 1 = erfolgreich, -1 = fehlgeschlagen</returns>
        public static int syncFncs(string fnc, object[] args, Action<string, object[]> execCallback)
        {
            return  0;
        }

        /// <summary>
        /// Wid aufgerufen, wenn keine syncrone Aufgabe gefunden wird. Läuft im Threadpool, kann also mit sleep usw arbeiten
        /// </summary>
        /// <param name="fnc">Funktion die aufgerufen wird (IMMER LOWERCASE)</param>
        /// <param name="args">Argumente aus Arma3</param>
        /// <param name="taskId">die Audgabennummer</param>
        /// <param name="execCallback">der Callback zurück zu Arma3 - Erklärung unten
        /// <param name="setTaskStatus">setzt den Status in der Laufzeit der Ausführung - Parameter1: taskId, Parameter2: Status (UPPERCASE)</param>
        /// <returns>0 - nichts passiert (Status: DONE), 1 = erfolgreich (Status: DONE), -1 = fehlgeschlagen (Status: ERROR)</returns>
        public static int assyncFncs(string fnc, object[] args, long taskId, Action<string, object[]> execCallback, Action<long, string> setTaskStatus)
        {
            return 0;
        }

        /// execCallback
        /// Parameter1: fncName, wenn anders als folgend, dann wird nur
        ///                 - log / error - Ausgabe von allem als diag_log/systemChat | Parameter2 beliebig wird in die Message gepackt
        ///                 - call / spawn - Ausführung des Codes | Parameter2 muss dann Code als String sein (aka. "player setPos [0,0,0]")
        /// Parameter2: deine Argumente
        ///                 - im Callback kommt es als _data Array mit deinen Argumenten
        ///                 - AUSNAHME: wenn es nur ein String ist, dann ist _data im Callback nur ein String
        
    }
}
