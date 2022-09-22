if exist Bench\bin rd /s/q Bench\bin
if exist BitmapToRaster\bin rd /s/q BitmapToRaster\bin
if exist Common\bin rd /s/q Common\bin
if exist Engine6\bin rd /s/q Engine6\bin
if exist FixEol\bin rd /s/q FixEol\bin
if exist Gl\bin rd /s/q Gl\bin
if exist Perf\bin rd /s/q Perf\bin
if exist ShaderGen\bin rd /s/q ShaderGen\bin
if exist Shaders\bin rd /s/q Shaders\bin
if exist TestResults\bin rd /s/q TestResults\bin
if exist Tests\bin rd /s/q Tests\bin
if exist Win32\bin rd /s/q Win32\bin
dotnet build ShaderGen -m:1 -v:m && dotnet build Engine6 -m:1 -v:m