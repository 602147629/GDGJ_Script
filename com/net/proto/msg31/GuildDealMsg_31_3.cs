/**
 * 管理公会 (自动生成，请勿编辑！)
 */

using System;
using System.IO;
using System.Collections.Generic;

using PCustomDataType;
    

namespace Proto
{
    public class GuildDealMsg_31_3
  	{

    public ushort code = 0;

    public static int getCode()
    {
        // (31, 3)
        return 7939;
    }

    public void read(MemoryStream msdata)
    {
        code = proto_util.readUShort(msdata);
    }
   }
}