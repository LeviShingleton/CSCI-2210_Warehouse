///////////////////////////////////////////////////////////////////////////////
//
// Author: Aaron Shingleton, shingletona@etsu.edu
// Course: CSCI-2210-001 - Data Structures
// Assignment: Project 4 - Warehouse
// Description: Controller class of simulation. Provides time increment pulses to Dock -> Truck.
//              Also stores file output variables for global access, mostly utilized by itself and Dock.
//              While 
//
///////////////////////////////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

namespace AS_Warehouse
{
    internal static class Warehouse
    {
        static public Random random = new Random();

        const int MAX_DOCKS = 15;
        const int TIME_INCREMENTS = 48;
        const int DOCK_OP_COST = 100;

        public static int CurrentTime
        {
            get { return currentTime; }
        }

        private static int currentTime = 0;
        static List<Dock> Docks = new();
        static Queue<Truck> Entrance = new();

        #region Output File String Variables

        private static readonly string FILE_OUTPUT_DIRECTORY = @"..\..\..\Warehouse Output\";

        private static string _fileNameBase = "Warehouse";
        public static string FileNameBase
        {
            get { return _fileNameBase; }
            private set { _fileNameBase = value; }
        }

        public static string OUTPUT_FILE_PATH
        {
            get; private set;
        }
        #endregion
        #region Analytics Variables
        #region Truck
        static private int _truckQueueMax = 0;
        static public int TruckQueueMax
        {
            get { return _truckQueueMax; }
            set { _truckQueueMax = value; }
        }
        static private string maxQueueDock;
        #endregion
        #endregion
        #region Sim Settings
        static int dockCount = 1;
        public static bool fastForward { get; private set; }
        #endregion

        #region Initialization
        static Warehouse()
        {
            Init();
        }

        /// <summary>
        /// Resets Warehouse variables to initial values
        /// </summary>
        static void Reset()
        {
            Docks = new();
            Entrance = new();
            dockCount = 1;
            currentTime = 0;

            TruckQueueMax = 0;
            maxQueueDock = "";

            FileNameBase = "Warehouse";
        }
        /// <summary>
        /// Determines Reset() condition and calls for simulation setup.
        /// </summary>
        private static void Init()
        {
            if (currentTime != 0 )
            {
                Reset();
            }
            InitDockList();
            InitFileOutput();
        }
        /// <summary>
        /// Setup of Docks in Warehouse
        /// </summary>
        private static void InitDockList()
        {
            GetCustomDockCount();

            for (int i = 0; i < dockCount; i++)
            {
                Dock tmp = new Dock();
                tmp.id = i.ToString();
                Docks.Add(tmp);
            }
        }
        /// <summary>
        /// Asks the user to define the number of docks, within spec constraints.
        /// </summary>
        static void GetCustomDockCount()
        {
            Console.WriteLine($"How many docks should the warehouse simulate?\nPlease enter a value between 1 and {MAX_DOCKS}.");
            string input = Console.ReadLine();

            while (!(int.TryParse(input.Trim(), out int newNum) && newNum > 0 && newNum <= MAX_DOCKS))
            {
                Console.WriteLine("Please enter a valid number.");
                input = Console.ReadLine();
            }

            dockCount = int.Parse(input.Trim());
        }
        /// <summary>
        /// Updates filepath string variables for .csv and .txt output.
        /// </summary>
        private static void InitFileOutput()
        {
            if (!Directory.Exists(FILE_OUTPUT_DIRECTORY)) 
            {
                Directory.CreateDirectory(FILE_OUTPUT_DIRECTORY);
            }

            // Warehouse.csv exists
            if (File.Exists(FILE_OUTPUT_DIRECTORY + FileNameBase + ".csv") 
                || File.Exists(FILE_OUTPUT_DIRECTORY + FileNameBase + ".txt"))
            {
                int idSuffix = 1;
                while (File.Exists(FILE_OUTPUT_DIRECTORY + FileNameBase + idSuffix.ToString() + ".csv"))
                {
                    idSuffix++;
                }
                FileNameBase += idSuffix.ToString();
            }
            OUTPUT_FILE_PATH = FILE_OUTPUT_DIRECTORY + FileNameBase;

            using (StreamWriter sw = new StreamWriter(OUTPUT_FILE_PATH + ".csv", true))
            {
                sw.WriteLine("Crate ID,Crate Value,Driver Name,Company,Current Time,Scenario");
            }

            using (StreamWriter sw = new StreamWriter(OUTPUT_FILE_PATH + ".txt", true))
            {
                sw.WriteLine($"This is the output report generated for {FileNameBase}.");
            }
        }
        #endregion

