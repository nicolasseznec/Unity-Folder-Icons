/* 
 * From Aarthificial and INeatFreak's Gist :
 * https://gist.github.com/INeatFreak/e01763f844336792ebe07c1cd1b6d018
 */

using System;
using UnityEngine;


namespace FolderIcons
{
    /// <summary>
    /// Pattern that allows for optional variables that can be enabled or disabled
    /// </summary>
    [Serializable]
    public struct Optional<T>
    {
        [SerializeField] private bool enabled;
        [SerializeField] private T value;

        public bool Enabled => enabled;
        public T Value => value;

        public Optional(T initialValue)
        {
            enabled = true;
            value = initialValue;
        }

        public Optional(T initialValue, bool initialEnabled)
        {
            enabled = initialEnabled;
            value = initialValue;
        }

        // Conversion operators
        public static implicit operator Optional<T>(T v)
        {
            return new Optional<T>(v);
        }

        public static implicit operator T(Optional<T> o)
        {
            return o.Value;
        }

        // If statements
        public static implicit operator bool(Optional<T> o)
        {
            return o.enabled;
        }

        // Equal operators
        public static bool operator ==(Optional<T> lhs, Optional<T> rhs)
        {
            if (lhs.value is null)
            {
                if (rhs.value is null)
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles the case of null on right side.
            return lhs.value.Equals(rhs.value);
        }

        public static bool operator !=(Optional<T> lhs, Optional<T> rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString() => value.ToString();
    }
}