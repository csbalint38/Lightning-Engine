## Test device:
**CPU:** Intel i5-11400H
**GPU:** NVIDIA GeForce RTX 2050
**RAM:** 16GB
**OS:** Windows 11 Home 64bit

|               | Base implementation | Using bounding spheres  | Gain | Calculate Bounding Spheres in C++ side | Gain | Total Gain |
|---------------|---------------------|-------------------------|------|----------------------------------------|------|------------|
|**4k lights**  | 115 fps             | 116 fps                 | 1%   | x fps                                  | x %  | x %        |
|**64k lights** | 3 fps               | 21 fps                  | 700% | x fps                                  | x %  | x %        |
