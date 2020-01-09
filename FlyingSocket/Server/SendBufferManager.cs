using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlyingSocket.Core;

namespace FlyingSocket.Server
{
    /// <summary>
    /// 发送数据包
    /// </summary>
    internal struct SendBufferPacket
    {
        /// <summary>
        /// 偏移量
        /// </summary>
        public int Offset;
        /// <summary>
        /// 数据量
        /// </summary>
        public int Count;
    }

    //由于是异步发送，有可能接收到两个命令，写入了两次返回，发送需要等待上一次回调才发下一次的响应

    /// <summary>
    /// 异步数据发送管理工具
    /// </summary>
    public class SendBufferManager
    {
        /// <summary>
        /// 数据缓存
        /// </summary>
        public DynamicBufferManager DynamicBufferManager { get; private set; }
        /// <summary>
        /// 发送数据缓存集合
        /// </summary>
        private List<SendBufferPacket> _sendBufferList = null;
        private SendBufferPacket _sendBufferPacket;

        public SendBufferManager(int bufferSize)
        {
            DynamicBufferManager = new DynamicBufferManager(bufferSize);
            _sendBufferList = new List<SendBufferPacket>();
            _sendBufferPacket.Offset = 0;
            _sendBufferPacket.Count = 0;
        }

        public void StartPacket()
        {
            _sendBufferPacket.Offset = DynamicBufferManager.DataCount;
            _sendBufferPacket.Count = 0;
        }

        public void EndPacket()
        {
            _sendBufferPacket.Count = DynamicBufferManager.DataCount - _sendBufferPacket.Offset;
            _sendBufferList.Add(_sendBufferPacket);
        }

        public bool GetFirstPacket(ref int offset, ref int count)
        {
            if (_sendBufferList.Count <= 0)
            {
                return false;
            }
            offset = 0;//_sendBufferList[0].Offset;清除了第一个包后，后续的包往前移，因此Offset都为0
            count = _sendBufferList[0].Count;
            return true;
        }

        /// <summary>
        /// 清除已发送的包
        /// </summary>
        /// <returns></returns>
        public bool ClearFirstPacket()
        {
            if (_sendBufferList.Count <= 0)
            {
                return false;
            }
            int count = _sendBufferList[0].Count;
            DynamicBufferManager.Clear(count);
            _sendBufferList.RemoveAt(0);
            return true;
        }

        public void ClearPacket()
        {
            _sendBufferList.Clear();
            DynamicBufferManager.Clear(DynamicBufferManager.DataCount);
        }
    }
}
