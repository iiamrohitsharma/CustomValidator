using System;
using System.Collections.Generic;
using System.Text;

namespace CustomValidator.Extensions
{
    /// <summary>
    /// Arrays extensions.
    /// </summary>
    public static class ArrayExtension
    {
        #region Remove Duplicates
        /// <summary>
        /// Removes the duplicates items from array.
        /// </summary>
        /// <returns>The duplicates.</returns>
        /// <param name="items">The items.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] RemoveDuplicates<T>(this T[] items)
        {
            List<T> uniqueItems = new List<T>();

            for (int i = 0; i < items.Length; i++)
            {
                if (!uniqueItems.Contains(items[i]))
                {
                    uniqueItems.Add(items[i]);
                }
            }

            return uniqueItems.ToArray();
        }
        #endregion
    }
}
