call clean.bat
dotnet build fixeol
fixeol\bin\fixeol.exe
dotnet build ShaderGen -m:1 -v:m && dotnet build Engine6 -m:1 -v:m