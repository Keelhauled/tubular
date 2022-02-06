#!/bin/sh

SCRIPT_DIR=$(dirname "$(readlink -f "$0")")

dotnet publish -r linux-x64 -c Release -o $SCRIPT_DIR/publish/linux-x64/ $SCRIPT_DIR/src/tubular/tubular.csproj

mkdir $HOME/.local/bin
ln -s $SCRIPT_DIR/publish/linux-x64/tubular $HOME/.local/bin/
