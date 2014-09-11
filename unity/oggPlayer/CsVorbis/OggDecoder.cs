#region Imports

using System;
using System.Collections.Generic;
using System.IO;



#endregion Imports

namespace OggSharp
{
    public struct PCMChunk
    {
        public byte[] Bytes;
        public int Length;
        public int Channels;
        public int Rate;
    }

    public class OggDecoder : IEnumerable<PCMChunk>
    {
        #region Constants

        private const int pageSize = 4096;
        private const int CHUNKSIZE = 8500;
        private const int SEEK_SET = 0;
        private const int SEEK_CUR = 1;
        private const int SEEK_END = 2;
        private const int OV_FALSE = -1;
        private const int OV_EOF = -2;
        private const int OV_HOLE = -3;
        private const int OV_EREAD = -128;
        private const int OV_EFAULT = -129;
        private const int OV_EIMPL = -130;
        private const int OV_EINVAL = -131;
        private const int OV_ENOTVORBIS = -132;
        private const int OV_EBADHEADER = -133;
        private const int OV_EVERSION = -134;
        private const int OV_ENOTAUDIO = -135;
        private const int OV_EBADPACKET = -136;
        private const int OV_EBADLINK = -137;
        private const int OV_ENOSEEK = -138;

        #endregion Constants

        #region Private variables

        private Stream input;
        private bool initialized;
        private int convsize;
        private int eos;
        private bool seekable;

        private long offset;
        private int links;
        private long[] offsets;
        private long[] dataoffsets;
        private int[] serialnos;
        private long[] pcmlengths;
        private Info[] vis;
        private Comment[] vcs;
        private long pcm_offset;
        private bool decode_ready = false;
        private int current_serialno;
        private int current_link;
        private float bittrack;
        private float samptrack;

        private SyncState oy; // sync and verify incoming physical bitstream
        private StreamState os; // take physical pages, weld into a logical stream of packets
        private Page og = new Page(); // one Ogg bitstream page.  Vorbis packets are inside
        private Packet op; // one raw packet of data for decode
        private Info vi;  // struct that stores all the static vorbis bitstream settings
        private Comment vc; // struct that stores all the bitstream user comments
        private DspState vd; // central working state for the packet->PCM decoder
        private Block vb;
        private float[][][] _pcm = new float[1][][];
        private int[] _index;
        private byte[] convbuffer = new byte[8192];

        #endregion Private variables

        #region Private methods

        //  link:   -1) return the vorbis_info struct for the bitstream section
        //              currently being decoded
        //         0-n) to request information for a specific bitstream section
        //
        // In the case of a non-seekable bitstream, any call returns the
        // current bitstream.  NULL in the case that the machine is not
        // initialized

