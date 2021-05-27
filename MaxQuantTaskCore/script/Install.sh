#!/bin/bash

cd "$(dirname "$0")"

INSTALL_DIR=`pwd`/../bin
RESET=`tput sgr0`
GOOD=`tput setaf 2`
BAD=`tput setaf 1`
DOTNET_FOUND="N"
BIN_DIR="../bin"

printf "Evironment information\n"
printf "Command dotnet available: "
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=`dotnet --version`
    DOTNET_VERSION=${DOTNET_VERSION:0:3}
    
    if [ $DOTNET_VERSION != "2.1" ] && [ $DOTNET_VERSION != "2.2" ]; then
        printf "$BAD ERROR: Incompatible dotnet version detected. Expected 2.1 or 2.2, found $DOTNET_VERSION $RESET\n"
    else
        printf "$GOOD $DOTNET_VERSION FOUND $RESET\n"
    fi

else
    printf "$BAD ERROR: dotnet command not found $RESET\n"
fi

printf "Command singularity available: "
if command -v singularity &> /dev/null; then
    printf "$GOOD OK $RESET\n"
else
    printf "$BAD ERROR: singularity command not found $RESET\n"
fi

printf "Dotnet capable MaxQuant: "
if [ -f "$BIN_DIR/MaxQuantTaskCore.dll" ]; then
    printf "$GOOD OK $RESET\n"
    DOTNET_FOUND=Y
else
    printf "$BAD ERROR: MaxQuant not dotnet capable $RESET\n"
fi

if [ $DOTNET_FOUND == N ]; then
    printf "$BAD FATAL: Cannot continue. MaxQuant not found. Likely cause is MaxQuant-GRID has been extracted to the wrong location. It should be installed in the MaxQuant directory next to bin$RESET\n"
    printf "./MaxQuant\n"
    printf "./MaxQuant/bin\n"
    printf "$GOOD./MaxQuant/GRID$RESET\n"
    printf "./MaxQuant/MaxQuant.exe\n"
    exit 1
fi

printf "\n"
printf "Install [y/n]: "
read DO_INSTALL

if [ $DO_INSTALL != "y" ] && [ $DO_INSTALL != "Y" ]; then
    printf "$BAD Unable to proceed without user input $RESET\n"
    exit 1
fi

printf "Installing for dotnet: "

mv $BIN_DIR/MaxQuantTaskCore.dll $BIN_DIR/MaxQuantTaskCore.Original.dll
mv $BIN_DIR/MaxQuantTaskCore.runtimeconfig.json $BIN_DIR/MaxQuantTaskCore.Original.runtimeconfig.json

cp Newtonsoft.Json.dll $BIN_DIR
cp RabbitMQ.Client.dll $BIN_DIR
cp System.Threading.Channels.dll $BIN_DIR
cp MaxQuantTaskCore.dll $BIN_DIR
cp MaxQuantTaskCore.deps.json $BIN_DIR
cp MaxQuantTaskCore.runtimeconfig.json $BIN_DIR

sed -i -e '/runtimepath=/d' config.txt
sed -i -e '/taskpath=/d' config.txt

echo "runtimepath=dotnet" >> config.txt
echo "taskpath=MaxQuantTaskCore.Original.dll" >> config.txt
printf "$GOOD OK $RESET\n"

printf "To create the RabbitMQ instance, now excute the comamnd: \n"

printf "singularity build --remote rabbitmq/rabbitmq.sif rabbitmq/rabbitmq.def\n"

printf "You may be required to login for this to work.\n"
