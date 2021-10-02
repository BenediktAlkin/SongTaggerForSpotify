import argparse
import sys
import os
import xml.etree.ElementTree as ET
import xml.dom.minidom

parser = argparse.ArgumentParser(description='Replace version in .csproj with the created git tag')
parser.add_argument('--tagname', help='tag name', required=True)
parser.add_argument('--project-file', help='project file with the project number that needs to be updated', required=True)
args = parser.parse_args()

tagname = args.tagname.replace("v", "")

if not os.path.exists(args.project_file):
    sys.exit(f"Invalid project file {args.project_file}")

root = ET.parse(args.project_file).getroot()

for node in root.iter():
    print(node.attrib)

tag_was_set = False
for node in root.iter():
    # remove whitespace for pretty printing
    node.tail = ""
    if node.text is not None:
        node.text = node.text.strip()
    # set version
    if node.tag == "Version":
        node.text = tagname
        tag_was_set = True
# add version node
if not tag_was_set:
    propertyGroup = root.find("./PropertyGroup") # XPath expression (Project is root node)
    versionNode = ET.Element("Version")
    versionNode.text = tagname
    propertyGroup.append(versionNode)

with open(args.project_file, "wb") as file:
    ugly_xml = ET.tostring(root).decode("utf-8")

    dom = xml.dom.minidom.parseString(ugly_xml)
    pretty_xml = dom.toprettyxml()

    file.write(pretty_xml.encode("utf-8"))

