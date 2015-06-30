# BaiduPCS_NET
百度网盘下载和上传工具。BaiduPCS 项目的 .net 4.0 封装。 See https://github.com/GangZhuo/BaiduPCS .

此项目还在开发中，可能存在 BUG 和不稳定，请过段时间再来。

功能：
* 图形界面，
* 多线程上传和下载，
* 断点续传(上传和下载都支持)，
* 上传目录，
* 下载目录。

注意：
* 不支持回收站，
* 以点开头的文件，上传后会自动去掉文件名的点号，
* 上传时，已存在的文件名将会自动加上日期，
* 下载时，本地文件已经存在时，将会自动覆盖文件（无任何提示）。
* 上传大文件后，Meta Information 窗口中显示的 md5 值，可能不匹配（这是百度网盘 API 本身的问题）。此时重新上传将可解决这个问题（重新上传将会触发妙传，速度会很快；主要耗时在计算文件 md5 值）。

## 编译 (Windows)：
    1. git clone https://github.com/GangZhuo/BaiduPCS.git
    2. git clone https://github.com/GangZhuo/BaiduPCS_NET.git
    3. 按照 BaiduPCS 中 [编译-windows] 的说明，使 BaiduPCS 能够编译通过
    4. 打开 BaiduPCS_NET 目录下的 BaiduPCS_NET.sln。
	   然后，先编译 BaiduPCS_DLL 项目, 然后编译其他项目。

## 使用
* 1. 下载：软件中鼠标选中需要下载的文件或目录（可多选），然后右击，在弹出的菜单中选择 "Download"，然后选择本地目录。
* 2. 上传：软件中打开上传的目标目录，在空白处右击，在弹出的菜单中选择 "Upload file" 或者 "Upload Directory"。
* 3. 无论是上传还是下载必须启动工作线程，可点击历史队列窗口中顶部的图标启动工作进程。

#### 下载 .net 4.0 预编译文件: [BaiduCloudDisk-v0.1-alpha.net4.rar] 。
![Main Window](https://raw.githubusercontent.com/GangZhuo/BaiduPCS_NET/master/Sample/Sample_0_FileExplorer/main-window.png)
![History Window](https://raw.githubusercontent.com/GangZhuo/BaiduPCS_NET/master/Sample/Sample_0_FileExplorer/history-window.png)

[BaiduPCS]: https://github.com/GangZhuo/BaiduPCS
[BaiduCloudDisk-v0.1-alpha.net4.rar]: https://github.com/GangZhuo/BaiduPCS_NET/releases
[编译-windows]:   https://github.com/GangZhuo/BaiduPCS/blob/master/README.md#编译-windows
