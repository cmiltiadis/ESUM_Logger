using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ESUM_Logger
{
    public enum DeviceName // <-- Symmetrical  
    {
        WaspCity,
        WaspGas,
        GPS
    };
    public enum DeviceComPort // < -- Symmetrical 
    {
        COM9,
        COM10,
        COM3
    }

    public enum ConsoleCommand { Status, Rescan, Monitor, Duration, Clear, Beep, Help, Exit };

    class Util
    {
        // Start automatically, or request the Rescan function from input
        public static bool StartPortsOnAwake = false;
        //
        public static DateTime startTime;
        public static bool monitorInput = false;
        //
        static readonly int[] BaudRates = { 9600, 14400, 19200, 28800, 38400, 57600, 115200 };//list of usual Baud Rates
        public static readonly string[] deviceStartPhrases = { "$Sound", "Bat", "$GNDTM" }; // <-- Symmertrical to Device Enums
        static readonly int[] deviceBaudRates = { 6, 6, 6 };  //eg. 6 is BaudRates[6] =115200  <-- Symmertrical to Device Enums 
        //
        static int numOfFramesToExport = 2;
        public static int NumOfFramesToExport { get { return numOfFramesToExport; } }
        //
        static string fileExtension { get { return ".csv"; } }
        public static string FileExtension { get { return fileExtension; } }
        //separator line
        public static string tabLine { get { return "############################################"; } }//\t # \t # \t # \t # \t # \t #"; } }

        //print available commands on the console
        public static void PrintConsoleCommands()
        {
            int nConsoleCommands = Enum.GetNames(typeof(ConsoleCommand)).Length;
            string toPrint = "Console Commands: ";
            for (int i = 0; i < nConsoleCommands; i++)
            {
                toPrint += ((ConsoleCommand)(i)).ToString() + "|";
            }
            Console.WriteLine(toPrint);
        }

        public static int DeviceBaudeRates(int indx)
        {
            return BaudRates[deviceBaudRates[indx]];
        }

        /*
         *      EXPORT STUFF
         */
        #region Export
        static string exportFolder;
        //export properties
        public static string ParentFolder { get { return "ESUM_Export"; } }
        public static string SessionFolder { get { return "ESUM_Session_"; } }

        public static void GetExportDirectory()
        {
            /*Folder path 
             * 
             *  ..../My Documents/ESUM_Export/ ESUM_Session_<datetime>/  
             *   + <deviceName>.<ExportFormat> // eg. GPS.CSV
             */
            string timeString = GetDateTimeSimple();
            exportFolder = (System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents", ParentFolder, SessionFolder + timeString));

            //if it doesnt exist create it 
            bool doesFolderExist = System.IO.Directory.Exists(exportFolder);
            if (!doesFolderExist)
                System.IO.Directory.CreateDirectory(exportFolder);
        }

        public static void PrintExportFolder()
        {
            Console.WriteLine("Session export folder: ");
            Console.WriteLine(exportFolder);
        }
        public static void ClearConsole()
        {
            Console.Clear();
            PrintConsoleCommands(); 
        }
        public static void ExportText(string fileName, string textToExport)
        {
            //combine paths to create new file 
            string currentExportPath = System.IO.Path.Combine(exportFolder, fileName + FileExtension);

            try
            {
                using (StreamWriter sw = new StreamWriter(currentExportPath, true)) // true is for append
                {
                    sw.Write(textToExport);
                    Console.WriteLine("Exported: " + fileName);// currentExportPath);
                }
            }
            catch
            {
                Console.WriteLine("Failed to export :" + currentExportPath);
            }

        }


        #endregion

        /*
         *  USB CLICKER
         */
        #region Clicker

        //Clicker 1 RED
        public static string Clicker1 { get { return "F1"; } }
        //Clicker 2 Orange
        public static string Clicker2 { get { return "F2"; } }
        //Clicker 3 Green
        public static string Clicker3 { get { return "F3"; } }

        //clicker combinations
        public static string Clicker_StartCommand { get { return Clicker3 + Clicker2 + Clicker1; } }

        public static string Clicker_LogEvent { get { return Clicker3 + Clicker3 + Clicker3; } }

        public static string ClickerPause { get { return Clicker1 + Clicker2 + Clicker3; } }
        public static string ClickerClear { get { return Clicker1 + Clicker1 + Clicker1; } }
        #endregion
        /*
         *  TIME
         */
        #region Time_Stuff

        /*  Time format guide
         * see here: https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
         * yy - 4 digit year
         * MM - 2 digit month (01 for January)
         * dd - 2 digit day of month
         * mm - 2 digit minutes
         * fff - 3 digit milliseconds
         */

        static string TimeStampFormat { get { return "yyyy_MM_dd_HH_mm_fff"; } }
        static string FolderTimeFormat { get { return "yyyy_MM_d_HH_mm"; } }

        public static TimeSpan RunTime()
        {
            TimeSpan duration = DateTime.Now - startTime;
            return duration;
        }

        //used for folder 
        public static string GetDateTimeSimple()
        {
            return (DateTime.Now).ToString(FolderTimeFormat);
        }
        //used for frame log
        public static string GetTimeStamp()
        {
            return (DateTime.Now).ToString(TimeStampFormat);
        }
        #endregion


        /*
         *  BEEPS
         */
        #region Beeps
        private static bool doBeeps = true;
        public static bool DoBeeps { get { return doBeeps; } set { doBeeps = value; } }

        static int onOffBeeps { get { return 3; } }

        static int errorBeeps { get { return 5; } }

        static int BeepFreq { get { return 500; } }
        static int BeepDurationNormal { get { return 400; } }
        static int BeepDurationError { get { return 700; } }

        static void DoNBeeps(int nBeeps, bool ascending)
        {
            int freq = BeepFreq;

            if (!ascending)
            {
                //calculate high freq note
                for (int i = 0; i < nBeeps - 1; i++)
                {
                    freq *= 2;
                }
            }

            for (int i = 0; i < nBeeps; i++)
            {
                Console.Beep(freq, BeepDurationNormal);
                if (ascending)
                {
                    freq *= 2;
                }
                else
                {
                    freq /= 2;
                }
            }
        }
        static void AlternateBeeps(int nBeeps)
        {
            for (int i = 0; i < nBeeps; i++)
            {
                if (i % 2 == 1)
                {
                    Console.Beep(BeepFreq, BeepDurationError);
                }
                else
                {
                    Console.Beep(BeepFreq * 3, BeepDurationError);
                }
            }
        }

        //start beep, ascending
        public static void BeepStartup()
        {
            DoNBeeps(onOffBeeps, true);
        }
        //shut down beep , descending
        public static void BeepShutDown()
        {
            DoNBeeps(onOffBeeps, false);
        }
        public static void BeepSynch(int deviceNum)
        {
            deviceNum += 1;// indxes start from 0
            //do as many beeps as the Index of the device, fast
            for (int i = 0; i < deviceNum; i++)
            {
                Console.Beep(BeepFreq, BeepDurationNormal / 2);
            }
            //do a long higer beep to say its synched
            Console.Beep(BeepFreq * 2, BeepDurationNormal * 3);
        }
        //error beep
        public static void BeepError()
        {
            AlternateBeeps(errorBeeps);
        }
        //do single freq n beeps
        static void DoNBeeps(int nBeeps)
        {
            for (int i = 0; i < nBeeps; i++)
            {
                Console.Beep();
            }
        }

        public static void BeepTest()
        {
            DoNBeeps(2);
        }

        #endregion


        public static string ProgramStamp { get { return "*** ESUM Logger\n*** Chair of Information Architecture \n*** ETH Zurich, 2015"; } }

    }
}
