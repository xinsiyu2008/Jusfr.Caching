using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching {
    internal class HttpRuntimeRegionCacheProvider : HttpRuntimeCacheProvider, IHttpRuntimeRegionCacheProvider {
        private String _prefix = null;
        public virtual String Region { get; private set; }

        private String GetPrifix() {
            if (_prefix == null) {
                _prefix = String.Concat("HttpRuntimeRegionCacheProvider_", Region, "_");
            }
            return _prefix;
        }

        public HttpRuntimeRegionCacheProvider(String region)
            : base(true) {
            Region = region;
        }

        public HttpRuntimeRegionCacheProvider(String region, Boolean supportNull = true)
            : base(supportNull) {
            Region = region;
        }

        protected override String BuildCacheKey(String key) {
            //Region 为空将被当作  String.Empty 处理
            return String.Concat(GetPrifix(), base.BuildCacheKey(key));
        }

        protected override Boolean Hit(DictionaryEntry entry) {
            return (entry.Key is String) && ((String)entry.Key).StartsWith(GetPrifix());
        }
    }
}
