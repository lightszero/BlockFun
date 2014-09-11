/* OggSharp
 * Copyright (C) 2000 ymnk, JCraft,Inc.
 *  
 * Written by: 2000 ymnk<ymnk@jcraft.com>
 * Ported to C# from JOrbis by: Mark Crichton <crichton@gimp.org> 
 *   
 * Thanks go to the JOrbis team, for licencing the code under the
 * LGPL, making my job a lot easier.
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public License
 * as published by the Free Software Foundation; either version 2 of
 * the License, or (at your option) any later version.
   
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Library General Public License for more details.
 * 
 * You should have received a copy of the GNU Library General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

using System;
using System.Runtime.CompilerServices;


namespace OggSharp 
{
	class Residue0 : FuncResidue
	{
		override public void pack(Object vr, csBuffer opb)
		{
			InfoResidue0 info=(InfoResidue0)vr;
			int acc=0;
			opb.write(info.begin,24);
			opb.write(info.end,24);

			opb.write(info.grouping-1,24);  /* residue vectors to group and 
				     code with a partitioned book */
			opb.write(info.partitions-1,6); /* possible partition choices */
			opb.write(info.groupbook,8);  /* group huffman book */

			/* secondstages is a bitmask; as encoding progresses pass by pass, a
			   bitmask of one indicates this partition class has bits to write
			   this pass */
			for(int j=0;j<info.partitions;j++)
			{
				if(ilog(info.secondstages[j])>3)
				{
					/* yes, this is a minor hack due to not thinking ahead */
					opb.write(info.secondstages[j],3); 
					opb.write(1,1);
					opb.write(info.secondstages[j] >> 3,5); 
				}
				else
				{
					opb.write(info.secondstages[j],4); /* trailing zero */
				}
				acc+=icount(info.secondstages[j]);
			}
			for(int j=0;j<acc;j++)
			{
				opb.write(info.booklist[j],8);
			}
		}

		override public Object unpack(Info vi, csBuffer opb)
		{
			int acc=0;
			InfoResidue0 info=new InfoResidue0();

			info.begin=opb.read(24);
			info.end=opb.read(24);
			info.grouping=opb.read(24)+1;
			info.partitions=opb.read(6)+1;
			info.groupbook=opb.read(8);

			for(int j=0;j<info.partitions;j++)
			{
				int cascade=opb.read(3);
				if(opb.read(1)!=0)
				{
					cascade|=(opb.read(5)<<3);
				}
				info.secondstages[j]=cascade;
				acc+=icount(cascade);
			}

			for(int j=0;j<acc;j++)
			{
				info.booklist[j]=opb.read(8);
				//    if(info.booklist[j]==255)info.booklist[j]=-1;
			}

			if(info.groupbook>=vi.books)
			{
				free_info(info);
				return(null);
			}

			for(int j=0;j<acc;j++)
			{
				if(info.booklist[j]>=vi.books)
				{
					free_info(info);
					return(null);
				}
			}
			return(info);
			//  errout:
			//    free_info(info);
			//    return(NULL);
		}

		override public Object look(DspState vd, InfoMode vm, Object vr)
		{
			InfoResidue0 info=(InfoResidue0)vr;
			LookResidue0 look=new LookResidue0();
			int acc=0;
			int dim;
			int maxstage=0;
			look.info=info;
			look.map=vm.mapping;

			look.parts=info.partitions;
			look.fullbooks=vd.fullbooks;
			look.phrasebook=vd.fullbooks[info.groupbook];

			dim=look.phrasebook.dim;

			look.partbooks=new int[look.parts][];

			for(int j=0;j<look.parts;j++)
			{
				int stages=ilog(info.secondstages[j]);
				if(stages!=0)
				{
					if(stages>maxstage)maxstage=stages;
					look.partbooks[j]=new int[stages];
					for(int k=0; k<stages; k++)
					{
						if((info.secondstages[j]&(1<<k))!=0)
						{
							look.partbooks[j][k]=info.booklist[acc++];
						}
					}
				}
			}

			look.partvals=(int)Math.Round(Math.Pow(look.parts,dim));
			look.stages=maxstage;
			look.decodemap=new int[look.partvals][];
			for(int j=0;j<look.partvals;j++)
			{
				int val=j;
				int mult=look.partvals/look.parts;
				look.decodemap[j]=new int[dim];

				for(int k=0;k<dim;k++)
				{
					int deco=val/mult;
					val-=deco*mult;
					mult/=look.parts;
					look.decodemap[j][k]=deco;
				}
			}
			return(look);
		}

		override public void free_info(Object i){}
		override public void free_look(Object i){}
		override public int forward(Block vb,Object vl, float[][] fin, int ch)
		{
			return 0;
		}

		static int[][][] partword=new int[2][][];
	
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal static int _01inverse(Block vb, Object vl, float[][] fin, int ch, int decodepart)
		{
			{
				int i,j,k,l,s;
				LookResidue0 look=(LookResidue0 )vl;
				InfoResidue0 info=look.info;

				// move all this setup out later
				int samples_per_partition=info.grouping;
				int partitions_per_word=look.phrasebook.dim;
				int n=info.end-info.begin;
  
				int partvals=n/samples_per_partition;
				int partwords=(partvals+partitions_per_word-1)/partitions_per_word;

				if(partword.Length<ch)
				{
					partword=new int[ch][][];
					for(j=0;j<ch;j++)
					{
						partword[j]=new int[partwords][];
					}
				}
				else
				{
					for(j=0;j<ch;j++)
					{
						if(partword[j]==null || partword[j].Length<partwords)
							partword[j]=new int[partwords][];
					}
				}

				for(s=0;s<look.stages;s++)
				{
					// each loop decodes on partition codeword containing 
					// partitions_pre_word partitions
					for(i=0,l=0;i<partvals;l++)
					{
						if(s==0)
						{
							// fetch the partition word for each channel
							for(j=0;j<ch;j++)
							{
								int temp=look.phrasebook.decode(vb.opb);
								if(temp==-1)
								{
									//goto eopbreak;
									return(0);
								}
								partword[j][l]=look.decodemap[temp];
								if(partword[j][l]==null)
								{
									//	      goto errout;
									return(0);
								}
							} 
						}
      
						// now we decode residual values for the partitions
						for(k=0;k<partitions_per_word && i<partvals;k++,i++)
							for(j=0;j<ch;j++)
							{
								int offset=info.begin+i*samples_per_partition;
								if((info.secondstages[partword[j][l][k]]&(1<<s))!=0)
								{
									CodeBook stagebook=look.fullbooks[look.partbooks[partword[j][l][k]][s]];
									//	      CodeBook stagebook=look.partbooks[partword[j][l][k]][s];
									if(stagebook!=null)
									{
										if(decodepart==0)
										{
											if(stagebook.decodevs_add(fin[j],offset,vb.opb,samples_per_partition)==-1)
											{
												// goto errout;
												return(0);
											}
										}
										else if(decodepart==1)
										{
											if(stagebook.decodev_add(fin[j], offset, vb.opb,samples_per_partition)==-1)
											{
												// goto errout;
												return(0);
											}
										}
									}
								}
							}
					} 
				}
				return(0);
			}
		}

		internal static int _2inverse(Block vb, Object vl, float[][] fin, int ch)
		{
			int i,k,l,s;
			LookResidue0 look=(LookResidue0 )vl;
			InfoResidue0 info=look.info;

			// move all this setup out later
			int samples_per_partition=info.grouping;
			int partitions_per_word=look.phrasebook.dim;
			int n=info.end-info.begin;
  
			int partvals=n/samples_per_partition;
			int partwords=(partvals+partitions_per_word-1)/partitions_per_word;

			int[][] partword=new int[partwords][];
			for(s=0;s<look.stages;s++)
			{
				for(i=0,l=0;i<partvals;l++)
				{
					if(s==0)
					{
						// fetch the partition word for each channel
						int temp=look.phrasebook.decode(vb.opb);
						if(temp==-1)
						{
							// goto eopbreak;
							return(0);
						}
						partword[l]=look.decodemap[temp];
						if(partword[l]==null)
						{
							// goto errout;
							return(0);
						}
					}

					// now we decode residual values for the partitions
					for(k=0;k<partitions_per_word && i<partvals;k++,i++)
					{
						int offset=info.begin+i*samples_per_partition;
						if((info.secondstages[partword[l][k]]&(1<<s))!=0)
						{
							CodeBook stagebook=look.fullbooks[look.partbooks[partword[l][k]][s]];
							if(stagebook!=null)
							{
								if(stagebook.decodevv_add(fin, offset, ch, vb.opb,samples_per_partition)==-1)
								{
									// goto errout;
									return(0);
								}
							}
						} 
					}
				}
			}
			//  errout:
			//  eopbreak:
			return(0);
		}

		override public int inverse(Block vb, Object vl, float[][] fin, int[] nonzero, int ch)
		{
			//System.err.println("Residue0.inverse");
			int used=0;
			for(int i=0;i<ch;i++)
			{
				if(nonzero[i]!=0)
				{
					fin[used++]=fin[i];
				}
			}
			if(used!=0)
				return(_01inverse(vb, vl, fin, used, 0));
			else
				return(0);
		}

		internal static int ilog(int v)
		{
			int ret=0;
			while(v!=0)
			{
				ret++;
				v = (int)((uint)v >> 1);
			}
			return(ret);
		}
		internal static int icount(int v)
		{
			int ret=0;
			while(v!=0)
			{
				ret+=(v&1);
				v = (int)((uint)v >> 1);
			}
			return(ret);
		}
	}

	class LookResidue0 
	{
		internal InfoResidue0 info;
		internal int map;
  
		internal int parts;
		internal int stages;
		internal CodeBook[] fullbooks;
		internal CodeBook phrasebook;
		internal int[][] partbooks;
		//  CodeBook[][] partbooks;

		internal int partvals;
		internal int[][] decodemap;

		//internal int postbits;
		//internal int phrasebits;
		//  int[][]     frames;
		//internal int frames;
	}

	class InfoResidue0
	{
		// block-partitioned VQ coded straight residue
		internal int begin;
		internal int end;

		// first stage (lossless partitioning)
		internal int grouping;                   // group n vectors per partition
		internal int partitions;                 // possible codebooks for a partition
		internal int groupbook;                  // huffbook for partitioning
		internal int[] secondstages=new int[64]; // expanded out to pointers in lookup
		internal int[] booklist=new int[256];    // list of second stage books

		// encode-only heuristic settings
		internal float[] entmax=new float[64];       // book entropy threshholds
		internal float[] ampmax=new float[64];       // book amp threshholds
		internal int[] subgrp=new int[64];       // book heuristic subgroup size
		internal int[] blimit=new int[64];       // subgroup position limits
	}
}