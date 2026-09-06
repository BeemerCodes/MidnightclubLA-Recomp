@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
set PATH=%PATH%;C:\Program Files\LLVM\bin
cmake --preset win-amd64-release -DREXSDK_DIR=thirdparty\rexglue-sdk
cmake --build out\build\win-amd64-release
