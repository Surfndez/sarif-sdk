:: RunTests.cmd
::
:: This script runs the tests in each test project. This is done in a separate script, 
:: rather than inline in BuildAndTest.cmd, because AppVeyor cannot run BuildAndTest.
:: AppVeyor runs the tests by invoking a separate script, and this is it.

@ECHO off
SETLOCAL ENABLEDELAYEDEXPANSION

set ReporterOption=
set ThisFileDirectory=%~dp0

:NextArg
if "%1" == "" goto :EndArgs
if "%1" == "/appveyor" (
    set ReporterOption=-appveyor&& shift && goto :NextArg
)
if "%1" == "/config" (
    if not "%2" == "Debug" if not "%2" == "Release" echo error: /config must be either Debug or Release && goto :ExitFailed
    set Configuration=%2&& shift && shift && goto :NextArg
)
echo Unrecognized option "%1" && goto :ExitFailed

:EndArgs

call SetBuildEnvVars.cmd

set Frameworks=netcoreapp2.0 net461
set TestRunnerRootPath=%ThisFileDirectory%src\packages\xunit.runner.console\2.3.1\tools\

for %%p in (%CrossPlatformTestProjects%) do (
    for %%f in (%Frameworks%) do (
        echo Running tests for %%p: %%f
        pushd %ThisFileDirectory%bld\bin\%%p\AnyCPU_%Configuration%\%%f
        if "%%f" EQU "netcoreapp2.0" (
            rem Hack: Without this line, the netcoreapp2.0 tests fail, on AppVeyor only,
            rem with the message:
            rem System.InvalidOperationException: Could not find/load any of the following assemblies: xunit.execution.dotnet.dll
            copy %ThisFileDirectory%src\packages\xunit.extensibility.execution\2.3.1\lib\netstandard1.1\xunit.execution.dotnet.dll

            dotnet %TestRunnerRootPath%netcoreapp2.0\xunit.console.dll %%p.dll %ReporterOption%
        ) ELSE (
            %TestRunnerRootPath%net452\xunit.console.exe %%p.dll %ReporterOption%
        )
        if "%ERRORLEVEL%" NEQ "0" (
            popd
            echo %%i: tests failed.
            goto ExitFailed
        )
        popd
    )
)

goto Exit

:ExitFailed
@echo Tests did not complete successfully.
exit /B 1

:Exit