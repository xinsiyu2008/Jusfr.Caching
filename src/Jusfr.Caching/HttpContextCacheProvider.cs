using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Jusfr.Caching {
    public class HttpContextCacheProvider : CacheProvider, ICacheProvider {
        private const String _prefix = "HCCP_";
        protected override String BuildCacheKey(String key) {
            return String.Concat(_prefix, key);
        }

        private Boolean InnerTryGet(String key, out Object entry) {
            Boolean exist = false;
            entry = null;
            if (HttpContext.Current.Items.Contains(key)) {
                exist = true;
                entry = HttpContext.Current.Items[key];
            }
            return exist;
        }

        public override bool TryGet<T>(string key, out T entry) {
            String cacheKey = BuildCacheKey(key);
            Object cacheEntry;
            Boolean exist = InnerTryGet(cacheKey, out cacheEntry);
            if (!exist) {
                entry = default(T);
                return false;
            }

            if (cacheEntry == null) {
                entry = (T)((Object)null);
                return true;
            }
            if (cacheEntry is T) {
                entry = (T)cacheEntry;
                return true;
            }
            else {
                //cacheEntry is not a t
                throw new InvalidOperationException(String.Format("缓存项`[{0}]`类型错误, {1} or {2} ?",
                    key, cacheEntry.GetType().FullName, typeof(T).FullName));
            }
        }

        public override void Overwrite<T>(String key, T entry) {
            HttpContext.Current.Items[BuildCacheKey(key)] = entry;
        }

        public override void Expire(String key) {
            HttpContext.Current.Items.Remove(BuildCacheKey(key));
        }
    }
}
