#!/bin/bash -l

#SBATCH -J MQG-Agents
#SBATCH -o %j.%N.out
#SBATCH -e %j.%N.err

#SBATCH --ntasks=1
#SBATCH --cpus-per-task=1

module load singularity


export MaxQuantGridDir=/path/to/MaxQuant/GRID

$MaxQuantGridDir/StartRabbitMQ

# Ths loop keeps RabbitMQ alive until the file stop_rabbit appears
# Creating stop_rabbit will cause this job to end
while [! -f stop_rabbit ]
do
	sleep 10
done

echo "Shutting down"

rm stop_rabbit