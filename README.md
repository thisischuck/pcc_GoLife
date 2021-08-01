# pcc_GoLife
Game of Life Implementation

Single threaded
Multi threaded using\
  -> user generated Threads\
  -> c# Tasks\
  -> Compute Shaders


## Abstract
Game of Life implemented in Unity3d. It contains 2 types of examples exploring threads and compute shaders.\
For a school report about performance.

## Project Organization
Using a git client any user is able to interact with the different versions of this implementation.\
The following versions are:
***main***
1. CPU implementation composed of
   - Game Of Life (Script) located on Main Camera object
     - Grid Size
     - Thread Selection
       - Single Thread (0)
       - Threads (1)
       - Tasks (2)
     - Thread Count

***only_gpu***
1. GPU implementation of
    - Game Of Life (Script) located on Main Camera object
      - 16K resolution
      - Mutation level switchable

***gpu-more-planes***
1. GPU implementation of
    - Game Of Life (Script) located on Main Camera object
      - 64K resolution**
      - Mutation level switchable

** Pseudo 64K resolution, using four 16K resolution planes.




