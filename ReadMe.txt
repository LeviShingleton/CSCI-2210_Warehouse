PROGRAM OUTLINE

Warehouse is a static class that acts as a central repository for the simulation and its necessary parts.
	Provides global access to output filepaths
	Provides time increment ticks to simulation classes (Dock -> Truck, Crate)
	Provides random arrival of Trucks that are "sorted" into Docks.

Crates are held in stacks by Trucks. Trucks are held in queues in Docks.

Docks process the first Truck by asking the truck to pop its Crate stack once per pulse. It packages this pulse information in a DockRunStats struct.
Docks provide console output of their 
Docks also package their simulation statistics into a DockInfo struct, which is sent to the Warehouse at the end of the simulation.

At the end of the simulation, the Warehouse will generate a Warehouse(ID).txt file that contains figures necessary to draw conclusions about the performance of the N-dock setups.