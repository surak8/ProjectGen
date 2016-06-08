@echo off
setlocal
set PATH=%ProgramFiles(x86)%\msbuild\14.0\bin;%PATH%
set OUTPATH=%~dp0bin\release\
msbuild -nologo projectgen.sln -t:clean -p:configuration=release -v:m
msbuild -nologo projectgen.sln -t:clean -p:configuration=debug -v:m
msbuild -nologo projectgen.sln -t:rebuild -p:configuration=release -v:m
if exist %OUTPATH%projectgen.pdb copy /y %OUTPATH%projectgen.pdb \usr\local\bin 
if exist %OUTPATH%projectgen.xml copy /y %OUTPATH%projectgen.xml \usr\local\bin 
copy /y %OUTPATH%projectgen.exe \usr\local\bin 
echo.
echo hit ENTER
pause