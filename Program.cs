namespace AS_Warehouse
{
    internal class Program
    {
        //TODO
        static int repeatCount = 1;
        static void Main(string[] args)
        {
            do
            {
                Console.Clear();

                Warehouse.StartSimulation();

                Console.WriteLine("Simulation completed. You may find records of the results within the Warehouse Output folder\n" +
                    "in the VS project folder.");
            }
            while (GetRestart());
        }


        /// <summary>
        /// Post-sim prompt to determine if user wants to start a new simulation.
        /// </summary>
        /// <returns>Interpreted Boolean value  of user input.</returns>
        static bool GetRestart()
        {
            Console.WriteLine($"Would you like to start a new simulation?" +
                $"\n Y/N");
            string input = Console.ReadLine().Trim().ToUpper();

            bool Y = input.Equals("Y");
            bool N = input.Equals("N");
            bool Yes = input.Equals("YES");
            bool No = input.Equals("NO");

            while (!(Y || N) && !(Yes || No))
            {
                Console.Clear();
                Console.WriteLine("Please enter Y/N.");
                input = Console.ReadLine().ToUpper();
            }

            if (input.Equals("Y") || input.Equals("YES"))
            {
                return true;
            }
            else if (input.Equals("N") || input.Equals("NO"))
            {
                return false;
            }
            else return false;
        }

        //TODO
        /// <summary>
        /// Prompt option to make current simulation config repeat itself for a user-defined amount of times.
        /// </summary>
        static void GetRepeatCount()
        {
            Console.WriteLine($"How many times should the simulation run consecutively?" +
                $"\n Default: 1 time");
            string input = Console.ReadLine();

            if (input.Equals(""))
            {
                return;
            }

            while (!(int.TryParse(input.Trim(), out int newNum) && newNum > 0))
            {
                Console.WriteLine("Please enter a valid number.");
                input = Console.ReadLine();
            }

            repeatCount = int.Parse(input.Trim());
        }
    }
}