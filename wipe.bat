for /f %%i in ('dir /s/b bin obj') do if exist %%i rd /s/q %%i