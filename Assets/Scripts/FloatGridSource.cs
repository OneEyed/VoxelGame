using System;
using UnityEngine;

namespace Night
{
	public class FloatGridSource : VolumeSource {
	
		/// The raw volume data.
		private float[,,] data;
		/// To have a little bit faster data access.
		private int depthTimesHeight;
		/// The maximum absolute density value to be written into the data when combining,
		/// influencing the compression rate on serialization.
		private float maxClampedAbsoluteDensity = 5;
		private bool trilinearValue = true;
		// private bool trilinearGradient = false;
		private bool sobelGradient = false;
		/// The texture width.
		private int width;
		/// The texture height.
		private int height;
		
		private int depth;
		
		/// The scale of the position based on the world width(x)/height(y)/depth(z).
		private Vector3 scale;
		/// The texture depth.
		
		
		public FloatGridSource(Vector3 pos, Vector3 scale, int width, int height, int depth, float maxClampedAbsoluteDensity)
		{
		    this.maxClampedAbsoluteDensity=maxClampedAbsoluteDensity;
		    this.scale=scale;
		    data = new float[width,height,depth];
		    
		    this.width=width;
		    this.height=height;
		    this.depth=depth;
		    
			SimplexNoise.Initialize();
			
			int xx,yy,zz;
	        for (int x = 0; x < width; x++) {
	            //float[] row = grid[x];
	            for (int z = 0; z < depth; z++) {
	                //int blockHeight = (int)newGrid[x][z];
	                
	               // for (int y = 0; y < blockHeight-2; y++) {
	                for (int y = 0; y < height; y++) {
	                    xx = (int)pos.x + x;
						yy = (int)pos.y + y;
						zz = (int)pos.z + z;
	                    data[x,y,z] = SimplexNoise.octave_noise_3d(3,0.05f,0.025f,xx,yy,zz);//locX(float)blockHeight - (float)y));
	                    
	                }
	                
	            }
	        }
			/*
		    for(int x=0;x<width;x++)
		       for(int y=0;y<height;y++)
		           for(int z=0;z<depth;z++)
		               data[x,y,z] = -1;
		    */
		}
		
		public void setVolumeGridValue(int x, int y, int z, float value) {
		    // Clamp if wanted.
		    if (maxClampedAbsoluteDensity != 0) {
		        if (value > maxClampedAbsoluteDensity) {
		            value = maxClampedAbsoluteDensity;
		        } else if (value < -maxClampedAbsoluteDensity) {
		            value = (float) (-maxClampedAbsoluteDensity);
		        }
		    }
		
		
		
		    data[x,y,z] = value;
		}
		
		public float getVolumeGridValue(int x, int y, int z) {
		    if (x >= width) {
		        x = width - 1;
		    } else if (x < 0) {
		        x = 0;
		    }
		
		    if (y >= height) {
		        y = height - 1;
		    } else if (y < 0) {
		        y = 0;
		    }
		
		    if (z >= depth) {
		        z = depth - 1;
		    } else if (z < 0) {
		        z = 0;
		    }
		
		    return data[x,y,z];
		}
		