        #region Sim State Functions

        /// <summary>
        /// Main loop that pulses simulation ticks. Before looping, checks for reset condition.
        /// </summary>
        public static void StartSimulation()
        {
            // check to see if restart is necessary before continuing
            if (currentTime != 0)
            {
                Init();
                fastForward = false;
                currentTime = 0;
            }
            

            while(currentTime++ < TIME_INCREMENTS)
            {
                Run();
                if (!fastForward) 
                {
                    Console.WriteLine("\nPress enter to continue to the next time increment.");
                    Console.WriteLine("Enter \"exit\" to fast-forward the simulation.");

                    string input = Console.ReadLine();
                    if (!input.Trim().Equals(""))
                    {
                        if (input.Trim().ToLower().Equals("exit"))
                        {
                            Console.Clear();
                            fastForward = true;
                        }
                    }
                }
            }
            if (!fastForward)
            {
                Console.WriteLine("Press any key to finish the simulation.");
            }
            
            EndSim();
        }
        /// <summary>
        /// The actual simulation tick function.
        /// Warehouse generates new arrival of Trucks and loops through Docks in order to propagate tick.
        /// </summary>
        public static void Run()
        {
            if (!fastForward)
            {
                Console.Clear();
            }
            
            RollNewTrucks();

            // Dequeue Entrance into Dock.Line
            if (!fastForward)
            {
                Console.WriteLine($"Trucks in Entrance: {Entrance.Count}");
            }
            
            if (Entrance.Count > 0)
            {
                Dock toDock = GetShortestLine();
                toDock.JoinLine(Entrance.Dequeue());
            }

            // Loop through Docks
            for (int i = 0; i < Docks.Count; i++)
            {
                Dock dock = Docks[i];
                dock.RunTick();

                // Keep track of longest Line throughout simulation
                if (IsLargestQueue(dock.Line))
                {
                    TruckQueueMax = dock.Line.Count;
                    maxQueueDock = dock.id;
                }
            }
        }