        private Info getInfo(int link)
        {
            if (link < 0)
            {
                if (decode_ready)
                {
                    return vis[current_link];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (link >= links)
                {
                    return null;
                }
                else
                {
                    return vis[link];
                }
            }
        }

        private Comment getComment(int link)
        {
            if (link < 0)
            {
                if (decode_ready) { return vcs[current_link]; }
                else { return null; }
            }
            else
            {
                if (link >= links) { return null; }
                else { return vcs[link]; }
            }
        }

        private int make_decode_ready()
        {
            vd.synthesis_init(vis[0]);
            vb.init(vd);
            decode_ready = true;
            return (0);
        }

        // fetch and process a packet.  Handles the case where we're at a
        // bitstream boundary and dumps the decoding machine.  If the decoding
        // machine is unloaded, it loads it.  It also keeps pcm_offset up to
        // date (seek and read both use this.  seek uses a special hack with
        // readp). 
        //
        // return: -1) hole in the data (lost packet) 
        //          0) need more date (only if readp==0)/eof
        //          1) got a packet 

        private int process_packet(int readp)
        {
            Page og = new Page();

            // handle one packet.  Try to fetch it from current stream state
            // extract packets from page
            while (true)
            {
                // process a packet if we can.  If the machine isn't loaded,
                // neither is a page
                if (decode_ready)
                {
                    Packet op = new Packet();
                    int result = os.packetout(op);
                    long granulepos;
                    // if(result==-1)return(-1); // hole in the data. For now, swallow
                    // and go. We'll need to add a real
                    // error code in a bit.
                    if (result > 0)
                    {
                        // got a packet.  process it
                        granulepos = op.granulepos;
                        if (vb.synthesis(op) == 0)
                        { // lazy check for lazy
                            // header handling.  The
                            // header packets aren't
                            // audio, so if/when we
                            // submit them,
                            // vorbis_synthesis will
                            // reject them
                            // suck in the synthesis data and track bitrate
                            {
                                int oldsamples = vd.synthesis_pcmout(null, null);
                                vd.synthesis_blockin(vb);
                                samptrack += vd.synthesis_pcmout(null, null) - oldsamples;
                                bittrack += op.bytes * 8;
                            }

                            // update the pcm offset.
                            if (granulepos != -1 && op.e_o_s == 0)
                            {
                                int link = current_link;
                                int samples;
                                // this packet has a pcm_offset on it (the last packet
                                // completed on a page carries the offset) After processing
                                // (above), we know the pcm position of the *last* sample
                                // ready to be returned. Find the offset of the *first*
                                // 
                                // As an aside, this trick is inaccurate if we begin
                                // reading anew right at the last page; the end-of-stream
                                // granulepos declares the last frame in the stream, and the
                                // last packet of the last page may be a partial frame.
                                // So, we need a previous granulepos from an in-sequence page
                                // to have a reference point.  Thus the !op.e_o_s clause above

                                samples = vd.synthesis_pcmout(null, null);
                                granulepos -= samples;
                                for (int i = 0; i < link; i++)
                                {
                                    granulepos += pcmlengths[i];
                                }
                                pcm_offset = granulepos;
                            }
                            return (1);
                        }
                    }
                }

                if (readp == 0) return (0);
                if (get_next_page(og, -1) < 0) return (0); // eof. leave unitialized

                // bitrate tracking; add the header's bytes here, the body bytes
                // are done by packet above
                bittrack += og.header_len * 8;

                // has our decoding just traversed a bitstream boundary?
                if (decode_ready)
                {
                    if (current_serialno != og.serialno())
                    {
                        decode_clear();
                    }
                }

                // Do we need to load a new machine before submitting the page?
                // This is different in the seekable and non-seekable cases.  
                // 
                // In the seekable case, we already have all the header
                // information loaded and cached; we just initialize the machine
                // with it and continue on our merry way.
                // 
                // In the non-seekable (streaming) case, we'll only be at a
                // boundary if we just left the previous logical bitstream and
                // we're now nominally at the header of the next bitstream

                if (!decode_ready)
                {
                    int i;
                    current_serialno = og.serialno();

                    // match the serialno to bitstream section.  We use this rather than
                    // offset positions to avoid problems near logical bitstream
                    // boundaries
                    for (i = 0; i < links; i++)
                    {
                        if (serialnos[i] == current_serialno) break;
                    }
                    if (i == links) return (-1); // sign of a bogus stream.  error out,
                    // leave machine uninitialized
                    current_link = i;

                    os.init(current_serialno);
                    os.reset();

                    make_decode_ready();
                }
                os.pagein(og);
            }
        }

        // clear out the current logical bitstream decoder
        private void decode_clear()
        {
            os.clear();
            vd.clear();
            vb.clear();
            decode_ready = false;
            bittrack = 0.0f;
            samptrack = 0.0f;
        }

        // returns: total raw (compressed) length of content if i==-1
        //          raw (compressed) length of that logical bitstream for i==0 to n
        //          -1 if the stream is not seekable (we can't know the length)

        private long raw_total(int i)
        {
            if (i >= links) return (-1);
            if (i < 0)
            {
                long acc = 0;               // bug?
                for (int j = 0; j < links; j++)
                {
                    acc += raw_total(j);
                }
                return (acc);
            }
            else
            {
                return (offsets[i + 1] - offsets[i]);
            }
        }

        // returns: total PCM length (samples) of content if i==-1
        //          PCM length (samples) of that logical bitstream for i==0 to n
        //          -1 if the stream is not seekable (we can't know the length)
        private long pcm_total(int i)
        {
            if (i >= links) return (-1);
            if (i < 0)
            {
                long acc = 0;
                for (int j = 0; j < links; j++)
                {
                    acc += pcm_total(j);
                }
                return (acc);
            }
            else
            {
                return (pcmlengths[i]);
            }
        }

        // returns: total seconds of content if i==-1
        //          seconds in that logical bitstream for i==0 to n
        //          -1 if the stream is not seekable (we can't know the length)
        private float time_total(int i)
        {
            if (i >= links) return (-1);
            if (i < 0)
            {
                float acc = 0;
                for (int j = 0; j < links; j++)
                {
                    acc += time_total(j);
                }
                return (acc);
            }
            else
            {
                return ((float)(pcmlengths[i]) / vis[i].rate);
            }
        }

        // seek to an offset relative to the *compressed* data. This also
        // immediately sucks in and decodes pages to update the PCM cursor. It
        // will cross a logical bitstream boundary, but only if it can't get
        // any packets out of the tail of the bitstream we seek to (so no
        // surprises). 
        // 
        // returns zero on success, nonzero on failure

        private int raw_seek(int pos)
        {
            if (!seekable && pos != 0)
            {
                throw new InvalidOperationException("Cannot seek, the stream is not seekable");
            }

            if (pos < 0 || pos > offsets[links])
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // clear out decoding machine state
            pcm_offset = -1;
            decode_clear();

            // seek
            seek_helper(pos);

            // we need to make sure the pcm_offset is set.  We use the
            // _fetch_packet helper to process one packet with readp set, then
            // call it until it returns '0' with readp not set (the last packet
            // from a page has the 'granulepos' field set, and that's how the
            // helper updates the offset

            switch (process_packet(1))
            {
                case 0:
                    // oh, eof. There are no packets remaining.  Set the pcm offset to
                    // the end of file
                    pcm_offset = pcm_total(-1);
                    return (0);
                case -1:
                    // error! missing data or invalid bitstream structure
                    //goto seek_error;
                    pcm_offset = -1;
                    decode_clear();
                    return -1;
                default:
                    // all OK
                    if (!seekable)
                    {
                        break;
                    }
                    break;
            }
            while (true)
            {
                switch (process_packet(0))
                {
                    case 0:
                        // the offset is set.  If it's a bogus bitstream with no offset
                        // information, it's not but that's not our fault.  We still run
                        // gracefully, we're just missing the offset
                        return (0);
                    case -1:
                        // error! missing data or invalid bitstream structure
                        //goto seek_error;
                        pcm_offset = -1;
                        decode_clear();
                        return -1;
                    default:
                        // continue processing packets
                        if (!seekable)
                        {
                            break;
                        }
                        break;
                }
            }
        }

        // seek to a sample offset relative to the decompressed pcm stream 
        // returns zero on success, nonzero on failure

        private int pcm_seek(long pos)
        {
            int link = -1;
            long total = pcm_total(-1);

            if (pos < 0 || pos > total)
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // which bitstream section does this pcm offset occur in?
            for (link = links - 1; link >= 0; link--)
            {
                total -= pcmlengths[link];
                if (pos >= total) break;
            }

            // search within the logical bitstream for the page with the highest
            // pcm_pos preceeding (or equal to) pos.  There is a danger here;
            // missing pages or incorrect frame number information in the
            // bitstream could make our task impossible.  Account for that (it
            // would be an error condition)
            {
                long target = pos - total;
                long end = offsets[link + 1];
                long begin = offsets[link];
                int best = (int)begin;

                Page og = new Page();
                while (begin < end)
                {
                    long bisect;
                    int ret;

                    if (end - begin < CHUNKSIZE)
                    {
                        bisect = begin;
                    }
                    else
                    {
                        bisect = (end + begin) / 2;
                    }

                    seek_helper(bisect);
                    ret = get_next_page(og, end - bisect);

                    if (ret == -1)
                    {
                        end = bisect;
                    }
                    else
                    {
                        long granulepos = og.granulepos();
                        if (granulepos < target)
                        {
                            best = ret;  // raw offset of packet with granulepos
                            begin = offset; // raw offset of next packet
                        }
                        else
                        {
                            end = bisect;
                        }
                    }
                }
                // found our page. seek to it (call raw_seek).
                if (raw_seek(best) != 0)
                {
                    //goto seek_error;
                    pcm_offset = -1;
                    decode_clear();
                    return -1;
                }
            }

            // verify result
            if (pcm_offset >= pos)
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }
            if (pos > pcm_total(-1))
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // discard samples until we reach the desired position. Crossing a
            // logical bitstream boundary with abandon is OK.
            while (pcm_offset < pos)
            {
                float[][] pcm;
                int target = (int)(pos - pcm_offset);
                float[][][] _pcm = new float[1][][];
                int samples = vd.synthesis_pcmout(_pcm, _index);
                pcm = _pcm[0];

                if (samples > target) samples = target;
                vd.synthesis_read(samples);
                pcm_offset += samples;

                if (samples < target)
                    if (process_packet(1) == 0)
                    {
                        pcm_offset = pcm_total(-1); // eof
                    }
            }
            return 0;

            // seek_error:
            // dump machine so we're in a known state
            //pcm_offset=-1;
            //decode_clear();
            //return -1;
        }

