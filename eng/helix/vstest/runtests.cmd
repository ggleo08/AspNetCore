set target=%1
set sdkVersion=%2
set runtimeVersion=%3

set DOTNET_HOME=%HELIX_CORRELATION_PAYLOAD%\sdk
set DOTNET_ROOT=%DOTNET_HOME%\x64
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_MULTILEVEL_LOOKUP=0
set DOTNET_CLI_HOME=%HELIX_CORRELATION_PAYLOAD%\home

set PATH=%DOTNET_ROOT%;%PATH%

powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -useb 'https://dot.net/v1/dotnet-install.ps1'))) -Architecture x64 -Version %sdkVersion% -InstallDir %DOTNET_ROOT%"
powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -useb 'https://dot.net/v1/dotnet-install.ps1'))) -Architecture x64 -Runtime dotnet -Version %runtimeVersion% -InstallDir %DOTNET_ROOT%"

set HELIX=true

%DOTNET_ROOT%\dotnet vstest %target% --logger:trx;LogFileName=test-results.xml --logger:console;verbosity=normal

