#!/bin/bash -l

#SBATCH -J MQHPC-MaxQuant
#SBATCH -o slurm/maxquant-%j.%N.out
#SBATCH -e slurm/maxquant-%j.%N.err

#SBATCH --ntasks=1
#SBATCH --cpus-per-task=1

module load dotnet
module load maxquant-hpc

export MQHPC_CONFIG_PATH=/path/to/settings.conf

dotnet $MAXQUANT_BIN/MaxQuantTaskCore.dll --config

dotnet $MAXQUANT_BIN/MaxQuantCmd.exe mqpar.xml

dotnet $MAXQUANT_BIN/MaxQuantTaskCore.dll --errorLog