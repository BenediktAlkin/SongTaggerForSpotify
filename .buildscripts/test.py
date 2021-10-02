import os

print("full path")
os.system("\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\devenv.exe\"")
print("full path shortcut")
os.system("\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\devenv\"")

print("shortcut")
os.system("devenv")