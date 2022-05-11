#!/bin/bash
######################################
### MaxQuant HPC Submission Script ###
######################################
# !! This script is for use with the non-HPC variant of MaxQuant !!
# This script will run each individual stage of MaxQuant as an independent job. 
# The benefit of this is that it will automatically run parallel stages with
# the number of threads specified in your MqPar, and sequential stages with one 
# thread only. This will make running MaxQuant more efficient on a HPC
# platform.

#######################
## SCHEDULER HEADERS ##
#######################
# SLURM headers - generic for all stages
# Replace these if you do not use SLURM
#SBATCH --job-name      MaxQuant
#SBATCH --chdir         /path/to/folder/with/this/script

#SBATCH --output        maxquant-%A.out
#SBATCH --error         maxquant-%A.err

#SBATCH --ntasks=1
#SBATCH --mail-type=ALL
#SBATCH --mail-user=user@example.com


####################
### LOAD MODULES ###
####################
module load dotnet/3.1.413
module load maxquant/2.0.3

#######################
### MAXQUANT CONFIG ###
#######################
MQPAR="mqpar.xml"
MAXQUANT_PATH="$MAXQUANT_BIN/MaxQuantCmd.exe"
RUNTIME="dotnet"

########################
### SCHEDULER CONFIG ###
########################
SCRIPT="SubmitMaxQuant.sh"

# Scheduler specific
# SLURM #
SCHEDULER="sbatch"
CPU_ARG="--cpus-per-task="

# LSF # Untested
#SCHEDULER="bsub"
#CPU_ARG="-R span[hosts=1] -n "

###################################
### Do not edit below this line ###
###################################
MAX_THREADS=`sed -nr 's,<numThreads>([0-9]+)</numThreads>,\1,p' $MQPAR  | xargs`
MAX_THREADS="${MAX_THREADS/$'\r'/}"

IFS=","
SINGLE_THREADED=("Configuring${IFS}Testing fasta files${IFS}Combining apl files for first search${IFS}Combining apl files for main search${IFS}Combining second peptide files")
unset IFS

function SubmitJob() {
$SCHEDULER $CPU_ARG$THREADS $SCRIPT $STAGE_ID
}

function RunMaxQuant() {
$RUNTIME $MAXQUANT_PATH $MQPAR -p $THIS_STAGE_ID -e $THIS_STAGE_ID
}

THIS_STAGE_ID=1
NEXT_STAGE_ID=2
if [ -n "$1" ]; then
        THIS_STAGE_ID=$1
        NEXT_STAGE_ID=$((THIS_STAGE_ID+1))
fi

echo "Running MaxQuant stage $THIS_STAGE_ID ..."

RunMaxQuant

echo "MaxQuant stage $THIS_STAGE_ID complete"

LINES=`dotnet $MAXQUANT_BIN/MaxQuantCmd.exe $MQPAR -n`

while IFS= read -r LINE; do

        LINE_ARRAY=($LINE)
        STAGE_ID=${LINE_ARRAY[0]}
        STAGE_NAME=${LINE_ARRAY[@]:1:9}

        if [[ "STAGE_ID" -eq "$THIS_STAGE_ID" ]]; then
                if [ "$STAGE_NAME" == "Finish writing tables" ]; then
                        exit 0
                fi
        fi

        if [[ "$STAGE_ID" -eq "$NEXT_STAGE_ID" ]]; then
                break
        fi
done <<< "$LINES"
unset IFS

echo "Stage $STAGE_ID: $STAGE_NAME"

THREADS=$MAX_THREADS
IFS=","
if [[ "${IFS}${SINGLE_THREADED[*]}${IFS}" =~ "${IFS}${STAGE_NAME}${IFS}" ]]; then
    THREADS=1
fi
unset IFS

echo "Threads required: $THREADS"

SubmitJob