@ECHO OFF

cls
echo "What format do you want created"
echo ""

set OPTION=0
echo "*0-Apple Developer Ad-hoc version"
echo "1-Apple AppStore version"
echo "2-Axel's Apple"
echo "3-Kanwar's Apple 12 mini"
echo "4-Richard's Apple SE"
echo "5-Ngoga's' Apple 11"
echo "6-Umugwaneza's' Apple 11"
echo "7-Neema's Apple 14 Max Pro"
echo "8-Amelia's Apple 15 Pro"
set /P OPTION=Choose option :

if %OPTION%==0 (GOTO :adhoc)
if %OPTION%==1 (GOTO :appstore)
if %OPTION%==2 (GOTO :axel)
if %OPTION%==3 (GOTO :kanwar)
if %OPTION%==4 (GOTO :richard)
if %OPTION%==5 (GOTO :Ngoga)
if %OPTION%==6 (GOTO :Umugwaneza)
if %OPTION%==7 (GOTO :Neema)
if %OPTION%==8 (GOTO :Amelia)
) else (goto :adhoc)
pause

:adhoc
echo "My adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Mac_Provisioning" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "My adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for my ad-hoc is completed, now you can test it"
GOTO :end

:appstore
echo "My apstore"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Maui_Provisioning" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "My apstore"
C:\Users\Public\Python\text2speech2.exe "The IOS for apstore is completed, now you can test it"
GOTO :end

:axel
echo "Axel's adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Axel_Provisioning" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "Axel's adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for Axel's ad-hoc is completed, now you can test it"
GOTO :end

:kanwar
echo "Kanwar's adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Kanwar_Provisioning" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "Kanwar's adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for Kanwar's ad-hoc is completed, now you can test it"
GOTO :end

:richard
echo "Richard's adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Richard_Provisioning" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "Richard's adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for Richard's ad-hoc is completed, now you can test it"
GOTO :end

:Ngoga
echo "Ngoga's adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Ngoga Adhoc" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "Ngoga's adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for Ngoga adhoc is completed, now you can test it"
GOTO :end

:Umugwaneza
echo "Umugwaneza's adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Umugwaneza Adhoc" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "Umugwaneza's adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for Umugwaneza's ad-hoc is completed, now you can test it"
GOTO :end

:Neema
echo "Neema's adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Neema Nina Adhoc" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "Neema's adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for Neema's ad-hoc is completed, now you can test it"
GOTO :end

:Amelia
echo "Amelia's adhoc"
dotnet publish -f net8.0-ios -c Release -p:ArchiveOnBuild=true -p:RuntimeIdentifier=ios-arm64 -p:CodesignKey="Apple Distribution: Richard Perreault (K2Q72686FS)" -p:CodesignProvision="Amelia Adhoc" -p:ServerAddress=10.0.0.23 -p:ServerUser=richard -p:ServerPassword=677677jmr -p:TcpPort=58181 -p:_DotNetRootRemoteDirectory=/Users/richard/Library/Caches/Xamarin/XMA/SDKs/dotnet/
echo "Amelia's adhoc"
C:\Users\Public\Python\text2speech2.exe "The IOS for Amelia's ad-hoc is completed, now you can test it"
GOTO :end

:end
pause