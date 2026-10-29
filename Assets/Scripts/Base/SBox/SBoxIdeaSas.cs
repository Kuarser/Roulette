/**
 * @file SBoxIdeaSas.cs
 * @brief U3D 与 IDEA SAS 接口。 *
 * 本文件只负责 SAS 命令的请求封装和响应解析，底层数据通过
 * SBoxIOStream 与 IDEA 通信。SAS 主机及真实 Table C-7 meter 数据
 * 由 QSoftSasEgmBoard 提供，U3D 不维护本地 meter 累加值。 */
using Hal;
using System;

namespace SBoxApi
{
    /** IDEA cmd 20508 返回，Board/SAS 控制及退票事务状态，*/
    public class SBoxSasExtendedState
    {
        public int linked;          // Game Link 是否已连接：0 否，1 ，        
        public int statusValid;     // Board 状态是否有效：0 否，1 ，        
        public int playEnabled;     // SAS 主机是否允许游戏， 否，1 ，        
        public int soundEnabled;    // SAS 主机是否允许声音， 否，1 ，        
        public int aftLockState;    // AFT 锁定状态，，Board 定义的状态，        
        public int handpayPending;  // 是否存在 handpay， 否，1 ，        
        public int cashoutState;    // 退票事务状态，，IDEA SAS 状态，        
        public int cashoutError;    // 最近一次退票错误码， 表示无错，        
        public int featureFlags;    // Board/ Game Link 功能，    
    }

    /**
     * IDEA cmd 20509 exact Table C-7 meters.
     * Each uint64 meter is transported as two int values: low 32 bits, then high 32 bits.
     */

    public class SBoxSasMeters
    {
        public int status;           // 0 成功，1 参数错误，2 meter 尚未刷新完整
        public uint validMaskLow;    // meter 0..31 的有效位
        public uint validMaskHigh;   // meter 32..39 的有效位
        public uint maxAgeMs;        // 本次返回中最老有，meter 的年龄（毫秒，
        public ulong[] values = new ulong[40]; // Exact meter values

    }

    /** 20502 返回的打印机和纸钞机状态，*/
    public class SBoxSasDeviceStatus
    {
        public int result;
        public int printerStatus; // 打印机状态：，8 位为标准状态值；bit 8=1 表示打印机忙， 表示空闲
                                  // 状态值：0 空闲， 纸张不足， 缺纸， 打印，切刀故障， 离线， 未知
        public int billStatus;    // 纸钞机状态：bit 0=1 在线， 未连接；bit 8=1 有纸钞暂存（escrow，                                  // bits 16..23 ，Board 返回，SAS 纸钞机状态字节，其他位保，    
    }

