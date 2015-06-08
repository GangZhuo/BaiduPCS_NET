# BaiduPCS_NET
The C# wrap for BaiduPCS. See https://github.com/GangZhuo/BaiduPCS .

# 编译 (Windows)：
    1. git clone https://github.com/GangZhuo/BaiduPCS.git
    2. git clone https://github.com/GangZhuo/BaiduPCS_NET.git
    3. 按照 BaiduPCS 中 [编译-windows] 的说明，使 BaiduPCS 能够编译通过
    4. 打开 BaiduPCS_NET 目录下的 BaiduPCS_NET.sln。然后，先编译 BaiduPCS_DLL 项目, 然后编译其他项目。

# Sample_1

# Sample_2_FileBackup
    1. 备份目录方法
	    a. 命令行执行 "FileBackup.exe --login" 登录网盘。
		b. 登录成功后，编辑用户目录 C:\Users\USER_NAME\AppData\Roaming\.pcs\ 下的 config.txt 文件，加入需要备份的本地目录和网盘目录。
		   一行一个，可加入多个。
		c. 命令行执行 "FileBackup.exe backup" 进行备份。
		
	2. 还原目录的方法
	    a. 同“备份目录方法”中的 (a)
		b. 同“备份目录方法”中的 (b)
		c. 命令行执行 "FileBackup.exe restore" 进行还原。

[编译-windows]:   https://github.com/GangZhuo/BaiduPCS/blob/master/README.md#编译-windows
