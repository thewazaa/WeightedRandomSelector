﻿namespace DataStructures.RandomSelector
{
    using DataStructures.RandomSelector.Math;

    /// <summary>
    /// Uses Binary Search for picking random items
    /// Good for large sized number of items
    /// </summary>
    /// <typeparam name="T">Type of items you wish this selector returns</typeparam>
    public class StaticRandomSelectorBinary<T> : IRandomSelector<T>
    {
        private readonly System.Random _random;
        private readonly T[] _items;
        private readonly float[] _CDA;

        /// <summary>
        /// Constructor, used by StaticRandomSelectorBuilder
        /// Needs array of items and CDA (Cummulative Distribution Array). 
        /// </summary>
        /// <param name="items">Items of type T</param>
        /// <param name="CDA">Cummulative Distribution Array</param>
        /// <param name="seed">Seed for internal random generator</param>
        public StaticRandomSelectorBinary(T[] items, float[] CDA, int seed)
        {

            this._items = items;
            this._CDA = CDA;
            this._random = new System.Random(seed);
        }

        /// <summary>
        /// Selects random item based on their weights.
        /// Uses binary search for random selection.
        /// </summary>
        /// <returns>Returns item</returns>
        public T SelectRandomItem()
        {

            float randomValue = (float)_random.NextDouble();

            return _items[_CDA.SelectIndexBinarySearch(randomValue)];
        }

        /// <summary>
        /// Selects random item based on their weights.
        /// Uses binary search for random selection.
        /// </summary>
        /// <param name="randomValue">Random value from your uniform generator</param>
        /// <returns>Returns item</returns>
        public T SelectRandomItem(float randomValue)
        {

            return _items[_CDA.SelectIndexBinarySearch(randomValue)];
        }
    }
}