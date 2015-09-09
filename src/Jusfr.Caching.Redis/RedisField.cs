using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching.Redis {
    public struct RedisField : IEquatable<RedisField> {
        private String key1;
        private byte[] key2;

        public Boolean HasValue {
            get {
                return key1 != null || key2 != null;
            }
        }

        public static Boolean operator ==(RedisField f1, RedisField f2) {
            return f1.Equals(f2);
        }

        public static Boolean operator !=(RedisField f1, RedisField f2) {
            return !f1.Equals(f2);
        }

        public override int GetHashCode() {
            return ((String)this).GetHashCode();
        }

        public override bool Equals(Object obj) {
            if (obj == null) {
                return false;
            }
            if (!(obj is RedisField)) {
                return false;
            }

            return base.Equals((RedisField)obj);
        }

        public static implicit operator RedisField(String key) {
            return new RedisField() { key1 = key };
        }

        public static implicit operator RedisField(Byte[] key) {
            return new RedisField() { key2 = key };
        }

        public static implicit operator String(RedisField key) {
            if (key.key1 != null) {
                return key.key1;
            }
            if (key.key2 != null) {
                key.key1 = Encoding.UTF8.GetString(key.key2);
                return key.key1;
            }
            return null;
        }

        public static implicit operator byte[] (RedisField key) {
            if (key.key2 != null) {
                return key.key2;
            }
            if (key.key1 != null) {
                key.key2 = Encoding.UTF8.GetBytes(key.key1);
                return key.key2;
            }
            return null;
        }

        public override String ToString() {
            return (String)this;
        }

        public bool Equals(RedisField other) {
            if ((HasValue && !other.HasValue) || ((!HasValue && other.HasValue))) {
                return false;
            }
            if (!HasValue && !other.HasValue) {
                return true;
            }

            return ((String)this).Equals((String)other, StringComparison.OrdinalIgnoreCase);
        }
    }

    public struct RedisEntry : IEquatable<RedisEntry> {
        public RedisField Name { get; private set; }
        public RedisField Value { get; private set; }

        public RedisEntry(RedisField name, RedisField value) {
            Name = name;
            Value = value;
        }

        public bool Equals(RedisEntry other) {
            return Name.Equals(other.Name) && Value.Equals(other.Value);
        }

        public static implicit operator RedisEntry(KeyValuePair<RedisField, RedisField> value) {
            return new RedisEntry {
                Name = value.Key,
                Value = value.Value
            };
        }

        public static implicit operator KeyValuePair<RedisField, RedisField>(RedisEntry value) {
            return new KeyValuePair<RedisField, RedisField>(value.Name, value.Value);
        }

        public static Boolean operator ==(RedisEntry re1, RedisEntry re2) {
            return re1.Name.Equals(re2.Name) && re1.Value.Equals(re2.Value);
        }

        public static Boolean operator !=(RedisEntry re1, RedisEntry re2) {
            return !re1.Name.Equals(re2.Name) || !re1.Value.Equals(re2.Value);
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (!(obj is RedisEntry)) {
                return false;
            }

            return this.Equals((RedisEntry)obj);
        }

        public override int GetHashCode() {
            return Name.GetHashCode() ^ Value.GetHashCode();
        }
    }
}
