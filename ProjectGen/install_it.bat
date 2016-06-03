@echo off
setlocal
msbuild -nologo projectgen.sln -t:rebuild -p:configuration=release -v:m
if exist projectgen.pdb copy /y projectgen.pdb \usr\local\bin 
if exist projectgen.xml copy /y projectgen.xml \usr\local\bin 
copy /y projectgen.exe \usr\local\bin 
