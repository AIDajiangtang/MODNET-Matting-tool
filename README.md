# MODNET-Matting-tool
基于深度学习的抠图工具C#推理工具  
[[`Paper`](https://arxiv.org/pdf/2011.11961.pdf )]  

## 支持多种文件类型</h2>  
 文件->图像文件 加载本地图像文件  
 文件->截图  
 文件->剪切板  
 文件->视频  为了效率目前Fps为2，即每秒两帧

 <img width="500" src="https://user-images.githubusercontent.com/18625471/258716872-9098bd15-165b-41b8-9fe4-77cc7d42a94c.png">  
 
 ## 支持背景替换</h2>  
 背景->背景颜色 背景替换为纯色  
 <img width="500" src="https://user-images.githubusercontent.com/18625471/258718249-be1ccc3b-bc17-4b52-b77b-42761a75f5e6.png">  
 背景->背景图像  将背景替换为图像  
 <img width="500" src="https://user-images.githubusercontent.com/18625471/258718270-b71ece79-cfc6-408c-a089-12e3eb807085.png">  

  ## 支持多种保存选择</h2>  
  保存->保存整体  前景+背景  
  <img width="300" src="https://user-images.githubusercontent.com/18625471/258719054-bf497476-c953-420f-a0a3-228545c7a60d.png">   
  
  保存->保存前景  只保存前景  
  <img width="300" src="https://user-images.githubusercontent.com/18625471/258719046-f863bb9e-7334-45cd-ab7c-2b33aa044810.png"> 


 ## 源码编译</h2>  
 1.下载源码到本地  
 2.Visual Studio打开.sln项目解决方案  
 3.安装Nuget包  
  3.1在Visual Studio中，鼠标右键单击项目并选择“管理NuGet程序包”。  
  3.2在“NuGet包管理器”窗口中，选择“浏览”选项卡。  
  3.3搜索Microsoft.ML.OnnxRuntime，选择1.15.1版本，点击安装  
  3.4搜索OpenCvSharp4,选择4.8.0版本，点击安装  
  3.5搜索OpenCvSharp4.runtime.win,选择4.8.0版本，点击安装  
  3.6找到Debug或者Release下的ScreenCapture.dll,添加到引用中  
 4.将MODNET.onnx放到exe路径下  
 5.运行程序
