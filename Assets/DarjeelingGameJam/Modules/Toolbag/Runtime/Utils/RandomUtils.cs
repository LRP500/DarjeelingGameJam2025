using UnityEngine;

namespace Modules.Toolbag.Utils
{
    public static class RandomUtils
    {
        /// <summary>
        /// Returns a random value between 0 [inclusive] and 100 [inclusive].
        /// </summary>
        /// <returns></returns>
        public static int D100()
        {
            return Random.Range(0, 101);
        }

        /// <summary>
        /// Flips a coin.
        /// </summary>
        /// <returns></returns>
        public static bool CoinFlip()
        {
            return Random.Range(0, 2) == 0;
        }
        
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static bool EvaluatePercentage(float percentage)
        {
            return Random.Range(0, 100) <= percentage;
        }
        
        /// <summary>
        /// Returns random index in array of weights.
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int GetRandomWeightedIndex(int[] weights)
        {
            // Get the total sum of all the weights.
            var length = weights.Length;
            var weightSum = 0;
            
            for (var i = 0; i < length; ++i)
            {
                weightSum += weights[i];
            }
 
            // Step through all the possibilities, one by one, checking to see if each one is selected.
            var index = 0;
            var lastIndex = length - 1;
            
            while (index < lastIndex)
            {
                // Do a probability check with a likelihood of weights[index] / weightSum.
                if (Random.Range(0, weightSum) < weights[index])
                {
                    return index;
                }
 
                // Remove the last item from the sum of total untested weights and try again.
                weightSum -= weights[index++];
            }
 
            // No other item was selected, so return very last index.
            return index;
        }
    }
}