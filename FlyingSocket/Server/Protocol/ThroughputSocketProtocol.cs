using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using FlyingSocket.Core;

namespace FlyingSocket.Server.Protocol
{
    class ThroughputSocketProtocol : BaseSocketProtocol
    {
        public ThroughputSocketProtocol(FlyingSocketServer socketServer, SocketUserToken userToken)
            : base("Throughput",socketServer, userToken)
        {
        }

        public override void Close()
        {
            base.Close();
        }

        public override bool ProcessCommand(byte[] buffer, int offset, int count) //处理分完包的数据，子类从这个方法继承
        {
            ThroughputSocketCommand command = ParseCommand(IncomingDataParser.Command);
            OutgoingDataAssembler.Clear();
            OutgoingDataAssembler.AddResponse();
            OutgoingDataAssembler.AddCommand(IncomingDataParser.Command);
            if (command == ThroughputSocketCommand.CyclePacket)
            {
                return DoCyclePacket(buffer, offset, count);
            }
            else
            {
                //Program.Logger.Error("Unknow command: " + m_incomingDataParser.Command);
                return false;
            }
        }

        public ThroughputSocketCommand ParseCommand(string command)
        {
            if (command.Equals(ProtocolKey.CyclePacket, StringComparison.CurrentCultureIgnoreCase))
            {
                return ThroughputSocketCommand.CyclePacket;
            }
            else
            {
                return ThroughputSocketCommand.None;
            }
        }

        public bool DoCyclePacket(byte[] buffer, int offset, int count)
        {
            int cycleCount = 0;
            if (IncomingDataParser.GetValue(ProtocolKey.Count, ref cycleCount))
            {
                OutgoingDataAssembler.AddSuccess();
                cycleCount = cycleCount + 1;
                OutgoingDataAssembler.AddValue(ProtocolKey.Count, cycleCount);
            }
            else
            {
                OutgoingDataAssembler.AddFailure(ProtocolCode.ParameterError, "");
            }
            return SendResult(buffer, offset, count);
        }
    }
}
