using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS_Warehouse
{

    internal class Truck
    {
        #region Driver Names
        string[] d_first = { "Jerry", "Carlee", "Brian", "Stephanie", "Nick", "Amanda", "Jack", "Susie", "Marcus", "Linda" };
        string[] d_last = { "Kurzyck", "Crosston", "Vines", "Reissman", "Hart", "Whitlock", "Barr", "Knapp", "Santiago", "Mathews" };
        #endregion
        #region Company Names
        string[] c_names = { "That One Shipping Co.", "Defex", "MePS", "Omozon, Inc." };
        #endregion

        public string Driver = "";
        public string DeliveryCompany = "";
        Stack<Crate> Trailer = new();

        public Truck()
        {
            Driver = d_first[Warehouse.random.Next(d_first.Length)] + " " + d_last[Warehouse.random.Next(d_last.Length)];
            DeliveryCompany = c_names[Warehouse.random.Next(c_names.Length)];

            for (int i = 0; i < Warehouse.random.Next(4,13); i++)
            {
                Trailer.Push(new Crate());
            }
        }
        public int TrailerCount
        {
            get => Trailer.Count;
        }
        /// <summary>
        /// Adds a Crate to the Truck's trailer, LIFO order.
        /// </summary>
        /// <param name="crate">The Crate to be added to the Trailer.</param>
        public void Load(Crate crate)
        {
            Trailer.Push(crate);
        }
        /// <summary>
        /// Removes a Crate from the Truck's Trailer.
        /// </summary>
        /// <returns>The Crate that was removed.</returns>
        public Crate Unload()
        {
            return Trailer.Pop();
        }

    }

}
