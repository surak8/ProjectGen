@echo off
setlocal
msbuild -nologo projectgen.sln -t:rebuild -p:configuration=release
copy /y projectgen.pdb \usr\local\bin 
copy /y projectgen.exe \usr\local\bin 
