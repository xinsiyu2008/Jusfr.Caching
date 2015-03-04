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

        protected override Boolean InnerTryGet(String key, out Object entry) {
            Boolean exist = false;
            entry = null;
            if (HttpContext.Current.Items.Contains(key)) {
                exist = true;
                entry = HttpContext.Current.Items[key];
            }
            return exist;
        }

        public override void Overwrite<T>(String key, T entry) {
            HttpContext.Current.Items[BuildCacheKey(key)] = entry;
        }

        public override void Expire(String key) {
            HttpContext.Current.Items.Remove(BuildCacheKey(key));
        }
    }
}
