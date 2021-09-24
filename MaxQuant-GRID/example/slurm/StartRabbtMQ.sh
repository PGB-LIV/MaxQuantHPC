#!/bin/bash -l

#SBATCH -J MQHPC-RabbitMQ
#SBATCH -o slurm/rabbit-%j.%N.out
#SBATCH -e slurm/rabbit-%j.%N.err

#SBATCH --ntasks=1
#SBATCH --cpus-per-task=1

module load singularity
module load dotnet
module load maxquant-hpc


MQHPC_StartRabbitMQ

# Ths loop keeps RabbitMQ alive until the file stop_rabbit appears
# Creating stop_rabbit will cause this job to end
while [ ! -f stop_rabbit ]
do
	sleep 10
done

echo "Shutting down"

rm stop_rabbit