        // seek to a playback time relative to the decompressed pcm stream 
        // returns zero on success, nonzero on failure
        private int time_seek(float seconds)
        {
            // translate time to PCM position and call pcm_seek

            int link = -1;
            long pcm_tot = pcm_total(-1);
            float time_tot = time_total(-1);

            if (seconds < 0 || seconds > time_tot)
            {
                //goto seek_error;
                pcm_offset = -1;
                decode_clear();
                return -1;
            }

            // which bitstream section does this time offset occur in?
            for (link = links - 1; link >= 0; link--)
            {
                pcm_tot -= pcmlengths[link];
                time_tot -= time_total(link);
                if (seconds >= time_tot) break;
            }

            // enough information to convert time offset to pcm offset
            {
                long target = (long)(pcm_tot + (seconds - time_tot) * vis[link].rate);
                return (pcm_seek(target));
            }
        }

        // tell the current stream offset cursor.  Note that seek followed by
        // tell will likely not give the set offset due to caching
        private long raw_tell()
        {
            return (offset);
        }

        // return PCM offset (sample) of next PCM sample to be read
        private long pcm_tell()
        {
            return (pcm_offset);
        }

        // return time offset (seconds) of next PCM sample to be read
        private float time_tell()
        {
            // translate time to PCM position and call pcm_seek

            int link = -1;
            long pcm_tot = 0;
            float time_tot = 0.0f;

            pcm_tot = pcm_total(-1);
            time_tot = time_total(-1);

            // which bitstream section does this time offset occur in?
            for (link = links - 1; link >= 0; link--)
            {
                pcm_tot -= pcmlengths[link];
                time_tot -= time_total(link);
                if (pcm_offset >= pcm_tot) break;
            }

            return ((float)time_tot + (float)(pcm_offset - pcm_tot) / vis[link].rate);
        }

