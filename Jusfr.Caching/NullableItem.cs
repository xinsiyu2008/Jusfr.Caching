using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching {
    public struct NullableEntry<T> {
        private T entry;
        private Boolean isNull;

        public NullableEntry(T value) {
            entry = value;
            isNull = (value == null);
        }

        public static implicit operator NullableEntry<T>(T value) {
            return new NullableEntry<T>(value);
        }

        public static implicit operator T(NullableEntry<T> value) {
            if (value.isNull) {
                return (T)((Object)null);
            }
            return value.entry;
        }
    }
}
