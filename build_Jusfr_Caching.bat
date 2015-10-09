@setlocal 
@set local=%~dp0
@pushd %WINDIR%\Microsoft.NET\Framework\v4.0.30319\
@goto build


:build
msbuild "%local%src\Jusfr.Caching.sln" /t:Rebuild /P:Configuration=Release
@goto copy

:copy
robocopy "%local%src\Jusfr.Caching\bin\Release" %local%release /mir
robocopy "%local%src\Jusfr.Caching.Memcached\bin\Release" %local%release /e
robocopy "%local%src\Jusfr.Caching.Redis\bin\Release" %local%release /e
@goto end

:end
@pushd %local%
@pause