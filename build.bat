call clean.bat
del /q shaders\*.cs
dotnet build fixeol
fixeol\bin\fixeol.exe
dotnet build ShaderGen -m:1 -v:m && dotnet build Shaders -m:1 -v:m && dotnet build Engine6 -m:1 -v:m