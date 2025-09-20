
namespace Qiandao.Model.Entity
{
    public class Device
    {
        public required int Id { get; set; }

        public required string Serial_num { get; set; }

        public required int Status { get; set; }
        public required int TenantId { get; set; }
    }

    public class DeviceNew
    {
        public required int Id { get; set; }

        public required string Serial_num { get; set; }
        public required string DeviceName { get; set; }

        public  int Status { get; set; }
    }

}
