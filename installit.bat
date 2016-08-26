@echo off
setlocal
set DEST=c:\usr\local\bin
set DIR=ProjectGen
set OUTDIR=%DIR%\bin\release
REM set PATH=%ProgramFiles(x86)%\msbuild\14.0\bin;%PATH%
msbuild -nologo -p:configuration=release -t:rebuild ProjectGen\projectgen.sln
copy /y  %OUTDIR%\projectgen.exe %DEST%
if exist %OUTDIR%\projectgen.xml copy /y %OUTDIR%\projectgen.xml %DEST%
copy /y  %OUTDIR%\projectgen.exe.config %DEST%
pause
