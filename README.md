# MaxQuantHPC

MaxQuantHPC is a plugin for MaxQuant that allows a MaxQuant analysis to be spread over a multi-node HPC cluster to allow for the processing of massive proteomic studies that may contain a very large number of samples or a large search space due to post-translation modifications. Compared to the default edition of MaxQuant, this version provides:

* Support for multi-node clusters
* Works with Job Schedulers (Slurm, Torque, etc.) - Optional
* When used with a job scheduler will utilise resources efficiently, if MaxQuant is in a single threaded phase then it will only request 1 CPU

Not only do you get faster searches due to the ability to spread the work across multiple nodes, feature detection and other heavy I/O phases will benefit from the ability to utilise multi-node and consequently multi-NICs. Bandwidth no longer limited by node, but by storage/switch.
