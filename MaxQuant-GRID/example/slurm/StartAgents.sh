#!/bin/bash -l

#SBATCH -J MQG-Agents
#SBATCH -o %j.%N.out
#SBATCH -e %j.%N.err

# We recomend you set this higher than <numThreads> in mqpar.xml. E.g. numTheads+10
#SBATCH --array=1-200

#SBATCH --ntasks=1
#SBATCH --cpus-per-task=1

module load dotnet

export MaxQuantBinDir=/path/to/MaxQuant/bin
export MQHPC_CONFIG_PATH=/path/to/settings.conf

dotnet $MaxQuantBinDir/MaxQuantTaskCore.dll --config

dotnet $MaxQuantBinDir/MaxQuantTaskCore.dll --agent