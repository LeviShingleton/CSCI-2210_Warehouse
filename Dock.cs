using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS_Warehouse
{
    internal class Dock
    {
        public string id = "";

        public Queue<Truck> Line = new();
        private Truck? currentTruck = null;
        public bool Occupied => currentTruck != null;

        private double TotalSales;

        private int NumCratesTotal;
        private int NumTrucksTotal;
        private int TimeInUse;
        private int TimeNotInUse;

        /// <summary>
        /// Collection of simulation statistics for the Dock's Run operations. Updated every call of RunTick().
        /// </summary>
        public struct DockRunStats
        {
            public string CrateID;
            public double CrateValue;
            public string Scenario;
            public int TimeInc;
            public string DriverName;
            public string CompanyName;

            public DockRunStats()
            {
                CrateID = "N/A";
                CrateValue = 0;
                Scenario = "N/A";
                TimeInc = 0;
                DriverName = "N/A";
                CompanyName = "N/A";
            }

            public void Clear()
            {
                this = new DockRunStats();
            }
        }
        
        public DockRunStats DockRunInfo = new DockRunStats();

        /// <summary>
        /// Collection of simulation statistics for the Dock's overall utility. Created by GetDockInfo();
        /// </summary>
        public struct DockStats
        {
            public int NumTrucksTotal;
            public int NumCratesTotal;
            public int TimeInUse;
            public int TimeNotInUse;
            public double SalesTotal;

            public DockStats()
            {
                NumTrucksTotal = 0;
                NumCratesTotal = 0;
                TimeInUse = 0;
                TimeNotInUse = 0;
                SalesTotal = 0.0;
            }

            public void Clear()
            {
                this = new DockStats();
            }
        }

        /// <summary>
        /// Adds a Truck object to this Dock's Line queue.
        /// </summary>
        /// <param name="truck">The Truck to be added to Line.</param>
        public void JoinLine(Truck truck)
        {
            Line.Enqueue(truck);
        }
        /// <summary>
        /// Returns the first Truck in Line and removes it from Line.
        /// </summary>
        /// <returns>Truck that was removed from the first location in Line.</returns>
        public Truck SendOff()
        {
            return Line.Dequeue();
        }

        public void RunTick()
        {
            if (currentTruck == null && Line.Count > 0)
            {
                DockRunInfo.Clear();

                currentTruck = Line.Peek();

                DockRunInfo.DriverName = currentTruck.Driver;
                DockRunInfo.CompanyName = currentTruck.DeliveryCompany;
                DockRunInfo.Scenario = "N/A";
            }

            // If a truck is in line,
            // Increment time in use
            // Unload from it


            // If a truck is docked
            if (currentTruck != null)
            {
                TimeInUse++;

                // If the docked truck has crates before unloading
                if (currentTruck.TrailerCount > 0)
                {
                    DockRunInfo.Scenario = "A crate was unloaded, ";
                    DoUnload(currentTruck.Unload());

                    // The truck doesn't have a crate after unloading
                    if (currentTruck.TrailerCount <= 0)
                    {
                        Line.Dequeue();
                        currentTruck = null;
                        NumTrucksTotal++;
                        DockRunInfo.Scenario += "and the truck has no more crates to unload, ";

                        // See if another truck is in Line at the Dock
                        if (Line.Count > 0)
                        {
                            DockRunInfo.Scenario += "and another truck is already in the Dock.";
                        }
                        else
                        {
                            DockRunInfo.Scenario += "but another truck is NOT already in the Dock.";
                        }
                        WriteTickStats();
                        DockRunInfo.Clear();
                    }
                    // If the docked truck still has crates after unloading
                    else
                    {
                        DockRunInfo.Scenario += "but the truck still has more crates to unload.";
                        WriteTickStats();
                    }
                }

                //// If it finished unloading, go ahead and get the next truck in line
                //else if (currentTruck.TrailerCount <= 0)
                //{
                //    if (Line.Count > 0)
                //    {
                //        Line.Dequeue();
                //        Stats.Scenario += "and another truck is already in the Dock.";
                //    }
                //    else
                //    {
                //        Stats.Scenario += "but another truck is NOT already in the Dock.";
                //    }
                //}

            }
            else
            {
                TimeNotInUse++;
                Console.WriteLine($"Dock {id} not in use.");
                if (!DockRunInfo.CrateID.Equals(""))
                { DockRunInfo.Clear(); }
            }
        }

        private void DoUnload(Crate crate)
        {
            DockRunInfo.TimeInc = Warehouse.CurrentTime;
            DockRunInfo.CrateID = crate.ID;
            DockRunInfo.CrateValue = crate.Price;

            TotalSales += crate.Price;
            NumCratesTotal++;
        }

        /// <summary>
        /// Writes a formatted block of text describing current Dock statistics to console.
        /// Automatically calls WriteToCsv();
        /// </summary>
        private void WriteTickStats()
        {
            Console.WriteLine($"\nDock ID: {id}\n" 
                + $"Crate ID: {DockRunInfo.CrateID} \t Crate Value: ${DockRunInfo.CrateValue}\n" +
                $"Driver: {DockRunInfo.DriverName} \t Company: {DockRunInfo.CompanyName}\n" +
                $"Current Time: {DockRunInfo.TimeInc}\n" +
                $"Scenario: {DockRunInfo.Scenario}\n");

            WriteToCsv();
        }

        /// <summary>
        /// Writes RunTick() info to Warehouse CsvOutputFile.
        /// </summary>
        public void WriteToCsv()
        {
            using (StreamWriter sw = new StreamWriter(Warehouse.CsvOutputFile, true))
            {
                sw.WriteLine($"{DockRunInfo.CrateID},${DockRunInfo.CrateValue}," +
                    $"{DockRunInfo.DriverName},\"{DockRunInfo.CompanyName}\"," +
                    $"{DockRunInfo.TimeInc},\"{DockRunInfo.Scenario}\"");
            }
        }

        public DockStats GetDockInfo()
        {
            DockStats DockInfo = new DockStats();
            DockInfo.NumTrucksTotal = NumTrucksTotal;
            DockInfo.NumCratesTotal = NumCratesTotal;
            DockInfo.TimeInUse = TimeInUse;
            DockInfo.TimeNotInUse = TimeNotInUse;

            return DockInfo;
        }
    }
}
