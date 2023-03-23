using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS_Warehouse
{
    internal static class Warehouse
    {
        static public Random random = new Random();

        const int MAX_DOCKS = 15;
        const int TIME_INCREMENTS = 48;

        public static int CurrentTime
        {
            get { return currentTime; }
        }

        private static int currentTime = 0;
        static List<Dock> Docks = new();
        static Queue<Truck> Entrance = new();

        #region Statistics Variables
        #region Truck
        static private int _truckQueueMax = 0;
        static public int TruckQueueMax
        {
            get { return _truckQueueMax; }
            set { _truckQueueMax = value; }
        }

        static private int _truckCount = 0;
        static public int TruckCount
        {
            get { return _truckCount; }
            set { _truckCount = value; }
        }
        static private double _truckValueTotal = 0;
        static public double TruckValueTotal
        {
            get { return _truckValueTotal; }
            private set { _truckValueTotal = value; }
        }
        #endregion
        #region Crate
        static private int _crateCount
        {
            get { return _crateCount; }
            set { _crateCount = value; }
        }
        static public int CrateCount
        {
            get { return _crateCount; }
            set { _crateCount = value; }
        }
        #endregion
        #endregion

        static Warehouse()
        {
            InitDockList();
        }

        private static void InitDockList()
        {
            for (int i = 0; i < random.Next(1, MAX_DOCKS + 1); i++)
            {
                Dock tmp = new Dock();
                tmp.id = i.ToString();
                Docks.Add(tmp);
            }
        }

        public static void StartSimulation()
        {
            currentTime = 0;
            while(currentTime++ < TIME_INCREMENTS)
            {
                Run();
                Console.WriteLine("Press any key to continue to the next time increment.");
                Console.ReadKey();
            }
        }
        public static void Run()
        {
            Console.Clear();
            RollNewTrucks();

            Console.WriteLine($"Trucks in Entrance: {Entrance.Count}");
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

                if (dock.Line.Count > TruckQueueMax)
                {
                    TruckQueueMax = dock.Line.Count;
                }
                
                
            }
            // Dequeue Entrance into Dock.Line
            // Keep track of longest Line
            // Trucks should pop a crate
        }

        /// <summary>
        /// Returns bool indicating if input queue's size is larger than the Warehouse's logged largest truck queue size.
        /// </summary>
        /// <param name="queue">Truck queue to be measured against logged maximum size.</param>
        /// <returns>Boolean value to indicate if queue is larger.</returns>
        static public bool IsLargestQueue(Queue<Truck> queue)
        {
            return TruckQueueMax < queue.Count;
        }

        static double GetEntranceBias(int CurrentDailyInterval)
        {
            if (CurrentDailyInterval < 12 || CurrentDailyInterval > 36)
            {
                return -0.25;
            }
            else return 0;
        }

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
    }
}