	    /** Gets a gradient of a point with optional sobel blurring.
	    @param x
	        The x coordinate of the point.
	    @param y
	        The x coordinate of the point.
	    @param z
	        The x coordinate of the point.
	    */
	    public override Vector3 getGradient(float fx, float fy, float fz)
	    {
	        int x = (int)(fx* scale.x + 0.5f);
	        int y = (int)(fy* scale.y + 0.5f);
	        int z = (int)(fz* scale.z + 0.5f);
	        
	        if (sobelGradient)
	        {
	            // Calculate gradient like in the original MC paper but mix a bit of Sobel in
	            if(x+1 >= width || x-1 < 0 || y+1 >= height || y-1 < 0 || z+1 >= depth || z-1 < 0)
	            {
	                Vector3 rfNormal = new Vector3(
	                (getVolumeGridValue(x + 1, y - 1, z) - getVolumeGridValue(x - 1, y - 1, z))
	                        + 2.0f * (getVolumeGridValue(x + 1, y, z) - getVolumeGridValue(x - 1, y, z))
	                        + (getVolumeGridValue(x + 1, y + 1, z) - getVolumeGridValue(x - 1, y + 1, z)),
	                (getVolumeGridValue(x, y + 1, z - 1) - getVolumeGridValue(x, y - 1, z - 1))
	                    + 2.0f * (getVolumeGridValue(x, y + 1, z) - getVolumeGridValue(x, y - 1, z))
	                    + (getVolumeGridValue(x, y + 1, z + 1) - getVolumeGridValue(x, y - 1, z + 1)),
	                (getVolumeGridValue(x - 1, y, z + 1) - getVolumeGridValue(x - 1, y, z - 1))
	                    + 2.0f * (getVolumeGridValue(x, y, z + 1) - getVolumeGridValue(x, y, z - 1))
	                    + (getVolumeGridValue(x + 1, y, z + 1) - getVolumeGridValue(x + 1, y, z - 1)));
	                rfNormal = rfNormal * (-1f);
	               // rfNormal.normalizeLocal();
	                return rfNormal;
	            }else{
	                 Vector3 rfNormal = new Vector3(
	                (data[x + 1, y - 1, z] - data[x - 1, y - 1, z])
	                        + 2.0f * (data[x + 1, y, z] - data[x - 1, y, z])
	                        + (data[x + 1, y + 1, z] - data[x - 1, y + 1, z]),
	                (data[x, y + 1, z - 1] - data[x, y - 1, z - 1])
	                    + 2.0f * (data[x, y + 1, z] - data[x, y - 1, z])
	                    + (data[x, y + 1, z + 1] - data[x, y - 1, z + 1]),
	                (data[x - 1, y, z + 1] - data[x - 1, y, z - 1])
	                    + 2.0f * (data[x, y, z + 1] - data[x, y, z - 1])
	                    + (data[x + 1, y, z + 1] - data[x + 1, y, z - 1]));
	                rfNormal *= (-1f);
	               // rfNormal.normalizeLocal();
	                return rfNormal;   
	            }
	        }
	        // Calculate gradient like in the original MC paper
	        
	        if(x+1 >= width || x-1 < 0 || y+1 >= height || y-1 < 0 || z+1 >= depth || z-1 < 0)
	        {
	            Vector3 rfNormal = new Vector3(
	            getVolumeGridValue(x - 1, y, z) - getVolumeGridValue(x + 1, y, z),
	            getVolumeGridValue(x, y - 1, z) - getVolumeGridValue(x, y + 1, z),
	            getVolumeGridValue(x, y, z - 1) - getVolumeGridValue(x, y, z + 1));
	           // rfNormal.normalizeLocal();
	            return rfNormal;
	        }else{
	            Vector3 rfNormal = new Vector3(
	                    data[x-1,y,z]-data[x+1,y,z],
	                    data[x,y-1,z]-data[x,y+1,z],
	                    data[x,y,z-1]-data[x,y,z+1]);
	           // rfNormal.normalizeLocal();
	            return rfNormal;   
	        }
	    }
		    
		
		private static int    BIG_ENOUGH_INT   = 16 * 1024;
		private static double BIG_ENOUGH_FLOOR = BIG_ENOUGH_INT;
		
		private static int fastCeil(float x) {
		   return BIG_ENOUGH_INT - (int)(BIG_ENOUGH_FLOOR-x); // credit: roquen
		}
		 
		public override float getValue(float posX, float posY, float posZ)
		{
		    float scaledPosX = posX * scale.x;
		    float scaledPosY = posY * scale.y; 
		    float scaledPosZ = posZ * scale.z;
		    
		    float value;
		    if (trilinearValue)
		    {
		        int x0 = (int)scaledPosX;
		        int x1 = (int)fastCeil(scaledPosX);
		        int y0 = (int)scaledPosY;
		        int y1 = (int)fastCeil(scaledPosY);
		        int z0 = (int)scaledPosZ;
		        int z1 = (int)fastCeil(scaledPosZ);
		
		        float dX = scaledPosX - x0;
		        float dY = scaledPosY - y0;
		        float dZ = scaledPosZ - z0;
		        
		
		        float f000,f100,f010,f001,f101,f011,f110,f111;
		        
		        if(x1 >= width || x0 < 0 || y1 >= height || y0 < 0 || z1 >= depth || z0 < 0)
		        {
		            f000 = getVolumeGridValue(x0, y0, z0);
		            f100 = getVolumeGridValue(x1, y0, z0);
		            f010 = getVolumeGridValue(x0, y1, z0);
		            f001 = getVolumeGridValue(x0, y0, z1);
		            f101 = getVolumeGridValue(x1, y0, z1);
		            f011 = getVolumeGridValue(x0, y1, z1);
		            f110 = getVolumeGridValue(x1, y1, z0);
		            f111 = getVolumeGridValue(x1, y1, z1);
		        }else{
		            f000 = data[x0,y0,z0];
		            f100 = data[x1,y0,z0];
		            f010 = data[x0,y1,z0];
		            f001 = data[x0,y0,z1];
		            f101 = data[x1,y0,z1];
		            f011 = data[x0,y1,z1];
		            f110 = data[x1,y1,z0];
		            f111 = data[x1,y1,z1];  
		        }
		
		        float oneMinX = 1.0f - dX;
		        float oneMinY = 1.0f - dY;
		        float oneMinZ = 1.0f - dZ;
		        float oneMinXoneMinY = oneMinX * oneMinY;
		        float dXOneMinY = dX * oneMinY;
		
		        value = oneMinZ * (f000 * oneMinXoneMinY
		            + f100 * dXOneMinY
		            + f010 * oneMinX * dY)
		            + dZ * (f001 * oneMinXoneMinY
		            + f101 * dXOneMinY
		            + f011 * oneMinX * dY)
		            + dX * dY * (f110 * oneMinZ
		            + f111 * dZ);
		    
		    }
		    else
		    {
		        // Nearest neighbour else
		        int x = (int)(scaledPosX + 0.5);
		        int y = (int)(scaledPosY + 0.5);
		        int z = (int)(scaledPosZ + 0.5);
		        value = getVolumeGridValue(x, y, z);
		    }
		    return value;
		}
		
