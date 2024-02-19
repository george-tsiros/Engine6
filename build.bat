@echo off
for %%p in (common win32 gl shadergen shaders engine6) do (
cd %%p
dotnet build --nologo --no-dependencies --no-restore
cd ..
)
