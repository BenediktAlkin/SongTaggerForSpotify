import os
import argparse

parser = argparse.ArgumentParser(description='Builds VS setup project')
parser.add_argument('--sln', help='.sln file', required=True)
parser.add_argument('--vdproj', help='.vdproj file of the setup project', required=True)
parser.add_argument('--build-config', help='build config (Release, Debug)', required=True)
args = parser.parse_args()

os.system(f"\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\devenv.exe\" {args.sln} {args.vdproj} /Build ${{ env.build_config }}")