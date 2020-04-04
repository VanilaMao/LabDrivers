xcopy ..\Drivers\Prior LabDrivers\build /s /e /y
xcopy ..\Drivers\Prime LabDrivers\build /s /e /y
xcopy ..\bin\x64\Release\LabDrivers.dll LabDrivers\lib\net45 /y
Nuget pack LabDrivers\LabDrivers.nuspec
nuget sources Add -Name "GitHub" -Source "https://nuget.pkg.github.com/VanilaMao/index.json" -UserName VanilaMao -Password ed47406c703a8f342b728b3204d037b76f540689

nuget push "LabDrivers.1.0.7.nupkg" -Source "GitHub"