xcopy ..\LabSave\bin\x64\Release\LabSave.dll LabSave\lib\net45 /y
Nuget pack LabSave\LabSave.nuspec
nuget sources Add -Name "GitHub" -Source "https://nuget.pkg.github.com/VanilaMao/index.json" -UserName VanilaMao -Password ed47406c703a8f342b728b3204d037b76f540689

nuget push "LabSave.1.0.4.nupkg" -Source "GitHub"