        //The helpers are over; it's all toplevel interface from here on out
        // clear out the OggVorbis_File struct
        private int clear()
        {
            vb.clear();
            vd.clear();
            os.clear();

            if (vis != null && links != 0)
            {
                for (int i = 0; i < links; i++)
                {
                    vis[i].clear();
                    vcs[i].clear();
                }
                vis = null;
                vcs = null;
            }
            if (dataoffsets != null) dataoffsets = null;
            if (pcmlengths != null) pcmlengths = null;
            if (serialnos != null) serialnos = null;
            if (offsets != null) offsets = null;
            oy.clear();
            //if(datasource!=null)(vf->callbacks.close_func)(vf->datasource);
            //memset(vf,0,sizeof(OggVorbis_File));
            return (0);
        }

        private int open_seekable()
        {
            Info initial_i = new Info();
            Comment initial_c = new Comment();
            int serialno;
            long end;
            int ret;
            int dataoffset;
            Page og = new Page();
            // is this even vorbis...?
            int[] foo = new int[1];
            ret = fetch_headers(initial_i, initial_c, foo, null);
            serialno = foo[0];
            dataoffset = (int)offset; //!!
            os.clear();
            if (ret == -1) return (-1);

            offset = input.Position = input.Length;
            end = offset;

            // We get the offset for the last page of the physical bitstream.
            // Most OggVorbis files will contain a single logical bitstream
            end = get_prev_page(og);
            // moer than one logical bitstream?
            if (og.serialno() != serialno)
            {
                // Chained bitstream. Bisect-search each logical bitstream
                // section.  Do so based on serial number only
                if (bisect_forward_serialno(0, 0, end + 1, serialno, 0) < 0)
                {
                    clear();
                    return OV_EREAD;
                }
            }
            else
            {
                // Only one logical bitstream
                if (bisect_forward_serialno(0, end, end + 1, serialno, 0) < 0)
                {
                    clear();
                    return OV_EREAD;
                }
            }
            prefetch_all_headers(initial_i, initial_c, dataoffset);
            return (raw_seek(0));
        }

