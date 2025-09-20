using Microsoft.AspNetCore.Mvc;
using Qiandao.Model.Entity;
using Qiandao.Model.Response;
using Qiandao.Service;
using Qiandao.Web.Extensions;
using Qiandao.Web.WebSocketHandler;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Qiandao.Web.Controllers
{
    [Route("api/")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly PersonService _personService;
        private readonly EnrollinfoService _enrollInfoService;
        private readonly ILogger<PersonController> _logger;
        public PersonController(ILogger<PersonController> logger, PersonService personService, EnrollinfoService enrollInfoService)
        {
            _logger = logger;
            _personService = personService;
            _enrollInfoService = enrollInfoService;
        }

        [HttpPost("addPerson")]
        public async Task<IActionResult>  AddPerson([FromForm]  PersonTemp personTemp, [FromForm] IFormFile? pic)
        {
            int tenantId = Convert.ToInt32(HttpContext.Session.GetObject<string>("TenantId"));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (personTemp == null)
            {
                return BadRequest("Invalid form personTemp");
            }
            string newName = "";
            // 检查 pic 是否为空
            if (pic != null)
            {
                // 检查文件类型和大小
                if (pic.Length > 1024 * 1024) // 限制文件大小为1MB
                {
                    return BadRequest("File is too large.");
                }
                // 检查文件扩展名
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(pic.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Unsupported file extension.");
                }
                var path = Program.GetConfiguration().GetValue<string>("ImagePath:Url");
                if (path != null)
                {
                    string v = await ProcessImage(pic, path);
                    newName = v;
                }
            }
            // 处理文件
            try
            {
                Person p = new Person
                {
                    Id = personTemp.UserId,
                    Name = personTemp.Name,
                    Roll_id = personTemp.Privilege,
                    TenantId = tenantId
                };

                var existingPerson =  _personService.SelectByPrimaryKey(personTemp.UserId,tenantId);
                if (existingPerson.Result == null)
                {
                     _personService.AddPersonAsync(p);
                }
                 SaveAdditionalInformation(personTemp, newName, tenantId);

                var response = new Dictionary<string, object>
            {
                { "code", 0 },
                { "msg", "success" }
            };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }



        [HttpPost("addRemoteUser")]
        public async Task<IActionResult> addRemoteUser([FromForm] PersonTemp personTemp, [FromForm] IFormFile? pic , string devicesn)
        {
            int tenantId = Convert.ToInt32(HttpContext.Session.GetObject<string>("TenantId"));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (personTemp == null)
            {
                return BadRequest("Invalid form personTemp");
            }
            string newName = "";
            // 检查 pic 是否为空
            if (pic != null)
            {
                // 检查文件类型和大小
                if (pic.Length > 1024 * 1024) // 限制文件大小为1MB
                {
                    return BadRequest("File is too large.");
                }
                // 检查文件扩展名
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(pic.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Unsupported file extension.");
                }
                var path = Program.GetConfiguration().GetValue<string>("ImagePath:Url");
                if (path != null)
                {
                    string v = await ProcessImage(pic, path);
                    newName = v;
                }
            }
            // 处理文件
            try
            {
                Person p = new Person
                {
                    Id = personTemp.UserId,
                    Name = personTemp.Name,
                    Roll_id = personTemp.Privilege,
                    TenantId = tenantId
                };

                _personService.DeletePersonIfExists(personTemp.UserId, tenantId);

                var existingPerson = _personService.SelectByPrimaryKey(personTemp.UserId,tenantId);
                if (existingPerson.Result == null)
                {
                    _personService.AddRemoteUser(p , devicesn);
                }
              //  SaveAdditionalInformation(personTemp, newName);

                var response = new Dictionary<string, object>
                {
                    { "code", 0 },
                    { "msg", "success" }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }


        private async Task<string> ProcessImage(IFormFile file, string path)
        {
            if (file == null || string.IsNullOrEmpty(file.FileName)) return null;
            string newName = Guid.NewGuid().ToString();
            string fullPath = Path.Combine(path, newName + ".jpg");
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // 读取文件内容到字节数组
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            // 将字节数组转换为 Base64 字符串
            string base64String = Convert.ToBase64String(fileBytes);
            ImageProcess.Base64ToImage(base64String, newName + ".jpg");
            return newName; 
        }
        
        private void SaveAdditionalInformation(PersonTemp personTemp, string newName, int tenantId)
        {
            if (!string.IsNullOrEmpty(personTemp.Password))
            {
                 _enrollInfoService.AddEnrollInfo(new Enrollinfo
                {
                    Backupnum = 10,
                    Enroll_id = personTemp.UserId,
                    Signatures = personTemp.Password,
                    TenantId = tenantId
                });
            }

            if (!string.IsNullOrEmpty(personTemp.CardNum))
            {
                 _enrollInfoService.AddEnrollInfo(new Enrollinfo
                {
                    Backupnum = 11,
                    Enroll_id = personTemp.UserId,
                    Signatures = personTemp.CardNum,
                     TenantId = tenantId
                 });
            }

            if (!string.IsNullOrEmpty(newName))
            {
                string imagePath = newName + ".jpg";
             var _filePath=   Program.GetConfiguration().GetValue<string>("ImagePath:Url");
                if (_filePath != null)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(_filePath, imagePath));
                    string base64String = Convert.ToBase64String(fileBytes);
                    _enrollInfoService.AddEnrollInfo(new Enrollinfo
                    {
                        Backupnum = 50,
                        Enroll_id = personTemp.UserId,
                        ImagePath = imagePath,
                        Signatures = base64String,
                        TenantId = tenantId
                    });
                }
            }
        }


        [HttpGet("setUsernameToDevice")]
        public  async Task<IActionResult> setUsernameToDevice(string deviceSn)
        {
            int tenantId = Convert.ToInt32(HttpContext.Session.GetObject<string>("TenantId"));
            ResponseModel pm=await  _personService.GetPersonallList(tenantId);
            if (pm.Data == null) {
                return BadRequest(new { code = 1, msg = "Fail" });
            }
            List<Person> persons = pm.Data;
            ResponseModel  en=  _enrollInfoService.setUsernameToDevice(deviceSn, persons);
            if (en.Code == 200)
            {
                return Ok(new { code = 0, msg = "Success" });
            }
            else {
                return BadRequest(new { code = 1, msg = "Fail" });
            }
        }

        [HttpGet("setSpecificUsernameToDevice")]
        public  async Task<IActionResult> setSpecificUsernameToDevice(string deviceSn, string userIds)
        {
            int tenantId = Convert.ToInt32(HttpContext.Session.GetObject<string>("TenantId"));
            ResponseModel pm=await  _personService.GetSpecificPersonallList(userIds);
            if (pm.Data == null) {
                return BadRequest(new { code = 1, msg = "Fail" });
            }
            List<Person> persons = pm.Data;
            ResponseModel  en=  _enrollInfoService.setUsernameToDevice(deviceSn, persons);
            if (en.Code == 200)
            {
                return Ok(new { code = 0, msg = "Success" });
            }
            else {
                return BadRequest(new { code = 1, msg = "Fail" });
            }
        }


        [HttpGet("initSystem")]
        public IActionResult initSystem(string deviceSn)
        {
            ResponseModel en = _enrollInfoService.initSystem(deviceSn);
            if (en.Code == 200)
            {
                return Ok(new { code = 0, msg = "Success" });
            }
            else
            {
                return BadRequest(new { code = 1, msg = "Fail" });
            }
        }
    }
}
