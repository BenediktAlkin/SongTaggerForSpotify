import argparse
import sys
import os
import re

parser = argparse.ArgumentParser(description='Replace version in .vdproj with the created git tag')
parser.add_argument('--tagname', help='tag name', required=True)
parser.add_argument('--project-file', help='project file with the project number that needs to be updated', required=True)
args = parser.parse_args()

tagname = args.tagname.replace("v", "")

if not os.path.exists(args.project_file):
    sys.exit(f"Invalid project file {args.project_file}")

with open(args.project_file, "r") as file:
    lines = file.readlines()

tag_was_set = False
for i in range(len(lines)):
    old_line = lines[i]
    if "ProductVersion" in old_line:
        idxs = [match.start() for match in re.finditer('"', old_line)]
        newVersion = f'"8:{tagname}"'
        postfix = old_line[idxs[3]+1:] # store line ending
        newline = old_line[:idxs[2]] # remove old version from line
        newline += newVersion + postfix
        lines[i] = newline
        tag_was_set = True

if not tag_was_set:
    sys.exit("Could not find ProductVersion in project file")

with open(args.project_file, "w") as file:
    file.writelines(lines)

