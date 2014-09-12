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
using System.Runtime.InteropServices;


namespace OggSharp 
{
	/*
	  function: LSP (also called LSF) conversion routines

	  The LSP generation code is taken (with minimal modification) from
	  "On the Computation of the LSP Frequencies" by Joseph Rothweiler
	  <rothwlr@altavista.net>, available at:
  
	  http://www2.xtdl.com/~rothwlr/lsfpaper/lsfpage.html 
	 ********************************************************************/

	class Lsp
	{

		[StructLayout(LayoutKind.Explicit, Size=32, CharSet=CharSet.Auto)]
		struct FloatHack 
		{
			[FieldOffset(0)] public float fh_float;
			[FieldOffset(0)] public int fh_int;
		}
		static float M_PI=(float)(3.1415926539);

		internal static void lsp_to_curve(float[] curve,
			int[] map, int n, int ln,
			float[] lsp, int m,
			float amp, float ampoffset)
		{
			int i;
			float wdel=M_PI/ln;
			for(i=0;i<m;i++)lsp[i]=Lookup.coslook(lsp[i]);
			int m2=(m/2)*2;

			i=0;
			while(i<n)
			{
				FloatHack fh = new FloatHack();
				int k=map[i];
				float p=.7071067812f;
				float q=.7071067812f;
				float w=Lookup.coslook(wdel*k);
				//int ftmp=0;
				int c=(int)((uint)m >> 1);

				for(int j=0;j<m2;j+=2)
				{
					q*=lsp[j]-w;
					p*=lsp[j+1]-w;
				}

				if((m&1)!=0)
				{
					/* odd order filter; slightly assymetric */
					/* the last coefficient */
					q*=lsp[m-1]-w;
					q*=q;
					p*=p*(1.0f-w*w);
				}
				else
				{
					/* even order filter; still symmetric */
					q*=q*(1.0f+w);
					p*=p*(1.0f-w);
				}

				//  q=frexp(p+q,&qexp);
				q=p+q;
				fh.fh_float = q;
				int hx=fh.fh_int;
				int ix=0x7fffffff&hx;
				int qexp=0;

				if(ix>=0x7f800000||(ix==0))
				{
					// 0,inf,nan
				}
				else
				{
					if(ix<0x00800000)
					{            // subnormal
						q*=3.3554432000e+07F;        // 0x4c000000
						fh.fh_float = q;
						hx=fh.fh_int;
						ix=0x7fffffff&hx;
						qexp=-25;
					}
					qexp += (int)(((uint)ix >> 23)-126);
					hx=(int)((hx&0x807fffff)|0x3f000000);
					fh.fh_int = hx;
					q=fh.fh_float;
				}

				q=Lookup.fromdBlook(amp*
					Lookup.invsqlook(q)*
					Lookup.invsq2explook(qexp+m)-ampoffset);

				do{curve[i]*=q; i++;}
					//    do{curve[i++]=q;}
				while(i<n&&map[i]==k);

			}
		}
	}
}
