using System.Collections.Generic;
using System;
using DataStructures.RandomSelector.Math;

namespace DataStructures.RandomSelector
{
    /// <summary>
    /// DynamicRandomSelector allows you adding or removing items.
    /// Call "Build" after you finished modification.
    /// Switches between linear or binary search after each Build(), 
    /// depending on count of items, making it more performant for general use case.
    /// </summary>
    /// <typeparam name="T">Type of items you wish this selector returns</typeparam>
    public class DynamicRandomSelector<T> : IRandomSelector<T>, IRandomSelectorBuilder<T>
    {
        private Random random;

        // internal buffers
        private readonly List<T> _itemsList;
        private readonly List<float> _weightsList;
        private readonly List<float> _CDL; // Cummulative Distribution List

        // internal function that gets dynamically swapped inside Build
        private Func<List<float>, float, int> selectFunction;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="seed">Leave it -1 if you want seed to be randomly picked</param>
        /// <param name="expectedNumberOfItems">Set this if you know how much items the collection will hold, to minimize Garbage Collection</param>
        public DynamicRandomSelector(int seed = -1, int expectedNumberOfItems = 32)
        {

            if (seed == -1)
                random = new Random();
            else
                random = new Random(seed);

            _itemsList = new List<T>(expectedNumberOfItems);
            _weightsList = new List<float>(expectedNumberOfItems);
            _CDL = new List<float>(expectedNumberOfItems);
        }

        /// <summary>
        /// Constructor, where you can preload collection with items/weights array. 
        /// </summary>
        /// <param name="items">Items that will get returned on random selections</param>
        /// <param name="weights">Un-normalized weights/chances of items, should be same length as items array</param>
        public DynamicRandomSelector(T[] items, float[] weights) : this()
        {
            for (int i = 0; i < items.Length; i++)
                Add(items[i], weights[i]);

            Build();
        }

        /// <summary>
        /// Constructor, where you can preload collection with items/weights list. 
        /// </summary>
        /// <param name="items">Items that will get returned on random selections</param>
        /// <param name="weights">Un-normalized weights/chances of items, should be same length as items array</param>
        public DynamicRandomSelector(List<T> items, List<float> weights) : this()
        {
            for (int i = 0; i < items.Count; i++)
                Add(items[i], weights[i]);

            Build();
        }

        /// <summary>
        /// Clears internal buffers, should make no garbage (unless internal lists hold objects that aren't referenced anywhere else)
        /// </summary>
        public void Clear()
        {

            _itemsList.Clear();
            _weightsList.Clear();
            _CDL.Clear();
        }

        /// <summary>
        /// Add new item with weight into collection. Items with zero weight will be ignored.
        /// Do not add duplicate items, because removing them will be buggy (you will need to call remove for duplicates too!).
        /// Be sure to call Build() after you are done adding items.
        /// </summary>
        /// <param name="item">Item that will be returned on random selection</param>
        /// <param name="weight">Non-zero non-normalized weight</param>
        public void Add(T item, float weight)
        {

            // ignore zero weight items
            if (weight == 0)
                return;

            _itemsList.Add(item);
            _weightsList.Add(weight);
        }

        /// <summary>
        /// Remove existing item with weight into collection.
        /// Be sure to call Build() after you are done removing items.
        /// </summary>
        /// <param name="item">Item that will be removed out of collection, if found</param>
        public void Remove(T item)
        {
            int index = _itemsList.IndexOf(item);

            // nothing was found
            if (index == -1)
                return;

            _itemsList.RemoveAt(index);
            _weightsList.RemoveAt(index);
            // no need to remove from CDL, should be rebuilt instead
        }

        /// <summary>
        /// Retrieves a dictionary of the full set of items
        /// </summary>
        /// <returns>The dictionary</returns>
        public Dictionary<T, float> GetItems()
        {
            Dictionary<T, float> items = new Dictionary<T, float>();

            for (int i = 0; i < _itemsList.Count; i++)
            {
                items.Add(_itemsList[i], _weightsList[i]);
            }

            return items;
        }
        
        /// <summary>
        /// Re/Builds internal CDL (Cummulative Distribution List)
        /// Must be called after modifying (calling Add or Remove), or it will break. 
        /// Switches between linear or binary search, depending on which one will be faster.
        /// Might generate some garbage (list resize) on first few builds.
        /// </summary>
        /// <param name="seed">You can specify seed for internal random gen or leave it alone</param>
        /// <returns>Returns itself</returns>
        public IRandomSelector<T> Build(int seed = -1)
        {

            if (_itemsList.Count == 0)
                throw new Exception("Cannot build with no items.");

            // clear list and then transfer weights
            _CDL.Clear();
            for (int i = 0; i < _weightsList.Count; i++)
                _CDL.Add(_weightsList[i]);

            RandomMath.BuildCumulativeDistribution(_CDL);

            // default behavior
            // if seed wasn't specified (it is seed==-1), keep same seed - avoids garbage collection from making new random
            if (seed != -1)
            {

                // input -2 if you want to randomize seed
                if (seed == -2)
                {
                    seed = random.Next();
                    random = new Random(seed);
                }
                else
                {
                    random = new Random(seed);
                }
            }

            // RandomMath.ListBreakpoint decides where to use Linear or Binary search, based on internal buffer size
            // if CDL list is smaller than breakpoint, then pick linear search random selector, else pick binary search selector
            if (_CDL.Count < RandomMath.ListBreakpoint)
                selectFunction = RandomMath.SelectIndexLinearSearch;
            else
                selectFunction = RandomMath.SelectIndexBinarySearch;

            return this;
        }

        /// <summary>
        /// Selects random item based on its probability.
        /// Uses linear search or binary search, depending on internal list size.
        /// </summary>
        /// <param name="randomValue">Random value from your uniform generator</param>
        /// <returns>Returns item</returns>
        public T SelectRandomItem(float randomValue)
        {
            return _itemsList[selectFunction(_CDL, randomValue)];
        }

        /// <summary>
        /// Selects random item based on its probability.
        /// Uses linear search or binary search, depending on internal list size.
        /// </summary>
        /// <returns>Returns item</returns>
        public T SelectRandomItem()
        {
            float randomValue = (float)random.NextDouble();
            return _itemsList[selectFunction(_CDL, randomValue)];
        }
    }
}