		public override float getValue(Vector3 pos) {
		    return getValue(pos.x,pos.y,pos.z);
		}
		
		public override Vector3 getGradient(Vector3 pos) {
		    return getGradient(pos.x,pos.y,pos.z);
		}
		
		/**
		 * @return the width
		 */
		public int getWidth() {
		    return width;
		}
		
		/**
		 * @return the height
		 */
		public int getHeight() {
		    return height;
		}
		
		/**
		 * @return the depth
		 */
		public int getDepth() {
		    return depth;
		}
		
		
		public void addSphere(Vector3 center, float radius, bool add)
		{
		    float worldWidthScale = 1.0f / scale.x;
		    float worldHeightScale = 1.0f / scale.y;
		    float  worldDepthScale = 1.0f / scale.z;
		    
		    
		    float radiusSqrt = (float)Math.Sqrt(radius);
		
		
		    // No need for trilineaer interpolation here as we iterate over the
		    // cells anyway.
		    bool oldTrilinearValue = trilinearValue;
		    trilinearValue = false;
		    float value;
		   // int x, y;
		    Vector3 scaledCenter = new Vector3 (center.x * scale.x, center.y * scale.y, center.z * scale.z);
		    int xStart = clamp((int)(scaledCenter.x - radius * scale.x), 0, width);
		    int xEnd = clamp((int)(scaledCenter.x + radius * scale.x), 0, width);
		    int yStart = clamp((int)(scaledCenter.y - radius * scale.y), 0, height);
		    int yEnd = clamp((int)(scaledCenter.y + radius * scale.y), 0, height);
		    int zStart = clamp((int)(scaledCenter.z - radius * scale.z), 0, depth);
		    int zEnd = clamp((int)(scaledCenter.z + radius * scale.z), 0, depth);
		    Vector3 pos = new Vector3();
		    for (int z = zStart; z < zEnd; ++z)
		    {
		        for (int y = yStart; y < yEnd; ++y)
		        {
		            for (int x = xStart; x < xEnd; ++x)
		            {
		                pos.x = x * worldWidthScale;
		                pos.y =  y * worldHeightScale;
		                pos.z = z * worldDepthScale;
		                
		                /*float a = (center.x-pos.x);
		                float b = (center.y-pos.y);
		                float c = (center.z-pos.z);*/
		                
		                
		                if(add)
		                {
		                    float otherValue= radiusSqrt - Vector3.Distance(pos,scaledCenter);//;a*a+b*b+c*c;
		                    value = Math.Max(getValue(pos), otherValue);
		                }else{
		                   float otherValue= Vector3.Distance(pos,scaledCenter)-radiusSqrt;//;a*a+b*b+c*c;
		                   value = Math.Min(getValue(pos), otherValue); 
		                }
		                
		                // float otherValue = radius - pos.distance(scaledCenter)-1f;
		                
		                
		                setVolumeGridValue(x, y, z, value);
		            }
		        }
		    }
		
		    trilinearValue = oldTrilinearValue;
		}
		
		private static int clamp(int  value, int  min, int max) {
			return Math.Max(min, Math.Min(max, value));
		}
	}

}

