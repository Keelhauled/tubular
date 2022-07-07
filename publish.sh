#!/bin/sh

SCRIPT_DIR=$(dirname "$(readlink -f "$0")")
cd $SCRIPT_DIR || exit

dotnet publish -r linux-x64 -c Release -o publish/linux-x64/ src/tubular/tubular.csproj

mkdir $HOME/.local/bin
ln -s $SCRIPT_DIR/publish/linux-x64/tubular $HOME/.local/bin/
