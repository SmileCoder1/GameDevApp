Description of File: 

The Chunk class handles all terrain generation by procedurally and path-independently determining what biome the user is and using 
seeded random value-noise to create the terrain. In order to speed up the logic for mining and placing blocks, the class also implements
a 2-way look up table to check which blocks should be rendered.