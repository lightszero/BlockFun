using System;
using System.Collections.Generic;
using System.Text;

public class LZMAHelper
{
    public static System.IO.Stream Compress(System.IO.Stream stream, uint length, bool EOF = false)
    {
        SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
        System.IO.MemoryStream sout = new System.IO.MemoryStream();
        if (EOF)
        {
            encoder.SetCoderProperties(new SevenZip.CoderPropID[] { SevenZip.CoderPropID.EndMarker }, new object[] { true });
        }
        else
        {
            //写入文件长度
            sout.Write(BitConverter.GetBytes(length), 0, 4);
        }
        encoder.WriteCoderProperties(sout);
        encoder.Code(stream, sout, (int)length, -1, null);
        sout.Position = 0;
        return sout;

    }
    public static System.IO.Stream DeCompress(System.IO.Stream stream, uint length, bool EOF = false)
    {
        SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
        int tagsize = 5;

        int filelen = -1;//如果压缩时使用了结束标志选项，则这里传-1，解压缩会知道文件长度
        if (!EOF)
        {
            //读取文件长度
            byte[] lbuf = new byte[4];
            stream.Read(lbuf, 0, 4);
            filelen = (int)BitConverter.ToUInt32(lbuf, 0);
            tagsize += 4;
        }
        //读取压缩属性
        byte[] properties = new byte[5];
        if (stream.Read(properties, 0, 5) != 5)
            throw (new Exception("input .lzma is too short"));
        decoder.SetDecoderProperties(properties);
        System.IO.MemoryStream sout = new System.IO.MemoryStream();
        decoder.Code(stream, sout, length - tagsize, filelen, null);
        sout.Position = 0;
        return sout;
    }
}
