﻿



using WebSocketSharp;

namespace Qiandao.Model.Entity
{
   public class DeviceStatus
    {
        public string? deviceSn { get; set; }
        public WebSocket ? webSocket { get; set; }  // WebSocket 类型属性
        public int? status { get; set; }
        public string? ConnectionUri { get; set; }        // WebSocket 连接的 URI
        public int? ReconnectAttempts { get; set; }        // 重连尝试次数
        public override string ToString()
        {
            return $"deviceSn: {this.deviceSn}, status: {this.status}, ConnectionUri: {this.ConnectionUri}, ReconnectAttempts: {this.ReconnectAttempts}"; // 根据类的属性调整
        }
    }
}
