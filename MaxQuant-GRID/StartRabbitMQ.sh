#!/bin/bash

cd "$(dirname "$0")"

HOSTNAME=`hostname -f`
RESET=`tput sgr0`
GOOD=`tput setaf 2`
BAD=`tput setaf 1`

mkdir -p var/lib/rabbitmq
mkdir -p var/log/rabbitmq

printf "Server hostname: "
printf "$GOOD $HOSTNAME $RESET\n"


printf "Command singularity available: "
if command -v singularity &> /dev/null; then
    printf "$GOOD OK $RESET\n"
else
    printf "$BAD ERROR: singularity command not found $RESET\n"

    exit 1
fi

printf "SIF Image: "
if [ -f "rabbitmq.sif" ]; then
    printf "$GOOD OK $RESET\n"
else
    printf "$BAD ERROR: SIF image not found.\n"
    printf "To create SIF image, execute:\n"
    printf "singularity build --remote rabbitmq.sif rabbitmq.def $RESET\n\n"

    exit 1
fi

printf "Config file: "
if [ -f "$MQG_CONFIG_PATH" ]; then
    printf "$GOOD OK $RESET\n"
else
    printf "$BAD ERROR: Config file not found.\n"
    printf "Specify config file by setting system environment variable:\n"
    printf "MQG_CONFIG_PATH=/path/to/my/config.txt\n\n"
    printf "This value can be set in your job scheduler script or on the CLI prior to starting this script\n\n"

    exit 1
fi

printf "Starting instance: "
singularity instance start --bind var/lib:/var/lib,var/lib/rabbitmq:/var/lib/rabbitmq,var/log:/var/log rabbitmq.sif rabbitmq
printf "$GOOD OK $RESET\n"

sed -i -e '/rmq_host=/d' $MQG_CONFIG_PATH
echo "rmq_host=$HOSTNAME" >> $MQG_CONFIG_PATH