xcopy ..\Packages\EMGU.CV.4.1.1.3497\build\*.dll LabImage\build /s /e /y
xcopy ..\Packages\EMGU.CV.4.1.1.3497\lib\net35\Emgu.CV.World.dll LabImage\build /s /e /y
xcopy ..\LabImage\bin\x64\Release\LabImage.dll LabImage\lib\net45 /y
Nuget pack LabImage\LabImage.nuspec
nuget sources Add -Name "GitHub" -Source "https://nuget.pkg.github.com/VanilaMao/index.json" -UserName VanilaMao -Password ed47406c703a8f342b728b3204d037b76f540689

nuget push "LabImage.1.0.4.nupkg" -Source "GitHub"
