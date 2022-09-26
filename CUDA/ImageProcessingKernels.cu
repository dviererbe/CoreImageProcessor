//Kernel code:
extern "C"  
{   
	#include <math.h>

    // Device code
    __global__ void ConvertRgb24ToGray8(char* source, char* result, double redPortion, double greenPortion, double bluePortion, int N)
    {
        int indexResult = blockDim.x * blockIdx.x + threadIdx.x;
		
		if (indexResult < N)
		{
			int indexSource = indexResult * 3;
			
			double value = redPortion * source[indexSource] + greenPortion * source[indexSource + 1] + bluePortion * source[indexSource + 2];
		
			if (value >= 0xFF)
				result[indexResult] = 0xFF;
			else if (value <= 0x00)
				result[indexResult] = 0x00;
			else
				result[indexResult] = (char)value;
		}
    }
	
	__global__ void ConvertGray8ToRgb24(char* source, char* result, int N)
    {
        int indexSource = blockDim.x * blockIdx.x + threadIdx.x;
		
		if (indexSource < N)
		{
			int indexResult = indexSource * 3;
		
			result[indexResult++] = source[indexSource];
			result[indexResult++] = source[indexSource];
			result[indexResult]   = source[indexSource];
		}
    }
	
	__global__ void Invert(char* source, char* result, int N)
    {
        int i = blockDim.x * blockIdx.x + threadIdx.x;
        
		if (i < N)
            result[i] = ~source[i];
    }
	
	__global__ void ApplyThreshold(char* source, char* result, int threshold, int N)
    {
        int i = blockDim.x * blockIdx.x + threadIdx.x;
        
		if (i < N)
		{
			if (source[i] < threshold)
			{
				result[i] = 0x00;
			}
			else
			{
				result[i] = 0xFF;
			}
		}
    }
	
	__global__ void AdjustBrightness(char* source, char* result, int value, int N)
    {
        int i = blockDim.x * blockIdx.x + threadIdx.x;
        
		if (i < N)
		{
			int newValue = source[i] + value;
		
			if (newValue < 0x00)
				result[i] = 0x00;
			else if (newValue > 0xFF)
				result[i] = 0xFF;
			else
				result[i] = (char)newValue;
		}
    }
	
	__global__ void AdjustContrast(char* source, char* result, int factor, int N)
    {
		int i = blockDim.x * blockIdx.x + threadIdx.x;
        
		if (i < N)
		{
			float newValue = factor * (source[i] - 128.0) + 128.0;

			if (newValue > 0xFF)
				result[i] = 0xFF;
			else if (newValue < 0x00)
				result[i] = 0x00;
			else
				result[i] = (char)round(newValue);
		}
    }
	
	__global__ void ApplyU8LookupTablePerChannel(char* source, char* result, char* lookupTable, int N)
    {
		__shared__ char lut[256];

		if (threadIdx.x < 256)
			lut[threadIdx.x] = lookupTable[threadIdx.x];
	
		__syncthreads();
	
        int i = blockDim.x * blockIdx.x + threadIdx.x;
        
		if (i < N)
		{
			result[i] = lut[source[i]];
		}
    }
}