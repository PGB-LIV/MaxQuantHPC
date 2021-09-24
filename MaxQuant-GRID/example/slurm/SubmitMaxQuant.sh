#!/bin/bash -l

#SBATCH -J MaxQuantRun
#SBATCH -o %j.%N.out
#SBATCH -e %j.%N.err

#SBATCH --ntasks=1
#SBATCH --cpus-per-task=1

module load dotnet

export MaxQuantBinDir=/path/to/MaxQuant/bin
export MQHPC_CONFIG_PATH=/path/to/settings.conf

dotnet $MaxQuantBinDir/MaxQuantTaskCore.dll --config

dotnet $MaxQuantBinDir/MaxQuantCmd.exe mqpar.xml

dotnet $MaxQuantBinDir/MaxQuantTaskCore.dll --errorLog