using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS_Warehouse
{
    internal class Crate
    {
        const int PRICE_MIN = 50;
        const int PRICE_MAX = 500;

        public string ID = "ID";

        private double _price = Math.Round(Warehouse.random.Next(PRICE_MIN, PRICE_MAX) + (1 - Warehouse.random.NextDouble()), 2);
        public double Price
        {
            get { return _price; }
        }

        public Crate()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                sb.Append(Warehouse.random.Next(10).ToString());
            }
            ID += sb.ToString();
        }
    }
}
