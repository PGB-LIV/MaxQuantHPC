#!/bin/bash

cd "$(dirname "$0")/../rabbitmq"

HOSTNAME=`hostname -f`
RESET=`tput sgr0`
GOOD=`tput setaf 2`
BAD=`tput setaf 1`

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
if [ -f "$MQHPC_CONFIG_PATH" ]; then
    printf "$GOOD OK $RESET\n"
else
    printf "$BAD ERROR: Config file not found.\n"
    printf "Specify config file by setting system environment variable:\n"
    printf "MQHPC_CONFIG_PATH=/path/to/my/settings.conf\n\n"
    printf "This value can be set in your job scheduler script or on the CLI prior to starting this script $RESET\n\n"

    exit 1
fi

printf "Data Dir: "
if [ -z "$MQHPC_RABBIT_DIR" ]; then
    printf "$BAD ERROR: Data directory not found.\n"
    printf "Specify data directory by setting system environment variable:\n"
    printf "MQHPC_RABBIT_DIR=/path/to/my/rabbit/data\n\n"
    printf "This value can be set in your job scheduler script or on the CLI prior to starting this script $RESET\n\n"

    exit 1
else
    printf "$GOOD OK $RESET\n"
fi

mkdir -p $MQHPC_RABBIT_DIR/var/lib/rabbitmq
mkdir -p $MQHPC_RABBIT_DIR/var/log/rabbitmq

printf "Starting instance: "
singularity instance start --bind $MQHPC_RABBIT_DIR/var/lib:/var/lib,$MQHPC_RABBIT_DIR/var/lib/rabbitmq:/var/lib/rabbitmq,$MQHPC_RABBIT_DIR/var/log:/var/log rabbitmq.sif rabbitmq
printf "$GOOD OK $RESET\n"

sed -i -e '/rmq_host=/d' $MQG_CONFIG_PATH
echo "rmq_host=$HOSTNAME" >> $MQG_CONFIG_PATH