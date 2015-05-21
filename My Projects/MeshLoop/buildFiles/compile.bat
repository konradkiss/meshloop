@echo off

REM Handle our optional parameters
SET COMPILER=%1
SET CONFIG=%2

IF NOT DEFINED COMPILER SET COMPILER=VS2008
IF NOT DEFINED CONFIG   SET CONFIG=Release

REM Setting up some variables

REM First the defaults (set up for VS2008 by default)
SET ENVVAR="C:\Program Files\Microsoft Visual Studio 9.0\VC\vcvarsall.bat"
SET BUILDCMD=vcbuild
SET OPTIONS= /useenv "%CONFIG%|Win32"
SET BUILDDIR="VisualStudio 2008"

REM Handle the non-defaults
IF %COMPILER% == VS2003 SET ENVVAR="C:\Program Files\Microsoft Visual Studio .NET 2003\Common7\Tools\vsvars32.bat"
IF %COMPILER% == VS2005 SET ENVVAR="C:\Program Files\Microsoft Visual Studio 8\VC\vcvarsall.bat"

IF %COMPILER% == VS2003 SET BUILDCMD=devenv
IF %COMPILER% == VS2003 SET OPTIONS=/build %CONFIG%

IF EXIST "C:\Program Files\Xoreax\IncrediBuild\BuildConsole.exe" SET BUILDCMD="C:\Program Files\Xoreax\IncrediBuild\BuildConsole.exe"
IF EXIST "C:\Program Files\Xoreax\IncrediBuild\BuildConsole.exe" SET OPTIONS=/build "%CONFIG%|Win32"

IF %COMPILER% == VS2003 SET BUILDDIR="VisualStudio 2003"
IF %COMPILER% == VS2005 SET BUILDDIR="VisualStudio 2005"


echo Building all solutions under %COMPILER% with the %CONFIG% configuration

echo Initializing %COMPILER% environment variables...
call %ENVVAR%

echo Initializing the DirectX SDK environment variables...

IF NOT DEFINED %DXSDK_DIR% goto error_no_DXSDK_DIR
call "%DXSDK_DIR%Utilities\Bin\dx_setenv.cmd"

echo Moving to our build directory
cd %BUILDDIR%

echo      - Building
for %%a in (*.sln) do %BUILDCMD% "%%a" %OPTIONS% & IF ERRORLEVEL 1 goto error_compile

REM It is just polite for a batch file to leave you in the same dir you started in
cd ..

REM We were successful in everything so go to the end
goto :end

:error_no_DXSDK_DIR
@echo ERROR: DXSDK_DIR variable is not set. Make sure the DirectX SDK is installed properly.
@goto end_error

:error_compile
@echo ERROR: There was an error compiling a solution in %CD%
@goto end_error

:end_error
EXIT 1

:end