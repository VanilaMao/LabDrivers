nuget sources Add -Name "GitHub" \
     -Source "https://nuget.pkg.github.com/VanilaMao/index.json" \
     -UserName VanilaMao -Password ed47406c703a8f342b728b3204d037b76f540689

nuget push "LabDrivers.nupkg" -Source "GitHub"

<repository type="git" url="https://github.com/my-org/my-custom-repo"/>


 https://github.com/VanilaMao/LabDrivers/packages (view package)