    public partial class SBoxIdea
    {
        /**
         * 查询 Board/SAS 主机控制状态及退票事务状态，         
         * 请求参数：无业务参数，内部发，data=[0]，         
         * 返回值：无同步返回值；通过 SasExtendedStateReceived 异步返回状态对象，         
         */
        public static void SasGetExtendedState()
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20508, source: 1, target: 2, size: 1);
            packet.data[0] = 0;
            SBoxIOEvent.AddListener(packet.cmd, SasGetExtendedStateR);
            SBoxIOStream.Write(packet);
        }

        /** 解析 20508 ，9 个整数字段并触发响应事件，*/
        private static void SasGetExtendedStateR(SBoxPacket packet)
        {
            if (packet == null || packet.data == null || packet.data.Length < 9)
                return;
            SBoxSasExtendedState state = new SBoxSasExtendedState
            {
                linked = packet.data[0],
                statusValid = packet.data[1],
                playEnabled = packet.data[2],
                soundEnabled = packet.data[3],
                aftLockState = packet.data[4],
                handpayPending = packet.data[5],
                cashoutState = packet.data[6],
                cashoutError = packet.data[7],
                featureFlags = packet.data[8]
            };
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SAS_EXTENDED_STATE, state);
        }

        /**
         * 查询 Board 采集的真实 Table C-7 meter，         
         * 请求参数：无业务参数，内部发，data=[0]，         
         * 返回值：无同步返回值；通过 SasMetersReceived 异步返回 meter 对象，         
         */
        public static void SasGetMeters()
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20509, source: 1, target: 2, size: 1);
            packet.data[0] = 0;
            SBoxIOEvent.AddListener(packet.cmd, SasGetMetersR);
            SBoxIOStream.Write(packet);
        }

        /** Parse status fields plus 40 uint64 meters encoded as int pairs. */
        private static void SasGetMetersR(SBoxPacket packet)
        {
            if (packet == null || packet.data == null || packet.data.Length < 84)
                return;
            SBoxSasMeters meters = new SBoxSasMeters
            {
                status = packet.data[0],
                validMaskLow = unchecked((uint)packet.data[1]),
                validMaskHigh = unchecked((uint)packet.data[2]),
                maxAgeMs = unchecked((uint)packet.data[3])
            };
            for (int i = 0; i < meters.values.Length; i++)
            {
                int offset = 4 + i * 2;
                ulong low = unchecked((uint)packet.data[offset]);
                ulong high = unchecked((uint)packet.data[offset + 1]);
                meters.values[i] = low | (high << 32);
            }
            EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SAS_METERS, meters);
        }

        /**
         * 通知 IDEA 更新 SAS 时间，0500），        
         * @param seconds Unix 时间戳，单位为秒，         
         * @return 无；该命令为兼容保留接口，不产生业务响应，         
         */
        public static void SasUpdateTime(uint seconds)
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20500, source: 1, target: 2, size: 1);
            packet.data[0] = unchecked((int)seconds);
            SBoxIOStream.Write(packet);
        }

        /**
         * 发起退票（20501），         
         * @param amount 退票金额，使用游戏 score 单位，必须大，0，         
         * @return 无同步返回值；通过 SasCashoutResultReceived 返回结果码：
         *         0 表示已接受，负值表示参数、余额或当前状态错误，         
         */
        
        public static void SasCashout(int amount)
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20501, source: 1, target: 2, size: 1);
            packet.data[0] = amount;
            SBoxIOEvent.AddListener(packet.cmd, SasCashoutR);
            SBoxIOStream.Write(packet);
        }

        private static void SasCashoutR(SBoxPacket packet)
        {
            if (packet != null && packet.data != null && packet.data.Length > 0)
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SAS_CASH_OUT_RESULT, packet.data[0]);
        }

        /**
         * 查询打印机和纸钞机状态（20502），         
         * @return 无同步返回值；SasDeviceStatusReceived 返回 result、printerStatus
         *         ，billStatus，SasPrinterStatusReceived 保留旧版数组回调，         
         */
        public static void SasGetPrinterStatus()
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20502, source: 1, target: 2, size: 1);
            packet.data[0] = 0;
            SBoxIOEvent.AddListener(packet.cmd, SasPrinterStatusR);
            SBoxIOStream.Write(packet);
        }
        private static void SasPrinterStatusR(SBoxPacket packet)
        {
            if (packet != null && packet.data != null && packet.data.Length >= 2)
            {
                SBoxSasDeviceStatus status = new SBoxSasDeviceStatus
                {
                    result = packet.data[0],
                    printerStatus = packet.data[1],
                    billStatus = packet.data.Length >= 3 ? packet.data[2] : 0
                };
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SAS_DEVICE_STATUS, status);
            }
        }

        /**
         * 读取旧版 20503 meter 兼容数组，         
         * @return 无同步返回值；通过 SasLegacyMetersReceived 返回兼容数组，         
         *         新代码应使用 SasGetMeters 以避，32 位数值精度损失，         
         */
        public static void SasGetLegacyMeters()
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20503, source: 1, target: 2, size: 1);
            packet.data[0] = 0;
            SBoxIOEvent.AddListener(packet.cmd, SasLegacyMetersR);
            SBoxIOStream.Write(packet);
        }
        private static void SasLegacyMetersR(SBoxPacket packet)
        {
            if (packet != null && packet.data != null)
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SAS_LEGACY_METERS, packet.data);
        }

        /**
         * 报告游戏开始（20504），         
         * @param playerId 玩家编号；当，Board 适配器保留该字段，         
         * @param bet 本局下注金额，游，score 单位，         * @param credit 下注后玩家余额，游戏 score 单位，         
         * @return 无同步返回值；通过 SasGameBeginResultReceived 返回 0 或错误码，         
         */
        public static void SasGameBegin(int playerId, int bet, int credit)
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20504, source: 1, target: 2, size: 3);
            packet.data[0] = playerId;
            packet.data[1] = bet;
            packet.data[2] = credit;
            SBoxIOEvent.AddListener(packet.cmd, SasGameBeginR);
            SBoxIOStream.Write(packet);
        }
        private static void SasGameBeginR(SBoxPacket packet)
        {
            if (packet != null && packet.data != null && packet.data.Length > 0)
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SAS_GAME_BEGIN_RESULT, packet.data[0]);
        }

        /**
         * 报告游戏结束，0505），         
         * @param playerId 玩家编号；当，Board 适配器保留该字段，         
         * @param win 本局赢分，游，score 单位，         
         * @return 无同步返回值；通过 SasGameEndResultReceived 返回 0 或错误码，         
         */
        public static void SasGameEnd(int playerId, int win)
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20505, source: 1, target: 2, size: 2);
            packet.data[0] = playerId;
            packet.data[1] = win;
            SBoxIOEvent.AddListener(packet.cmd, SasGameEndR);
            SBoxIOStream.Write(packet);
        }
        private static void SasGameEndR(SBoxPacket packet)
        {
            if (packet != null && packet.data != null && packet.data.Length > 0)
                EventCenter.Instance.EventTrigger(SBoxEventHandle.SBOX_SAS_GAME_END_RESULT, packet.data[0]);
        }

        /**
         * 报告普，SAS 异常，0506），         
         * @param eventCode 0..255 ，SAS 事件码，         
         * @param data 附加数据，最，8 个字节；，null 表示没有附加数据，         
         * @return 无同步返回值；该通知命令不注册响应回调，         
         *
         * 常用 SAS Appendix A 异常码（eventCode）：
         * 0x00 无活动；
         * 0x11/0x12 机门开/关；
         * 0x13/0x14 投币门开/关；
         * 0x17/0x18 交流电源接，断开；
         * 0x19/0x1A 现金箱门开/关；
         * 0x20 通用 tilt，
         * 0x21 投币倾斜，0x22 出币倾斜，0x23 hopper 空；
         * 0x24 多付硬币，x25 分币器故障；0x26 玩家发起退票；0x27 现金箱满，         
         * 0x28 纸钞卡钞，x29 纸钞机硬件故障；0x2A 检测到反向纸钞，         
         * 0x2B 纸钞拒收，x2C 假钞，x2D 反向投币，x2E 现金箱接近满，         
         * 0x3D 退票打印完成；0x3E handpay 已验证；0x3F 验证未配置；
         * 0x40..0x46 转轮/转轮机构倾斜或断开，x47..0x50 各面额纸钞接收；
         * 0x51 handpay 等待，x52 handpay 已重置；0x53 progressive 超时，         
         * 0x54 progressive 赢取，x55 玩家取消 handpay，x56 progressive 等级命中，         
         * 0x57 系统验证请求，x60 打印机通信故障，x61 打印机缺纸；
         * 0x67 票据插入，x68 票据传输完成，x69 AFT 传输完成，         
         * 0x6A/0x6B 主机 AFT 退票请求；0x6C..0x6E AFT 注册状态；0x6F 游戏锁定，         
         * 0x70 异常缓冲区溢出；0x74 打印机纸张不足；0x75/0x76 打印机断，上电，         
         * 0x77 色带更换，x78 打印机走纸机构卡住；0x79 投币锁定故障，         
         * 0x7A 软表归零，x7B 纸钞机总数归零，x7C legacy bonus，         
         * 0x7E/0x7F 游戏开，结束，x80/0x81 hopper ，液位低；
         * 0x8E 组件列表变更，x8F 认证完成，         
         */
        public static void SasReportException(int eventCode, int[] data)
        {
            int dataLength = data == null ? 0 : data.Length;
            if (dataLength > 8)
                return;
            SBoxPacket packet = new SBoxPacket(cmd: 20506, source: 1, target: 2, size: dataLength + 1);
            packet.data[0] = eventCode;
            for (int i = 0; i < dataLength; i++)
                packet.data[i + 1] = data[i];
            SBoxIOStream.Write(packet);
        }

        /**
         * 设置优先 SAS 异常，0507），         
         * @param eventCode 0..255 ，SAS 事件码，         
         * @param active 0 清除事件， 激活事件，         
         * @return 无同步返回值；该设置命令不注册响应回调，         
         *
         * 当前 Board 支持的优先异常码，         
         * 0x57 系统验证请求，x67 票据已插入；0x68 票据传输完成，         
         * 0x3F 验证未配置；0x6A/0x6B AFT 主机退票请求；
         * 0x3D 退票打印完成；0x3E handpay 已验证；
         * 0x51 handpay 等待，x52 handpay 已重置；0x55 handpay 已取消，         
         */
        public static void SasSetPriorityException(int eventCode, int active)
        {
            SBoxPacket packet = new SBoxPacket(cmd: 20507, source: 1, target: 2, size: 2);
            packet.data[0] = eventCode;
            packet.data[1] = active;
            SBoxIOStream.Write(packet);
        }
    }
}
