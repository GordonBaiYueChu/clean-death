@echo off
REM �Թ���ԱȨ������
%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c "^&chr(34)^&"%~0"^&chr(34)^&" ::","%cd%","runas",1)(window.close)&&exit
cd /d "%~dp0"

if "%1" == "h" goto begin 
����  mshta vbscript:createobject("wscript.shell").run("%~nx0 h",0)(window.close)&&exit 
:begin 
REM ����UAC
REM C:\Windows\System32\cmd.exe /k %windir%\System32\reg.exe ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System /v EnableLUA /t REG_DWORD /d 1 /f

REM �������ڲ��ָĳ���Ҫ���ҵķ�������
sc query state= all |find /i "SupervisionService" >nul 2>nul
REM ���������ڣ���ת��exist��ǩ
if not errorlevel 1 (goto exist) else goto notexist

:exist
REM ����д�������ʱ�õĴ���
echo exist SupervisionService server
REM ��ͣ��ж�ط���
Net stop SupervisionService
sc delete SupervisionService
sc create SupervisionService binPath= "%~dp0\TuShan.BountyHunterDream.Service.exe"
sc failure SupervisionService reset= 3600 actions= restart/1000
sc config SupervisionService start= auto
Net Start SupervisionService
goto :end

:notexist
REM ����д���񲻴���ʱ�õĴ���
echo not exist SupervisionService server
sc create SupervisionService binPath= "%~dp0\TuShan.BountyHunterDream.Service.exe"
sc failure SupervisionService reset= 3600 actions= restart/1000
sc config SupervisionService start= auto
Net Start SupervisionService
goto :end

:end