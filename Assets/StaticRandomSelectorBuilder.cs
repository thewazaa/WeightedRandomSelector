using DataStructures.RandomSelector.Math;
using System;
using System.Collections.Generic;

/// <summary>
/// By Vili Volcini
/// Generic structure for high performance random selection of items on very big arrays
/// Is precompiled/precomputed, and uses binary search to find item based on random
/// O(log2(N)) per random pick for bigger arrays
/// O(n) per random pick for smaller arrays
/// O(n) construction
/// </summary>
namespace DataStructures.RandomSelector
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RandomSelectorBuilder<T> : IRandomSelectorBuilder<T>
    {
        private readonly Random _random;
        private readonly List<T> _itemBuffer;
        private readonly List<float> _weightBuffer;

        private static readonly RandomSelectorBuilder<T> _staticBuilder = new RandomSelectorBuilder<T>();

        public RandomSelectorBuilder()
        {

            _random = new System.Random();
            _itemBuffer = new List<T>();
            _weightBuffer = new List<float>();
        }

        /// <summary>
        /// Add new item with weight into collection. Items with zero weight will be ignored.
        /// Be sure to call Build() after you are done adding items.
        /// </summary>
        /// <param name="item">Item that will be returned on random selection</param>
        /// <param name="weight">Non-zero non-normalized weight</param>
        public void Add(T item, float weight)
        {

            // ignore zero weight items
            if (weight == 0)
                return;

            _itemBuffer.Add(item);
            _weightBuffer.Add(weight);
        }

        /// <summary>
        /// Builds StaticRandomSelector & clears internal buffers. Must be called after you finish Add-ing items.
        /// </summary>
        /// <param name="seed">Seed for random selector. If you leave it -1, the internal random will generate one.</param>
        /// <returns>Returns IRandomSelector, underlying objects are either StaticRandomSelectorLinear or StaticRandomSelectorBinary. Both are non-mutable.</returns>
        public IRandomSelector<T> Build(int seed = -1)
        {

            T[] items = _itemBuffer.ToArray();
            float[] CDA = _weightBuffer.ToArray();

            _itemBuffer.Clear();
            _weightBuffer.Clear();

            RandomMath.BuildCumulativeDistribution(CDA);

            if (seed == -1)
                seed = _random.Next();

            // RandomMath.ArrayBreakpoint decides where to use Linear or Binary search, based on internal buffer size
            // if CDA array is smaller than breakpoint, then pick linear search random selector, else pick binary search selector
            if (CDA.Length < RandomMath.ArrayBreakpoint)

                return new StaticRandomSelectorLinear<T>(items, CDA, seed);
            else
                // bigger array sizes need binary search for much faster lookup
                return new StaticRandomSelectorBinary<T>(items, CDA, seed);
        }

        /// <summary>
        /// non-instance based, single threaded only. For ease of use. 
        /// Build from array of items/weights.
        /// </summary>
        /// <param name="itemsArray">Array of items</param>
        /// <param name="weightsArray">Array of non-zero non-normalized weights. Have to be same length as itemsArray.</param>
        /// <returns></returns>
        public static IRandomSelector<T> Build(T[] itemsArray, float[] weightsArray)
        {

            _staticBuilder.Clear();

            for (int i = 0; i < itemsArray.Length; i++)
                _staticBuilder.Add(itemsArray[i], weightsArray[i]);

            return _staticBuilder.Build();
        }


        /// <summary>
        /// non-instance based, single threaded only. For ease of use. 
        /// Build from array of items/weights.
        /// </summary>
        /// <param name="itemsList">List of weights</param>
        /// <param name="weightsList">List of non-zero non-normalized weights. Have to be same length as itemsList.</param>
        /// <returns></returns>
        public static IRandomSelector<T> Build(List<T> itemsList, List<float> weightsList)
        {

            _staticBuilder.Clear();

            for (int i = 0; i < itemsList.Count; i++)
                _staticBuilder.Add(itemsList[i], weightsList[i]);

            return _staticBuilder.Build();
        }

        private void Clear()
        {

            _itemBuffer.Clear();
            _weightBuffer.Clear();
        }
    }
}