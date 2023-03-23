using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS_Warehouse
{
    internal class Dock
    {
        public string id = "";
        public Queue<Truck> Line = new();
        Truck? currentTruck = null;
        public bool Occupied => currentTruck != null;

        double TotalSales;
        int NumCratesTotal;
        int NumTrucksTotal;
        int TimeInUse;
        int TimeNotInUse;

        public struct RunPulseStats
        {
            public string CrateID;
            public double CrateValue;
            public string Scenario;
            public int TimeInc;
            public string DriverName;
            public string CompanyName;

            public RunPulseStats()
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
                this = new RunPulseStats();
            }
        }

        RunPulseStats Stats = new RunPulseStats();

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
                Stats.Clear();

                currentTruck = Line.Peek();

                Stats.DriverName = currentTruck.Driver;
                Stats.CompanyName = currentTruck.DeliveryCompany;
                Stats.Scenario = "N/A";
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
                    Stats.Scenario = "A crate was unloaded, ";
                    DoUnload(currentTruck.Unload());

                    // The truck doesn't have a crate after unloading
                    if (currentTruck.TrailerCount <= 0)
                    {
                        Line.Dequeue();
                        currentTruck = null;
                        NumTrucksTotal++;
                        Stats.Scenario += "and the truck has no more crates to unload, ";

                        // See if another truck is in Line at the Dock
                        if (Line.Count > 0)
                        {
                            Stats.Scenario += "and another truck is already in the Dock.";
                        }
                        else
                        {
                            Stats.Scenario += "but another truck is NOT already in the Dock.";
                        }
                        WriteTickStats();
                        Stats.Clear();
                    }
                    // If the docked truck still has crates after unloading
                    else
                    {
                        Stats.Scenario += "but the truck still has more crates to unload.";
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
                if (!Stats.CrateID.Equals(""))
                { Stats.Clear(); }
            }
        }

        private void DoUnload(Crate crate)
        {
            Stats.TimeInc = Warehouse.CurrentTime;
            Stats.CrateID = crate.ID;
            Stats.CrateValue = crate.Price;

            TotalSales += crate.Price;
            NumCratesTotal++;
        }

        private void WriteTickStats()
        {
            Console.WriteLine($"\nDock ID: {id}\n" 
                + $"Crate ID: {Stats.CrateID} \t Crate Value: ${Stats.CrateValue}\n" +
                $"Driver: {Stats.DriverName} \t Company: {Stats.CompanyName}\n" +
                $"Current Time: {Stats.TimeInc}\n" +
                $"Scenario: {Stats.Scenario}\n");

        }
    }
}
