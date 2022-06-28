REM  This batch file will retrieve the currently connected RDP session
REM  and connect to it using the 'tscon' command, sending its output to
REM  a console window.  By default, once a session is disconnected the
REM  screen locks, and RDP processes will not be able to interact with
REM  on-screen elements. In practice, you would run this batch file from
REM  a desktop shortcut running as an administrator.

REM  Each session has a unique ID, which is needed as a parameter for tscon.exe.
REM  This command retrieves the current session ID attached to the CSURobotic
REM  account and stores it in the %%s variable.
for /f "skip=1 tokens=3" %%s in ('query user %CSURobotic%') do (
  tscon.exe %%s /dest:console
)
REM  As a security feature, the smart card will automatically de-authenticate
REM  as soon as a session is disconnected or control is passed over. This
REM  command is the scheduled job script which will log back into the
REM  smart card immediately.
powershell -file "WmsHub.ReferralsService.SmartCard.ps1"
