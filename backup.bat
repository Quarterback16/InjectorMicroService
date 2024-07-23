::  Backup solution
del c:\developer\backup\wiki\InjectorMicroService.zip
cd "c:\developer\projects\InjectorMicroService\"
c:\developer\utes\pkzip25 -add=update -rec -path=full -exclude=@c:\developer\work\exclude.lst -temp=c:\developer\temp c:\developer\backup\wiki\InjectorMicroService.zip
pause