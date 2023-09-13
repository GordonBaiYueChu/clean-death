@echo off
REM 以管理员权限运行
%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c "^&chr(34)^&"%~0"^&chr(34)^&" ::","%cd%","runas",1)(window.close)&&exit
cd /d "%~dp0"

if "%1" == "h" goto begin 
　　  mshta vbscript:createobject("wscript.shell").run("%~nx0 h",0)(window.close)&&exit 
:begin 
REM 禁用UAC
REM C:\Windows\System32\cmd.exe /k %windir%\System32\reg.exe ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System /v EnableLUA /t REG_DWORD /d 1 /f

REM 将引号内部分改成你要查找的服务名称
sc query state= all |find /i "SupervisionService" >nul 2>nul
REM 如果服务存在，跳转至exist标签
if not errorlevel 1 (goto exist) else goto notexist

:exist
REM 这里写服务存在时用的代码
echo exist SupervisionService server
REM 暂停并卸载服务
Net stop SupervisionService
sc delete SupervisionService
sc create SupervisionService binPath= "%~dp0\TuShan.BountyHunterDream.Service.exe"
sc failure SupervisionService reset= 3600 actions= restart/1000
sc config SupervisionService start= auto
Net Start SupervisionService
goto :end

:notexist
REM 这里写服务不存在时用的代码
echo not exist SupervisionService server
sc create SupervisionService binPath= "%~dp0\TuShan.BountyHunterDream.Service.exe"
sc failure SupervisionService reset= 3600 actions= restart/1000
sc config SupervisionService start= auto
Net Start SupervisionService
goto :end

:end