        // uses the local ogg_stream storage in vf; this is important for
        // non-streaming input sources
        private int fetch_headers(Info vi, Comment vc, int[] serialno, Page og_ptr)
        {
            //System.err.println("fetch_headers");
            Page og = new Page();
            Packet op = new Packet();
            int ret;

            if (og_ptr == null)
            {
                ret = get_next_page(og, CHUNKSIZE);
                if (ret == OV_EREAD) return OV_EREAD;
                if (ret < 0) return OV_ENOTVORBIS;
                og_ptr = og;
            }

            if (serialno != null) serialno[0] = og_ptr.serialno();

            os.init(og_ptr.serialno());

            // extract the initial header from the first page and verify that the
            // Ogg bitstream is in fact Vorbis data

            vi.init();
            vc.init();

            int i = 0;
            while (i < 3)
            {
                os.pagein(og_ptr);
                while (i < 3)
                {
                    int result = os.packetout(op);
                    if (result == 0) break;
                    if (result == -1)
                    {
                        throw new Exception("Corrupt header in logical bitstream.");
                        //goto bail_header;
                        vi.clear();
                        vc.clear();
                        os.clear();
                        return -1;
                    }
                    if (vi.synthesis_headerin(vc, op) != 0)
                    {
                        throw new Exception("Illegal header in logical bitstream.");
                        //goto bail_header;
                        vi.clear();
                        vc.clear();
                        os.clear();
                        return -1;
                    }
                    i++;
                }
                if (i < 3)
                    if (get_next_page(og_ptr, 1) < 0)
                    {
                        throw new Exception("Missing header in logical bitstream.");
                        //goto bail_header;
                        vi.clear();
                        vc.clear();
                        os.clear();
                        return -1;
                    }
            }
            return 0;
        }

        // last step of the OggVorbis_File initialization; get all the
        // vorbis_info structs and PCM positions.  Only called by the seekable
        // initialization (local stream storage is hacked slightly; pay
        // attention to how that's done)
        private void prefetch_all_headers(Info first_i, Comment first_c, int dataoffset)
        {
            Page og = new Page();
            int ret;

            vis = new Info[links];
            vcs = new Comment[links];
            dataoffsets = new long[links];
            pcmlengths = new long[links];
            serialnos = new int[links];

            for (int i = 0; i < links; i++)
            {
                if (first_i != null && first_c != null && i == 0)
                {
                    // we already grabbed the initial header earlier.  This just
                    // saves the waste of grabbing it again
                    // !!!!!!!!!!!!!
                    vis[i] = first_i;
                    //memcpy(vf->vi+i,first_i,sizeof(vorbis_info));
                    vcs[i] = first_c;
                    //memcpy(vf->vc+i,first_c,sizeof(vorbis_comment));
                    dataoffsets[i] = dataoffset;
                }
                else
                {
                    // seek to the location of the initial header
                    seek_helper(offsets[i]); //!!!
                    if (fetch_headers(vis[i], vcs[i], null, null) == -1)
                    {
                        throw new Exception("Error opening logical bitstream #" + (i + 1) + "\n");
                        dataoffsets[i] = -1;
                    }
                    else
                    {
                        dataoffsets[i] = offset;
                        os.clear();
                    }
                }

                // get the serial number and PCM length of this link. To do this,
                // get the last page of the stream
                long end = offsets[i + 1]; //!!!
                seek_helper(end);

                while (true)
                {
                    ret = get_prev_page(og);
                    if (ret == -1)
                    {
                        // this should not be possible
                        throw new Exception("Could not find last page of logical " +
                            "bitstream #" + (i) + "\n");
                        vis[i].clear();
                        vcs[i].clear();
                        break;
                    }
                    if (og.granulepos() != -1)
                    {
                        serialnos[i] = og.serialno();
                        pcmlengths[i] = og.granulepos();
                        break;
                    }
                }
            }
        }

