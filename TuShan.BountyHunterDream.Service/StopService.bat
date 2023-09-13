@echo off
REM 以管理员权限运行
%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c "^&chr(34)^&"%~0"^&chr(34)^&" ::","%cd%","runas",1)(window.close)&&exit
cd /d "%~dp0"

sc query state= all |find /i "SupervisionService" >nul 2>nul
REM 如果服务存在，跳转至exist标签
if not errorlevel 1 (goto exist) else goto notexist

:exist
REM 这里写服务存在时用的代码
echo exist SupervisionService server
REM 暂停并卸载服务
Net stop SupervisionService
sc delete SupervisionService
goto :end

:notexist
REM 这里写服务不存在时用的代码
echo not exist SupervisionService server
goto :end

:end