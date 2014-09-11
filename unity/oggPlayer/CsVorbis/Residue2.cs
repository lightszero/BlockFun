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


namespace OggSharp 
{
	class Residue2 : Residue0
	{
		override public int forward(Block vb,Object vl, float[][] fin, int ch)
		{
			return 0;
		}

		override public int inverse(Block vb, Object vl, float[][] fin, int[] nonzero, int ch)
		{
			//System.err.println("Residue0.inverse");
			int i=0;
			for(i=0;i<ch;i++)if(nonzero[i]!=0)break;
			if(i==ch)return(0); /* no nonzero vectors */

			return(_2inverse(vb, vl, fin, ch));
		}
	}
}