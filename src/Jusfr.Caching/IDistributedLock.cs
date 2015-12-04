using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jusfr.Caching {

    public interface IDistributedLock {
        IDisposable Lock(String key);
        void Lock(String key, Int32 timeoutSecond);
        Boolean TryLock(String key, Int32 timeoutSecond );
        void UnLock(String key);
    }
}