        /// <summary>
        /// Generates reports of simulation info and writes to files.
        /// </summary>
        public static void EndSim()
        {
            #region Data Prep
            List<double> dockAverageUptimes = new List<double>();
            List<double> dockOpCosts = new List<double>();
            double opCostTotal = 0.0;
            int trucksProcessed = 0;
            int cratesProcessed = 0;
            double grossRevenue = 0.0;

            int exTrucks = 0;
            double exValue = 0;

            foreach (Dock dock in Docks)
            {
                Dock.DockStats stats = dock.GetDockInfo();

                grossRevenue += stats.SalesTotal;
                trucksProcessed += stats.TrucksDocked;
                cratesProcessed += stats.CratesUnloaded;
                opCostTotal += Math.Round((double)(stats.TimeInUse * DOCK_OP_COST), 2);
                

                double used = stats.TimeInUse;
                double unused = stats.TimeNotInUse;
                double useAvg = Math.Round(used / (used + unused), 2);

                dockAverageUptimes.Add(useAvg);
                dockOpCosts.Add(stats.TimeInUse * DOCK_OP_COST);

                // Excess
                // Don't include docked truck in Line
                if (dock.Line.Count - 1 > 0)
                {
                    exTrucks += dock.Line.Count - 1;
                    exValue += dock.DoLineDump();
                }
                
            }
            #endregion

            #region Data Write
            const string lineBreak = "==================================";
            const string subBreak =  "- - - - - - - - - - - - - - - - - ";

            grossRevenue = Math.Round(grossRevenue, 2);
            double netRevenue = Math.Round(grossRevenue - opCostTotal,2);
            double avgTruckValue = Math.Round(grossRevenue / trucksProcessed, 2);
            double avgCrateValue = Math.Round(grossRevenue / cratesProcessed, 2);

            using (StreamWriter sw = new StreamWriter(OUTPUT_FILE_PATH + ".txt", false))
            {
                sw.WriteLine($"General Info:" +
                    $"\n{lineBreak}\n" +
                    $"Number of Docks: {Docks.Count}\n" +
                    $"Largest Truck Queue: {TruckQueueMax} at Dock {maxQueueDock}\n" +
                    $"Trucks Processed: {trucksProcessed} trucks\n" +
                    $"Crates Processed: {cratesProcessed} crates\n" +
                    $"Gross Crate Value: ${grossRevenue}\n" +
                    $"Average Crate Value: ${avgCrateValue}\n" +
                    $"Average Truck Value: ${avgTruckValue}\n" +
                    $"Operation Costs: ${opCostTotal}\n" +
                    $"Net Daily Revenue: ${netRevenue}" +
                    $"\n{lineBreak}");

                sw.WriteLine($"Dock Breakdown\n" +
                    $"{lineBreak}");

                for (int i = 0; i < Docks.Count; i++)
                {
                    sw.WriteLine($"{subBreak}\nDock {i}\n" +
                        $"Average Time Used: {dockAverageUptimes[i]} increments\n" +
                        $"Dock Actual Operations Cost: ${dockOpCosts[i]}\n" +
                        $"{subBreak}");
                }

                sw.WriteLine($"{lineBreak}");

                if (exTrucks > 0)
                {
                    sw.WriteLine($"Unrealized Value\n" +
                    $"{lineBreak}\n" +
                    $"At the end of the simulation, {exTrucks} trucks were still in a line at a dock.\n" +
                    $"Their total value was ${Math.Round(exValue, 2)}.");
                }
            }
            
            if (!fastForward)
            {
                Console.Clear();
            }
            #endregion
        }
        #endregion

        #region Run Tick Functions
        // Set of functions that are repeatedly called in order to facilitate a simulation tick.

        /// <summary>
        /// Returns bool indicating if input queue's size is larger than the Warehouse's logged largest truck queue size.
        /// </summary>
        /// <param name="queue">Truck queue to be measured against logged maximum size.</param>
        /// <returns>Boolean value to indicate if queue is larger.</returns>
        static public bool IsLargestQueue(Queue<Truck> queue)
        {
            return TruckQueueMax < queue.Count;
        }

        /// <summary>
        /// Generates a random, normalized decimal value to be used by RollNewTrucks().
        /// </summary>
        /// <param name="CurrentDailyInterval">The current interval number.</param>
        /// <returns>Random normalized decimal which may be decreased based on the value of CurrentDailyInterval.</returns>
        static double GetEntranceBias(int CurrentDailyInterval)
        {
            if (CurrentDailyInterval < 12 || CurrentDailyInterval > 36)
            {
                return -0.25;
            }
            else return 0;
        }

        /// <summary>
        /// Returns the Dock that has the shortest line. Does NOT consider how many crates are in the current Truck.
        /// </summary>
        /// <returns>Dock object with least amount of traffic.</returns>
        static Dock GetShortestLine()
        {
            Dock shortestLine = Docks[0];
            foreach (Dock dock in Docks)
            {
                if (dock.Line.Count == 0 && !dock.Occupied)
                {
                    return dock;
                }
                else if (dock.Line.Count == 0 && dock.Occupied)
                {
                    shortestLine = dock; continue;
                }
                else if (dock.Line.Count < shortestLine.Line.Count)
                {
                    shortestLine = dock;
                }
            }
            return shortestLine;
        }

        /// <summary>
        /// Performs a random number [1-4] of rolls for a Truck to arrive at the Entrance.
        /// </summary>
        static void RollNewTrucks()
        {
            int Rolls = random.Next(1, 5);

            for (int i = 0; i < Rolls; i++)
            {
                double rollResult = random.NextDouble();
                if (rollResult > 0.5f - GetEntranceBias(TIME_INCREMENTS))
                {
                    Entrance.Enqueue(new Truck());
                }
            }
        }
        #endregion
    }
}
