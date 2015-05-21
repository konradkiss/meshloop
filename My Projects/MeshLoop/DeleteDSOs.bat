for /R %%a IN (*.cs) do IF EXIST "%%a.dso" del "%%a.dso"
for /R %%a IN (*.cs) do IF EXIST "%%a.edso" del "%%a.edso"
