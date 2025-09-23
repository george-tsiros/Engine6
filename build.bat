@echo off
if not "%1"=="" set configuration=%1
if not "%2"=="" echo expected at most one argument && goto :eof
if "%configuration%"=="" set configuration=Debug
for %%p in (common win32 gl shadergen shaders engine6) do cd %%p & dotnet build --nologo --no-dependencies -v:q -c:%configuration% & cd ..
