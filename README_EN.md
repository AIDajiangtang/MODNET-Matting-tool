English | [简体中文](README.md)

# MODNET-Matting-tool
C# reasoning tool based on deep learning matting tool  
[[`Paper`](https://arxiv.org/pdf/2011.11961.pdf )]  


wechat：**人工智能大讲堂**    
<img width="180" src="https://user-images.githubusercontent.com/18625471/228743333-77abe467-2385-476d-86a2-e232c6482291.jpg">  
Reply [matting] in the background to get the installation package and model files  


## Supports multiple input file types</h2>  
File -> Image File Load a local image file  
File -> Screenshot  
File -> Clipboard  
File -> Video For efficiency, the current Fps is 2, that is, two frames per second  


 <img width="500" src="https://user-images.githubusercontent.com/18625471/258716872-9098bd15-165b-41b8-9fe4-77cc7d42a94c.png">  
 
 ## Support background replacement</h2>  
 Background->Background Colo： Replace the background with a solid color  
 <img width="500" src="https://user-images.githubusercontent.com/18625471/258718249-be1ccc3b-bc17-4b52-b77b-42761a75f5e6.png">  
 Background -> Background Image： Replace the background with an image  
 <img width="500" src="https://user-images.githubusercontent.com/18625471/258718270-b71ece79-cfc6-408c-a089-12e3eb807085.png">  

  ## Supports multiple save options</h2>  
  Save -> Save Overall ：Foreground+Background  
  <img width="300" src="https://user-images.githubusercontent.com/18625471/258719054-bf497476-c953-420f-a0a3-228545c7a60d.png">   
  
  Save -> Save Foreground： Save only foreground  
  <img width="300" src="https://user-images.githubusercontent.com/18625471/258719046-f863bb9e-7334-45cd-ab7c-2b33aa044810.png">   
  The saved images and videos are all under the path of the exe.  
  
  


 ## source code compilation</h2>  
1. Download the source code to the local
2.Visual Studio opens the .sln project solution<br />  
3. Install the Nuget package<br />  
   3.1 In Visual Studio, right-click on the project and select "Manage NuGet Packages".<br />  
   3.2 In the "NuGet Package Manager" window, select the "Browse" tab.<br />  
   3.3 Search for Microsoft.ML.OnnxRuntime, select version 1.15.1, and click Install<br />  
   3.4 Search for OpenCvSharp4, select version 4.8.0, and click Install<br />  
   3.5 Search for OpenCvSharp4.runtime.win, select version 4.8.0, and click Install<br />  
   3.6 Find ScreenCapture.dll under Debug or Release and add it to the reference<br />  
  5. Put MODNET.onnx in the exe path<br />  
  6. Run the program<br />  
