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
	class Time0 : FuncTime
	{
		override public void pack(Object i, csBuffer opb){}
		override public Object unpack(Info vi , csBuffer opb){return "";}
		override public Object look(DspState vd, InfoMode mi, Object i){return "";}
		override public void free_info(Object i){}
		override public void free_look(Object i){}
		override public int forward(Block vb, Object i){return 0;}
		override public int inverse(Block vb, Object i, float[] fin, float[] fout){return 0;}
	}
}
