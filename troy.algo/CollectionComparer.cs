using System;
using System.Collections.Generic;
using System.Linq;

namespace Tao.Venus.Util
{
    /// <summary>
    /// Comparing two collection of items based on their keys and values.
    /// 
    /// The two collection are first sorted based on their keys. The key sort
    /// algorithm are provided by users. Once sorted, two collection are 
    /// first compared side by side based on the keys. If one item in either
    /// collection is missing in the other collection, meaning there is no item
    /// bearing the same key, a corresponding action will be invoked. For course
    /// the action is provided by user.
    /// 
    /// If two items are found to have same key, they are then sent to another 
    /// user provided functor for fully comparing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class CollectionComparer<T, TKey>
    {
        public CollectionComparer(Func<T, TKey> keySelector)
        {
            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
            {
                KeyComparing = new ComparerDelegate<TKey>();
            }

            this.KeySelector = keySelector;
            this.FullComparison = (lhs, rhs) => true;
            this.OnLeftOdd = value => { };
            this.OnRightOdd = value => { };
            this.OnPropertyUnequal = (lhs, rhs) => { };
        }

        /// <summary>
        /// Functor to select the key.
        /// 
        /// Example 1: The value itself is the key1
        /// 
        /// var cpr = new CollectionComparer<string, string>();
        /// cpr.KeySelector = value => value;
        /// 
        /// Example 2: For model Person, user its name as key
        /// 
        /// class Person {
        ///     string Name {get; set;}
        ///     int Age {get; set;}
        ///     ...
        /// }
        /// 
        /// var cpr = new CollectionComparer<Person, string>();
        /// cpr.KeySelector = value => value.Name;
        /// 
        /// </summary>
        public Func<T, TKey> KeySelector { get; set; }

        /// <summary>
        /// The comparing algorithm for key.
        /// 
        /// This property shall not be NULL unless the T is IComparable
        /// </summary>
        public IComparer<TKey> KeyComparing { get; set; }

        /// <summary>
        /// The functor to determine if two values are exactly equal besides their keys.
        /// 
        /// Only values with identical keys are compared by this functor. If this property
        /// shall not be NULL.
        /// </summary>
        public Func<T, T, bool> FullComparison { get; set; }

        /// <summary>
        /// The action is invoked once a value in the left collection is found to be 
        /// missing in the right collection.
        /// 
        /// The comparing is only based on key.
        /// </summary>
        public Action<T> OnLeftOdd { get; set; }

        /// <summary>
        /// The action is invoked once a value in the right collection is found to be
        /// missing in the left collection.
        ///
        /// The comparing is only based on key.
        /// </summary>
        public Action<T> OnRightOdd { get; set; }

        /// <summary>
        /// The action is invoked when two values bearing same key are found to be
        /// unequal by FullComparison functor
        /// </summary>
        public Action<T, T> OnPropertyUnequal { get; set; }

        /// <summary>
        /// Comparing two collection of values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void Compare(IEnumerable<T> left, IEnumerable<T> right)
        {
            var l = PreSort(left).GetEnumerator();
            var r = PreSort(right).GetEnumerator();

            var cp = 0;
            bool leftDone = !l.MoveNext();
            bool rightDone = !r.MoveNext();

            while (!leftDone && !rightDone)
            {
                var vl = l.Current;
                var vr = r.Current;
                cp = CompareByKey(vl, vr);

                if (cp < 0)
                {
                    OnLeftOdd(vl);
                    leftDone = !l.MoveNext();
                }
                else if (cp > 0)
                {
                    OnRightOdd(vr);
                    rightDone = !r.MoveNext();
                }
                else
                {
                    if (!FullComparison(vl, vr))
                    {
                        OnPropertyUnequal(vl, vr);
                    }

                    leftDone = !l.MoveNext();
                    rightDone = !r.MoveNext();
                }
            }

            if (leftDone && rightDone)
            {
                // reach the end at the same time
            }
            else if (leftDone)
            {
                do
                {
                    OnRightOdd(r.Current);
                } while (r.MoveNext());
            }
            else if (rightDone)
            {
                do
                {
                    OnLeftOdd(l.Current);
                } while (l.MoveNext());
            }
        }

        private IOrderedEnumerable<T> PreSort(IEnumerable<T> input)
        {
            return input.OrderBy(KeySelector, KeyComparing);
        }

        private int CompareByKey(T left, T right)
        {
            return KeyComparing.Compare(KeySelector.Invoke(left), KeySelector.Invoke(right));
        }

        private class ComparerDelegate<K> : IComparer<K>
        {
            public int Compare(K x, K y)
            {
                return (x as IComparable<K>).CompareTo(y);
            }
        }
    }
}
