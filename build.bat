msbuild src\Jusfr.Caching\Jusfr.Caching.csproj
robocopy src\Jusfr.Caching\bin\Debug debug\Jusfr.Caching /mir

msbuild src\Jusfr.Caching.Memcached\Jusfr.Caching.Memcached.csproj
robocopy src\Jusfr.Caching.Memcached\bin\Debug debug\Jusfr.Caching.Memcached /mir

msbuild src\Jusfr.Caching.Mongodb
robocopy src\Jusfr.Caching.Mongodb\bin\Debug debug\Jusfr.Caching.Mongodb /mir