after the install (if an install package is used), you may need to add the program as a service

open a command promopt with elevated privilges and execute the following:
%systemroot%\Microsoft.NET\Framework\v4.0.30319 InstallUtil.exe "C:\Program Files (x86)\WebBasix\WBXEngine\WBXEngineService.exe"


%systemroot%\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u "C:\Program Files (x86)\WebBasix\WBXEngine\WBXEngineService.exe"


--- example in production
InstallUtil.exe "D:\Program Files\WEB BASIX\WBXEngineService\v6.x\ASWExpress\WBXEngineService.exe"