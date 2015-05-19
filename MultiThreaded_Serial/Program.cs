using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESUM_Logger
{
    class Program
    {
        static bool exit = false;

        static Device[] devices; // <-- symmetrical to Enums 

        static void Main(string[] args)
        {
            OnStart();
            //allocate device objects
            AllocateDeviceObjects();
            //make connection to ports
            if (Util.StartPortsOnAwake)
            {
                ScanInitializePorts();
            }
            else
            {
                Console.WriteLine("#Automatic start disabled / Waiting for RESCAN Command");
                Util.PrintConsoleCommands(); 
            }

            //Loop the program
            //this case is for using the Clicker as well
            string keyboardInput = "";
            ConsoleKeyInfo keyinfo;
            do
            {
                /*  because we need to read the USB Clicker, 
                 *  rather than reading only lines,
                 *  we need to read individual characters
                 */
                keyinfo = Console.ReadKey();

                if (keyinfo.Key == ConsoleKey.Enter)
                {
                    //try to get command, clear buffer
                    SwitchConsoleCommand(keyboardInput.ToLower());
                    keyboardInput = "";
                }
                else
                {
                    //add the letter to the key input holder
                    keyboardInput += keyinfo.Key.ToString().ToLower();
                }

                //check for Clicker commands. If there is a match clear the key buffer
                bool ClearKeyBuffer = SwitchClickerCommand(keyboardInput);
                if (ClearKeyBuffer)
                {
                    keyboardInput = "";
                }

            } while (exit == false);


            //if the previous loop is over, the program is about to shut down

            OnExit();

        }
        static void AllocateDeviceObjects()
        {
            devices = new Device[Enum.GetNames(typeof(DeviceName)).Length];
            for (int i = 0; i < devices.Length; i++)
            {
                Device dev = new Device((DeviceName)(i), (DeviceComPort)(i), Util.deviceStartPhrases[i], Util.DeviceBaudeRates(i));
                devices[i] = dev;
            }
        }

        static void OnStart()
        {
            Console.Title = "ESUM Logger    Chair of iA     ETHz";
            Console.WriteLine(Util.ProgramStamp);

            Util.GetExportDirectory();
            Util.startTime = DateTime.Now;

            if (Util.DoBeeps)
                Util.BeepStartup();

        }

        static void OnExit()
        {
            if (Util.DoBeeps)
                Util.BeepShutDown();

            //on exit
            for (int i = 0; i < devices.Length; i++)
            {
                //kill device object's thread
                if (devices[i] != null) devices[i].Kill();
            }
        }

        //Scan for the Com ports the devices are assigned to, and opens port
        static void ScanInitializePorts()
        {
            string[] availablePorts = SerialPort.GetPortNames();

            foreach (string portName in availablePorts)
            {
                bool isUsed = false;
                int nDevices = Enum.GetNames(typeof(DeviceComPort)).Length;
                for (int i = 0; i < nDevices; i++)
                {
                    if (portName == ((DeviceComPort)(i)).ToString())
                    {
                        devices[i].Start();
                        isUsed = true;
                        Console.WriteLine(portName + "::Used::" + devices[i].id);
                    }
                }
                if (!isUsed) Console.WriteLine(portName + "::Unused");

            }
        }

        static void PrintStatus()
        {
            Console.WriteLine(Util.tabLine);
            foreach (Device dev in devices)
            {
                Console.WriteLine(dev.Status());
            }
            Util.PrintExportFolder();
            Console.WriteLine(Util.tabLine);
        }
        static void SwitchConsoleCommand(string input)
        {
            //compare input with predefined console commands (ConsoleCommand enumerator)
            int cmdIndx = CompareStringToCommand(input);
            //if indx is -1, then its not a command
            if (cmdIndx != -1)
            {
                switch ((ConsoleCommand)(cmdIndx))
                {
                    case ConsoleCommand.Clear:
                        Util.ClearConsole();
                        break;
                    case ConsoleCommand.Duration:
                        Console.WriteLine("Duration:" + Util.RunTime().ToString());
                        break;
                    case ConsoleCommand.Monitor:
                        Util.monitorInput = !Util.monitorInput;
                        Console.WriteLine("Toggling intput monitoring:" + Util.monitorInput);
                        break;
                    case ConsoleCommand.Rescan:
                        ScanInitializePorts();
                        break;
                    case ConsoleCommand.Status:
                        PrintStatus();
                        if (Util.DoBeeps)
                        {
                            Util.BeepTest();
                        }
                        break;
                    case ConsoleCommand.Help:
                        Util.PrintConsoleCommands();
                        break;
                    case ConsoleCommand.Exit:
                        Console.WriteLine("Exiting // Program Duration:" + Util.RunTime());
                        exit = true;
                        break;
                    case ConsoleCommand.Beep:
                        Util.DoBeeps = !Util.DoBeeps;
                        Console.WriteLine("Toggle Beep:" + Util.DoBeeps);
                        if (Util.DoBeeps)
                        {
                            Util.BeepTest();
                        }
                        break;
                    default:
                        Console.WriteLine("Switch : Uncaught console command");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Unknown command: " + input + " > Type HELP");
            }
        }

        static bool SwitchClickerCommand(string keyboardInput)
        {

            if (keyboardInput == Util.ClickerStart.ToLower())
            {
                Console.WriteLine("Clicker start");
                ScanInitializePorts();
                return true;
            }
            else if (keyboardInput == Util.ClickerPause.ToLower())
            {
                Console.WriteLine("Clicker pause");
                //keyboardInput = "";
                return true;
            }
            else if (keyboardInput.EndsWith(Util.ClickerClear.ToLower())) //HAS TO BE ENDS WITH/otherwise the clicker piles up button presses
            {
                //clear key buffer. also screen

                Util.ClearConsole();
                return true;
            }
            return false;
        }

        static int CompareStringToCommand(string input)
        {
            int nConsoleCommands = Enum.GetNames(typeof(ConsoleCommand)).Length;
            for (int i = 0; i < nConsoleCommands; i++)
            {
                string comm = ConsoleCommandToText(i);
                if (input == comm)
                {
                    return i;
                }
            }
            return -1; //-1 > it doesnt correspond to a command
        }
        static string ConsoleCommandToText(int v)
        {
            //convert a console command to lower case text
            return (((ConsoleCommand)(v)).ToString()).ToLower();
        }
    }
}