        private void seek_helper(long offset)
        {
            //callbacks.seek_func(datasource, offst, SEEK_SET);
            input.Position = offset;
            this.offset = offset;
            oy.reset();
        }

        private int get_data()
        {
            int index = oy.buffer(CHUNKSIZE);
            byte[] buffer = oy.data;
            //  int bytes=callbacks.read_func(buffer, index, 1, CHUNKSIZE, datasource);
            int bytes = 0;
            try
            {
                bytes = input.Read(buffer, index, CHUNKSIZE);
            }
            catch (Exception e)
            {
                //Console.Error.WriteLine(e.Message);
                return OV_EREAD;
            }
            oy.wrote(bytes);
            if (bytes == -1)
            {
                bytes = 0;
            }
            return bytes;
        }

        private int get_next_page(Page page, long boundary)
        {
            if (boundary > 0) boundary += offset;
            while (true)
            {
                int more;
                if (boundary > 0 && offset >= boundary) return OV_FALSE;
                more = oy.pageseek(page);
                if (more < 0) { offset -= more; }
                else
                {
                    if (more == 0)
                    {
                        if (boundary == 0) return OV_FALSE;
                        //	  if(get_data()<=0)return -1;
                        int ret = get_data();
                        if (ret == 0) return OV_EOF;
                        if (ret < 0) return OV_EREAD;
                    }
                    else
                    {
                        int ret = (int)offset; //!!!
                        offset += more;
                        return ret;
                    }
                }
            }
        }

        private int get_prev_page(Page page)
        {
            long begin = offset; //!!!
            int ret;
            int offst = -1;
            while (offst == -1)
            {
                begin -= CHUNKSIZE;
                if (begin < 0)
                    begin = 0;
                seek_helper(begin);
                while (offset < begin + CHUNKSIZE)
                {
                    ret = get_next_page(page, begin + CHUNKSIZE - offset);
                    if (ret == OV_EREAD) { return OV_EREAD; }
                    if (ret < 0) { break; }
                    else { offst = ret; }
                }
            }
            seek_helper(offst); //!!!
            ret = get_next_page(page, CHUNKSIZE);
            if (ret < 0)
            {
                //System.err.println("Missed page fencepost at end of logical bitstream Exiting");
                //System.exit(1);
                return OV_EFAULT;
            }
            return offst;
        }

        private int bisect_forward_serialno(long begin, long searched, long end, int currentno, int m)
        {
            long endsearched = end;
            long next = end;
            Page page = new Page();
            int ret;

            while (searched < endsearched)
            {
                long bisect;
                if (endsearched - searched < CHUNKSIZE)
                {
                    bisect = searched;
                }
                else
                {
                    bisect = (searched + endsearched) / 2;
                }

                seek_helper(bisect);
                ret = get_next_page(page, -1);
                if (ret == OV_EREAD) return OV_EREAD;
                if (ret < 0 || page.serialno() != currentno)
                {
                    endsearched = bisect;
                    if (ret >= 0) next = ret;
                }
                else
                {
                    searched = ret + page.header_len + page.body_len;
                }
            }
            seek_helper(next);
            ret = get_next_page(page, -1);
            if (ret == OV_EREAD) return OV_EREAD;

            if (searched >= end || ret == -1)
            {
                links = m + 1;
                offsets = new long[m + 2];
                offsets[m + 1] = searched;
            }
            else
            {
                ret = bisect_forward_serialno(next, offset, end, page.serialno(), m + 1);
                if (ret == OV_EREAD) return OV_EREAD;
            }
            offsets[m] = begin;
            return 0;
        }

