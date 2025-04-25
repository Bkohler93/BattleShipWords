set -e

dotnet build

godot --path . --srcUdp=$1 --destUdp=$2 --tcp=$3