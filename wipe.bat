@if "%1"=="" exit /b 1
@for /f %%i in ('dir /s/b %*') do if exist %%i rd /s/q %%i