        private int ReadNextPage()
        {
            int index = oy.buffer(pageSize);
            int bytes = 0;
            byte[] buffer = oy.data;
            try
            {
                bytes = input.Read(buffer, index, pageSize);
            }
            catch
            {
                // TODO: Handle error
                throw;
            }
            oy.wrote(bytes);
            if (bytes == 0) eos = 1;

            return bytes;
        }

        private void ReadHeader()
        {
            oy = new SyncState();
            oy.init();
            os = new StreamState();
            op = new Packet();
            vi = new Info();
            vc = new Comment();
            vd = new DspState();
            vb = new Block(vd); // local working space for packet->PCM decode

            input.Position = 0;
            open_seekable();
            vi = vis[0];
            vc = vcs[0];

            SampleRate = vi.rate;
            Stereo = (vi.channels > 1);
            Length = this.time_total(-1);
            _index = new int[vi.channels];
            convsize = 4096 / vi.channels;
            Reset();
        }

        private void ConvertChunk(int bout, int bytesRead, float[][] pcm)
        {
            bool clipflag = false;
            // convert floats to 16 bit signed ints (host order) and
            // interleave

#if UNSAFE

            unsafe
            {
                fixed (byte* ptrStart = convbuffer)
                fixed (float* pcmStart1 = pcm[0])
                fixed (float* pcmStart2 = pcm.Length == 2 ? pcm[1] : null)
                {

#endif

                    for (int i = 0; i < vi.channels; i++)
                    {

#if UNSAFE

                        byte* ptr = ptrStart + (bytesRead + (i * 2));
                        float* currentPcm = (i == 0 ? pcmStart1 : pcmStart2);

#else

                        float[] currentPcm = (i == 0 ? pcm[0] : pcm[1]);
                        int ptr = (bytesRead + (i * 2));

#endif

                        //int ptr=i;
                        int mono = _index[i];
                        for (int j = 0; j < bout; j++)
                        {

#if UNSAFE

                            int val = (int)(*(currentPcm + mono + j) * 32767.0f);

#else

                            int val = (int)(currentPcm[mono + j] * 32767.0);

#endif

                            //        short val=(short)(pcm[i][mono+j]*32767.);
                            //        int val=(int)Math.round(pcm[i][mono+j]*32767.);
                            // might as well guard against clipping
                            if (val > 32767)
                            {
                                val = 32767;
                                clipflag = true;
                            }
                            if (val < -32768)
                            {
                                val = -32768;
                                clipflag = true;
                            }
                            if (val < 0) val = val | 0x8000;

                            if (BitConverter.IsLittleEndian)
                            {

#if UNSAFE

                                *ptr = (byte)val;
                                *(ptr + 1) = (byte)((uint)val >> 8);


#else

                                convbuffer[ptr] = (byte)(val);
                                convbuffer[ptr + 1] = (byte)((uint)val >> 8);

#endif

                            }
                            else
                            {

#if UNSAFE

                                *ptr = (byte)((uint)val >> 8);
                                *(ptr + 1) = (byte)val;

#else

                                convbuffer[ptr] = (byte)((uint)val >> 8);
                                convbuffer[ptr + 1] = (byte)(val);

#endif

                            }

                            ptr += 2 * (vi.channels);
                        }
                    }

#if UNSAFE

                }
            }

#endif

            if (clipflag)
            {
                // throw new Exception("Clipping in frame "+vd.sequence);
            }
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// Inializes and gets ready to decode. The stream will be seekable.
        /// </summary>
        public void Initialize(Stream input)
        {
            this.Initialize(input, true);
        }

        /// <summary>
        /// Initializes and gets ready to decode. The stream will NOT be seekable, but it will get initialized faster.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="seekable"></param>
        public void Initialize(Stream input, bool seekable)
        {
            if (initialized)
            {
                throw new InvalidOperationException("Already initialized");
            }

            this.input = input;
            this.seekable = seekable;
            ReadHeader();

            initialized = true;
        }

        /// <summary>
        /// Disposes of all resources
        /// </summary>
        public void Dispose()
        {
            os.clear();
            vb.clear();
            vd.clear();
            vi.clear();
            oy.clear();
        }

        /// <summary>
        /// Enumerates PCM chunks
        /// </summary>
        /// <returns>Enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Enumerates PCM chunks
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<PCMChunk> GetEnumerator()
        {
            int bytesRead = 0;

            while (eos == 0)
            {
                while (eos == 0)
                {
                    int result = oy.pageout(og);
                    if (result == 0) break; // need more data
                    if (result == -1)
                    {
                        // missing or corrupt data at this page position
                        throw new Exception("Corrupt or missing data in bitstream...");
                    }
                    else
                    {
                        os.pagein(og); // can safely ignore errors at this point
                        while (true)
                        {
                            result = os.packetout(op);

                            if (result == 0) break; // need more data
                            if (result == -1)
                            { // missing or corrupt data at this page position
                                // no reason to complain; already complained above
                            }
                            else
                            {
                                // we have a packet.  Decode it
                                int samples;

                                if (vb.synthesis(op) == 0)
                                { // test for success!
                                    vd.synthesis_blockin(vb);
                                }

                                // **pcm is a multichannel float vector.  In stereo, for
                                // example, pcm[0] is left, and pcm[1] is right.  samples is
                                // the size of each channel.  Convert the float values
                                // (-1.<=range<=1.) to whatever PCM format and write it out

                                while ((samples = vd.synthesis_pcmout(_pcm, _index)) > 0)
                                {
                                    float[][] pcm = _pcm[0];
                                    int bout = (samples < convsize ? samples : convsize);
                                    int chunkSize = 2 * vi.channels * bout;
                                    pcm_offset += samples;

                                    if (bytesRead + chunkSize > convbuffer.Length)
                                    {
                                        PCMChunk chunk = new PCMChunk { Bytes = convbuffer, Channels = vi.channels, Length = bytesRead, Rate = vi.rate };
                                        bytesRead = 0;
                                        yield return chunk;
                                    }

                                    ConvertChunk(bout, bytesRead, pcm);

                                    bytesRead += chunkSize;
                                    vd.synthesis_read(bout); // tell libvorbis how many samples we actually consumed
                                }
                            }
                        }
                        if (og.eos() != 0)
                        {
                            eos = 1;
                        }
                    }
                }
                if (eos == 0)
                {
                    ReadNextPage();
                }
            }

            if (bytesRead != 0)
            {
                PCMChunk chunk = new PCMChunk { Bytes = convbuffer, Channels = vi.channels, Length = bytesRead, Rate = vi.rate };
                yield return chunk;
            }
            else
            {
                yield break;
            }
        }

        /// <summary>
        /// Resets and prepares for enumeration again
        /// </summary>
        public void Reset()
        {
            eos = 0;
            vd.synthesis_init(vi); // central decode state
            vb.init(vd);           // local state for most of the decode
            os.reset();
            oy.reset();
            input.Position = 0;
            pcm_offset = 0;
        }

        /// <summary>
        /// Decodes an ogg stream into a full PCM chunk (useful for sound effects)
        /// </summary>
        /// <param name="input">Input ogg data</param>
        /// <returns>Fully decoded PCM data</returns>
        public static PCMChunk Decode(Stream input)
        {
            OggDecoder decoder = new OggDecoder();
            decoder.Initialize(input);
            MemoryStream ms = new MemoryStream(4096);
            foreach (PCMChunk chunk in decoder)
            {
                ms.Write(chunk.Bytes, 0, chunk.Length);
            }
            return new PCMChunk { Bytes = ms.ToArray(), Channels = (decoder.Stereo ? 2 : 1), Length = (int)ms.Length, Rate = decoder.SampleRate };
        }

        #endregion Public methods

        #region Public properties

        /// <summary>
        /// The stream being read from
        /// </summary>
        public Stream Stream
        {
            get { return input; }
        }

        /// <summary>
        /// Sample rate
        /// </summary>
        public int SampleRate
        {
            get;
            private set;
        }

        /// <summary>
        /// True for stereo, false for mono
        /// </summary>
        public bool Stereo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets / sets the position in seconds
        /// </summary>
        public float Position
        {
            get { return time_tell(); }
            set { time_seek(value); }
        }

        /// <summary>
        /// Gets the total length of the stream in seconds
        /// </summary>
        public float Length
        {
            get;
            private set;
        }

        #endregion Public properties
    }
}