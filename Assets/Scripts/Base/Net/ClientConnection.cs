using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace WebSockets
{
    public class ClientConnection
    {
        private readonly TcpClient m_tcpClient;
        private volatile bool m_isConnected;
        //0=未断开, 1=已开始断开, 保证多线程下 Disconnect 只执行一次
        private int m_disconnectStarted;

        public Guid Id { get; private set; }

        /// <summary>当前连接是否仍处于连接状态</summary>
        public bool IsConnected => m_isConnected;

        public event Action<ClientConnection, string> ReceivedTextualData = delegate { };
        public event Action<ClientConnection, byte[]> ReceivedBinaryData = delegate { };
        public event Action<ClientConnection> Disconnected = delegate { };

        public ClientConnection(Guid id, TcpClient tcpClient)
        {
            m_tcpClient = tcpClient;
            Id = id;
            m_isConnected = true;
        }

        public void StartReceiving()
        {
            var buffer = new byte[1024];
            var socketAsyncEventArgs = new SocketAsyncEventArgs();

            socketAsyncEventArgs.Completed += OnDataReceived;
            socketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);

            bool isAsync = m_tcpClient.Client.ReceiveAsync(socketAsyncEventArgs);
            if (!isAsync)
                OnDataReceived(m_tcpClient, socketAsyncEventArgs);
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            if (!m_isConnected)
                return;

            int numberOfBytesReceived = e.SocketError != SocketError.Success ? 0 : e.BytesTransferred;
            if (numberOfBytesReceived <= 0)
            {
                Disconnect();
                return;
            }

            if (HandleFrame(e))
                StartReceiving();
        }

        private bool HandleFrame(SocketAsyncEventArgs args)
        {
            Frame frame = Frame.FromBuffer(args.Buffer);

            if (frame.Opcode == Frame.Opcodes.Close)
            {
                Disconnect();
                return false;
            }

            // Note: No support for fragmented messages
            if (frame.Opcode == Frame.Opcodes.Binary)
                ReceivedBinaryData(this, frame.UnmaskedPayload);
            else if (frame.Opcode == Frame.Opcodes.Text)
            {
                string textContent = Encoding.UTF8.GetString(frame.UnmaskedPayload, 0, (int)frame.PayloadLength);
                ReceivedTextualData(this, textContent);
            }

            return true;
        }

        public void Send(byte[] data)
        {
            var frame = new Frame(Frame.Opcodes.Binary, data, true);
            Send(frame);
        }

        public void Send(string data)
        {
            var frame = new Frame(Frame.Opcodes.Text, Encoding.UTF8.GetBytes(data), true);
            Send(frame);
        }

        private void Send(Frame frame)
        {
            if (!m_isConnected)
                return;

            byte[] buffer = frame.ToBuffer();

            var sendEventArgs = new SocketAsyncEventArgs();
            sendEventArgs.SetBuffer(buffer, 0, buffer.Length);

            m_tcpClient.Client.SendAsync(sendEventArgs);
        }

        public void Disconnect()
        {
            //已断开直接返回，避免重复触发 Disconnected 事件
            if (!m_isConnected)
                return;

            //接收回调线程和业务线程可能同时触发断开，保证只清理一次
            if (System.Threading.Interlocked.CompareExchange(ref m_disconnectStarted, 1, 0) != 0)
                return;

            //先发送关闭帧，再标记断开
            try
            {
                Frame closingFrame = Frame.CreateClosingFrame();
                Send(closingFrame);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"发送 WebSocket 关闭帧失败: {ex.Message}");
            }

            m_isConnected = false;

            try
            {
                if (m_tcpClient.Client != null && m_tcpClient.Client.Connected)
                    m_tcpClient.Client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"关闭 WebSocket 连接失败: {ex.Message}");
            }
            finally
            {
                //必须保证 socket 释放和 Disconnected 事件不会被异常跳过
                try
                {
                    m_tcpClient.Close();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"释放 WebSocket 连接失败: {ex.Message}");
                }
                finally
                {
                    Disconnected(this);
                }
            }
        }
    }
}