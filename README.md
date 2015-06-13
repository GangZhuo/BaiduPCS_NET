# BaiduPCS_NET
百度网盘下载和上传工具。BaiduPCS 项目的 .net 4.0 封装。 See https://github.com/GangZhuo/BaiduPCS .

功能：
* 图形界面，
* 多线程上传和下载，
* 断点续传(上传和下载都支持)，
* 上传目录，
* 下载目录。

## 编译 (Windows)：
    1. git clone https://github.com/GangZhuo/BaiduPCS.git
    2. git clone https://github.com/GangZhuo/BaiduPCS_NET.git
    3. 按照 BaiduPCS 中 [编译-windows] 的说明，使 BaiduPCS 能够编译通过
    4. 打开 BaiduPCS_NET 目录下的 BaiduPCS_NET.sln。
	   然后，先编译 BaiduPCS_DLL 项目, 然后编译其他项目。

#### 下载 .net 4.0 预编译文件: [BaiduCloudDisk for .Net 4.0] 。
![BaiduCloudDisk for .Net 4.0](https://raw.githubusercontent.com/GangZhuo/BaiduPCS_NET/master/Sample/Sample_0_FileExplorer/main-window.png)

[BaiduPCS]: https://github.com/GangZhuo/BaiduPCS
[BaiduCloudDisk for .Net 4.0]: https://sourceforge.net/projects/baidupcs/files/Windows/
[编译-windows]:   https://github.com/GangZhuo/BaiduPCS/blob/master/README.md#编译-windows
