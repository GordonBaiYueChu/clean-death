@echo off
REM �Թ���ԱȨ������
%1 mshta vbscript:CreateObject("Shell.Application").ShellExecute("cmd.exe","/c "^&chr(34)^&"%~0"^&chr(34)^&" ::","%cd%","runas",1)(window.close)&&exit
cd /d "%~dp0"

sc query state= all |find /i "CleanDeathService" >nul 2>nul
REM ���������ڣ���ת��exist��ǩ
if not errorlevel 1 (goto exist) else goto notexist

:exist
REM ����д�������ʱ�õĴ���
echo exist CleanDeathService server
REM ��ͣ��ж�ط���
Net stop CleanDeathService
sc delete CleanDeathService
goto :end

:notexist
REM ����д���񲻴���ʱ�õĴ���
echo not exist CleanDeathService server
goto :end

:end