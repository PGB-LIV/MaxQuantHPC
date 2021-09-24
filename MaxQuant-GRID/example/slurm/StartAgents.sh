#!/bin/bash -l

#SBATCH -J MQHPC-Agents
#SBATCH -o slurm/agent-%j.%N.out
#SBATCH -e slurm/agent-%j.%N.err

# We recomend you set this higher than <numThreads> in mqpar.xml. E.g. numTheads+10
#SBATCH --array=1-200

#SBATCH --ntasks=1
#SBATCH --cpus-per-task=1

module load dotnet
module load maxquant-hpc

export MQHPC_CONFIG_PATH=/path/to/settings.conf

dotnet $MAXQUANT_BIN/MaxQuantTaskCore.dll --config

dotnet $MAXQUANT_BIN/MaxQuantTaskCore.dll --agent