﻿namespace AgilSystemutveckling_Xamarin_Net5.TestModels
{
    public class Inventory
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public int Stock { get; set; }
        public bool InStock { get; set; }
    }
}
