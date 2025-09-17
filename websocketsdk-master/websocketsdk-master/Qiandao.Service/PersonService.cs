using Qiandao.Model.Entity;
using Qiandao.Model.Response;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
namespace Qiandao.Service
{

    /// <summary>
    ///person服务
    /// </summary>
    public class PersonService : IScope
    {
        private readonly object _lockObject = new object();  // 专门的锁对象
        private bool _disposed = false;  // 标记是否已释放资源
        public void Dispose()
        { }

        private readonly ILogger<PersonService> _logger;
        private readonly Db _db;
        private readonly HRMDb _hrmdb;
        private readonly IMapper _mapper;
        private readonly EnrollinfoService enrollinfoService;
        private readonly Machine_commandService machine_CommandService;
        public PersonService(Db db, HRMDb hrmdb, IMapper mapper, ILogger<PersonService> logger, EnrollinfoService _enrollinfoService, Machine_commandService _machine_CommandService)
        {
            _logger = logger;
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _hrmdb = hrmdb ?? throw new ArgumentNullException(nameof(hrmdb));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.enrollinfoService = _enrollinfoService;
            this.machine_CommandService = _machine_CommandService;
        }
        /// <summary>
        /// 添加enrollinfo
        /// </summary>
        [HttpPost("addPerson")]
        public IActionResult AddPerson(Person addPerson)
        {
            lock (_lockObject)  // 确保同一时间只有一个线程访问
            {
                var response = AddPersonAsync(addPerson);
                return new OkObjectResult(response);
            }
        }
        public void DeletePersonIfExists(long id)
        {
            var person = _db.person.FirstOrDefault(a => a.Id == id);
            if (person != null) _db.Remove(person);
            _db.SaveChanges();
        }

        public ResponseModel AddPersonAsync(Person aperson)
        {
            lock (_lockObject)  // 确保同一时间只有一个线程访问
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.person.Add(aperson);
                        int rowsAffected = _db.SaveChanges();
                        if (rowsAffected > 0)
                        {

                            SaveTMSUser(aperson.Name, aperson.Id.ToString());


                            transaction.Commit();
                            return new ResponseModel { Code = 200, Result = "Success" };
                        }
                        else
                        {
                            transaction.Rollback();
                            return new ResponseModel { Code = 0, Result = "Fail" };
                        }
                    }
                    catch (Exception ex)
                    {
                        return new ResponseModel { Code = 0, Result = "fail：" + ex.Message };
                    }
                }
            }
        }


        public bool SaveTMSUser(string Name, string EmpNumber)
        {
            string sql = @"
                IF NOT EXISTS (SELECT 1 FROM Employee WHERE EmpNumber = @EmpNumber)
                BEGIN
                    INSERT INTO Employee (Name, EmpNumber, IsActive, created_at)
                    VALUES (@Name, @EmpNumber, 1, GETDATE())
                END";

            var parameters = new List<SqlParameter> 
                {
                    new SqlParameter("@Name", Name),
                    new SqlParameter("@EmpNumber", EmpNumber)
                };

            // 执行更新操作
            return _hrmdb.Database.ExecuteSqlRaw(sql, parameters.ToArray()) > 0;
        }


        public ResponseModel GetPersonList(int page, int limit)
        {
            lock (_lockObject)  // 确保同一时间只有一个线程访问
            {
                var skipCount = (page-1) * limit;
                // 构建动态 SQL 查询
                var sqlQuery = $@"
    WITH CTE AS (
        SELECT *,  ROW_NUMBER() OVER (ORDER BY Id asc) AS RowNum
        FROM Person)
    SELECT *
    FROM CTE
    WHERE RowNum BETWEEN (@SkipCount+1)  AND (@SkipCount + @Limit)";
                // 创建参数列表
                var parameters = new List<SqlParameter>
    {
        new SqlParameter("@SkipCount", skipCount),
        new SqlParameter("@Limit", limit)
    };

                // 执行查询
                var query = _db.Database.SqlQueryRaw<Person>(sqlQuery, parameters.ToArray());
                // 获取查询结果
                
                if (query==null) {
                    return new ResponseModel();
                }
                    var queryResult = query.ToList();
                ResponseModel responseModel = new ResponseModel();
                responseModel.Data = new List<Person>();
                    foreach (var comment in queryResult)
                    {
                        responseModel.Data.Add(new Person
                        {
                            Id = comment.Id,
                            Roll_id = comment.Roll_id,
                            Name = comment.Name
                        });
                    }
                    responseModel.Code = 200;
                    responseModel.Result = "Person list success";
                    return responseModel;
                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public async Task<ResponseModel> GetPersonallList()
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // 异步等待
                var query = _db.Database.SqlQueryRaw<Person>(@"SELECT * FROM person");
                if (query == null)
                {
                    return new ResponseModel();
                }
                // 执行查询并返回结果
                var queryResult = query.ToList();
                ResponseModel responseModel = new ResponseModel();
                responseModel.Data = new List<Person>();
                foreach (var comment in queryResult)
                {
                    responseModel.Data.Add(new Person
                    {
                        Id = comment.Id,
                        Roll_id = comment.Roll_id,
                        Name = comment.Name
                    });
                }
                semaphore.Release();
                responseModel.Code = 200;
                responseModel.Result = "Person list success";
                return responseModel;
            }
        }
        public async Task<ResponseModel> GetSpecificPersonallList(string userIds)
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // 异步等待
                var query = _db.Database.SqlQueryRaw<Person>(@"SELECT * FROM person where id in ("+ userIds +")");
                if (query == null)
                {
                    return new ResponseModel();
                }
                // 执行查询并返回结果
                var queryResult = query.ToList();
                ResponseModel responseModel = new ResponseModel();
                responseModel.Data = new List<Person>();
                foreach (var comment in queryResult)
                {
                    responseModel.Data.Add(new Person
                    {
                        Id = comment.Id,
                        Roll_id = comment.Roll_id,
                        Name = comment.Name
                    });
                }
                semaphore.Release();
                responseModel.Code = 200;
                responseModel.Result = "Person list success";
                return responseModel;
            }
        }


        public async Task<Person> SelectByPrimaryKey(long id)
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // 异步等待
                var sqlQuery = $@"SELECT * FROM Person WHERE id = @id";
                var parameters = new SqlParameter[]
                {
                new SqlParameter("@id", id)
                };

                try
                {
                    var query = _db.Database.SqlQueryRaw<Person>(sqlQuery, parameters);
                    var queryResult = query.ToList();
                    semaphore.Release();
                    return queryResult.Count > 0 ? queryResult[0] : null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error");
                    return null;
                }
            }
        }
        public ResponseModel AddRemoteUser(Person aperson, string deviceSn)
        {
            lock (_lockObject)  // Ensure only one thread accesses this at a time
            {
                // First command to cancel any ongoing user addition operation
                string cancelMessage = "{\"cmd\":\"adduser\",\"cancel\":true}";
                DateTime dt = DateTime.Now;

                Machine_command cancelCommand = new Machine_command
                {
                    Content = cancelMessage,
                    Name = "adduser",
                    Status = 0,
                    Send_status = 0,
                    Err_count = 0,
                    Serial = deviceSn,
                    Gmt_crate = dt,
                    Gmt_modified = dt,
                };

                // Second command to actually add the user using the data from aperson
                string addUserMessage = "{\"cmd\":\"adduser\",\"enrollid\":" + aperson.Id +
                                              ",\"backupnum\":50," + // backupnum fixed to 50
                                              "\"admin\":0," + // admin is set to 0 (non-admin by default)
                                              "\"name\":\"" + aperson.Name + "\","
                                              + "\"flag\":10}"; // flag is set to 10, assuming it's part of the command

                Machine_command addUserCommand = new Machine_command
                {
                    Content = addUserMessage,
                    Name = "adduser",
                    Status = 0,
                    Send_status = 0,
                    Err_count = 0,
                    Serial = deviceSn,
                    Gmt_crate = dt,
                    Gmt_modified = dt,
                };

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        // Add the cancel command to the machine_command table
                        //_db.machine_command.Add(cancelCommand);
                        //int cancelRowsAffected = _db.SaveChanges();

                        if (1 > 0)
                        {
                            // Simulate waiting for the response (usually via some async method or callback)
                            // Here, we're assuming an immediate response to proceed with the add user command.
                            _db.machine_command.Add(addUserCommand);
                            int addRowsAffected = _db.SaveChanges();

                            if (addRowsAffected > 0)
                            {
                                // Commit the transaction if both commands were added successfully
                                transaction.Commit();
                                return new ResponseModel { Code = 200, Result = "Add user success" };
                            }
                            else
                            {
                                // Rollback if the add user command failed to be added
                                transaction.Rollback();
                                return new ResponseModel { Code = 0, Result = "Add user command failed" };
                            }
                        }
                        else
                        {
                            // Rollback if the cancel command failed to be added
                            transaction.Rollback();
                            return new ResponseModel { Code = 0, Result = "Cancel command failed" };
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rollback in case of any exception and return the error message
                        transaction.Rollback();
                        return new ResponseModel { Code = 0, Result = "Failed: " + ex.Message };
                    }
                }
            }
        }


        public ResponseModel DeleteUserInfoFromDevice(int enrollId, string deviceSn)
        {
            lock (_lockObject)  // 确保同一时间只有一个线程访问
            {
                int backupNum = 13;
                string message = "{\"cmd\":\"deleteuser\",\"enrollid\":" + enrollId + ",\"backupnum\":" + backupNum + "}";
                DateTime dt = DateTime.Now;
                Machine_command machineCommand = new Machine_command
                {
                    Content = message,
                    Name = "deleteuser",
                    Status = 0,
                    Send_status = 0,
                    Err_count = 0,
                    Serial = deviceSn,
                    Gmt_crate = dt,
                    Gmt_modified = dt,
                };

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.machine_command.Add(machineCommand);
                        int rowsAffected = _db.SaveChanges();

                        if (rowsAffected > 0)
                        {
                            // 根据业务需求选择合适的删除方法
                            DeletePersonByEnrollId(enrollId);

                            transaction.Commit();
                            return new ResponseModel { Code = 200, Result = "Delete success" };
                        }
                        else
                        {
                            transaction.Rollback();
                            return new ResponseModel { Code = 0, Result = "Add fail" };
                        }
                    }
                    catch (Exception ex)
                    {
                        return new ResponseModel { Code = 0, Result = "fail：" + ex.Message };
                    }
                }
            }
        }

        public void DeletePersonByEnrollId(int id)
        {
            lock (_lockObject)  // 确保同一时间只有一个线程访问
            {
                _db.Database.ExecuteSqlRaw("DELETE FROM Person WHERE id = @id", new SqlParameter("@id", id));
                _db.Database.ExecuteSqlRaw("DELETE FROM Enrollinfo WHERE enroll_id = @id", new SqlParameter("@id", id));
            }
        }
       
            public async Task<ResponseModel> getUserInfo(string deviceSn)
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // 异步等待
                                              // 获取设备列表
                var response = await GetPersonallList();
            if (response == null || response.Data == null)
            {
                return new ResponseModel { Code = 0, Result = "null" };
            }
            var personList = response.Data as List<Person>;
            var now = DateTime.Now;
            foreach (var person in personList)
            {
                var enrollId = person.Id;
                var enrollResponse =await enrollinfoService.SelectEnrollallByIdAsync(enrollId);
                if ((enrollResponse != null) && (enrollResponse.Data != null))
                {
                    var enrollInfos = enrollResponse.Data as List<Enrollinfo>;
                    for (int j = 0; j < enrollInfos.Count; j++)
                    {
                            if (enrollInfos[j].Enroll_id != null && enrollInfos[j].Backupnum != null)
                            {
                                using (var transaction = _db.Database.BeginTransaction())
                                {

                                    var message = $"{{\"cmd\":\"getuserinfo\",\"enrollid\":{enrollInfos[j].Enroll_id},\"backupnum\":{enrollInfos[j].Backupnum}}}";
                                    var machineCommand = new Machine_command
                                    {
                                        Content = message,
                                        Name = "getuserinfo",
                                        Status = 0,
                                        Send_status = 0,
                                        Err_count = 0,
                                        Serial = deviceSn,
                                        Gmt_crate = now,
                                        Gmt_modified = now
                                    };
                                    try
                                    {
                                        _db.machine_command.Add(machineCommand);
                                        var rowsAffected = _db.SaveChanges();
                                        if (rowsAffected > 0)
                                        {
                                            transaction.Commit();
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // 记录异常信息
                                        Console.WriteLine($"Error: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
            semaphore.Release();
            }
        
            return new ResponseModel { Code = 200, Result = "Add success" };
        }

        public async void setUserToDevice2(string deviceSn)
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // 异步等待
                ResponseModel pm = await GetPersonallList();
                List<UserInfo> userInfos = await enrollinfoService.usersToSendDevice(pm);
                DateTime dt = DateTime.Now;
                for (int i = 0; i < userInfos.Count(); i++)
                {
                    long? enrollId = userInfos[i].EnrollId;
                    string name = userInfos[i].Name;
                    int? backupnum = userInfos[i].Backupnum;
                    int? admin = userInfos[i].Admin;
                    string? record = userInfos[i].Record;
                    Machine_command machineCommand = new Machine_command
                    {
                        Name = "setuserinfo",
                        Status = 0,
                        Send_status = 0,
                        Err_count = 0,
                        Serial = deviceSn,
                        Gmt_crate = dt,
                        Gmt_modified = dt
                    };
                    machineCommand.Content = "{\"cmd\":\"setuserinfo\",\"enrollid\":" + enrollId + ",\"name\":\"" + name + "\",\"backupnum\":" + backupnum
                            + ",\"admin\":" + admin + ",\"record\":\"" + record + "\"}";
                    if (backupnum == 11 || backupnum == 10)
                    {
                        machineCommand.Content = "{\"cmd\":\"setuserinfo\",\"enrollid\":" + enrollId + ",\"name\":\"" + name + "\",\"backupnum\":" + backupnum
                                + ",\"admin\":" + admin + ",\"record\":" + record + "}";
                    }

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            _db.machine_command.Add(machineCommand);
                            int rowsAffected = _db.SaveChanges();
                          
                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                semaphore.Release();
            }
        }

        public async void setSpecificUserToDevice2(string deviceSn , string userIds)
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // 异步等待
                ResponseModel pm = await GetSpecificPersonallList(userIds);
                List<UserInfo> userInfos = await enrollinfoService.usersToSendDevice(pm);
                DateTime dt = DateTime.Now;
                for (int i = 0; i < userInfos.Count(); i++)
                {
                    long? enrollId = userInfos[i].EnrollId;
                    string name = userInfos[i].Name;
                    int? backupnum = userInfos[i].Backupnum;
                    int? admin = userInfos[i].Admin;
                    string? record = userInfos[i].Record;
                    Machine_command machineCommand = new Machine_command
                    {
                        Name = "setuserinfo",
                        Status = 0,
                        Send_status = 0,
                        Err_count = 0,
                        Serial = deviceSn,
                        Gmt_crate = dt,
                        Gmt_modified = dt
                    };
                    machineCommand.Content = "{\"cmd\":\"setuserinfo\",\"enrollid\":" + enrollId + ",\"name\":\"" + name + "\",\"backupnum\":" + backupnum
                            + ",\"admin\":" + admin + ",\"record\":\"" + record + "\"}";
                    if (backupnum == 11 || backupnum == 10)
                    {
                        machineCommand.Content = "{\"cmd\":\"setuserinfo\",\"enrollid\":" + enrollId + ",\"name\":\"" + name + "\",\"backupnum\":" + backupnum
                                + ",\"admin\":" + admin + ",\"record\":" + record + "}";
                    }

                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            _db.machine_command.Add(machineCommand);
                            int rowsAffected = _db.SaveChanges();
                          
                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                semaphore.Release();
            }
        }

        public async Task SetRemoteUserToDevice2(string deviceSn, string EnrolId, string Name)
        {
            // Check if the selected users list is null or empty
            if (string.IsNullOrEmpty(EnrolId) || string.IsNullOrEmpty(Name))
            {
                // Handle the case where no users are selected or invalid parameters are provided
                // Log the issue or return an error if necessary
                return;  // You can return a specific error or log the issue if required
            }

            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // Asynchronous wait
                DateTime dt = DateTime.Now;

                try
                {
                    // Create the machine command for this user and device
                    Machine_command machineCommand = new Machine_command
                    {
                        Name = "adduser",
                        Status = 0,
                        Send_status = 0,
                        Err_count = 0,
                        Serial = deviceSn,  // Device serial number
                        Gmt_crate = dt,
                        Gmt_modified = dt,
                        Content = $"{{\"cmd\": \"adduser\", \"enrollid\": \"{EnrolId}\", \"name\": \"{Name}\", \"backupnum\": 50, \"admin\": 0}}"
                    };
                   
                    // Using a transaction to save the machine command
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            _db.machine_command.Add(machineCommand);
                            int rowsAffected = _db.SaveChanges();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    // Optionally return a response or log that all commands were processed successfully
                }
                catch (Exception ex)
                {
                    // Log or handle the exception here (you can log the overall error or re-throw)
                    // e.g., _logger.LogError(ex, "Unexpected error occurred while processing users for device {deviceSn}");
                }
                finally
                {
                    semaphore.Release();  // Always release the semaphore when done
                }
            }
        }




        public async Task<bool> SetRemoteUserToDevice(string deviceSn, string EnrolId, string Name)
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // Asynchronous wait
                DateTime dt = DateTime.Now;

                try
                {
                    // Create the machine command for this user and device
                    Machine_command machineCommand = new Machine_command
                    {
                        Name = "adduser",
                        Status = 0,
                        Send_status = 0,
                        Err_count = 0,
                        Serial = deviceSn,  // Device serial number
                        Gmt_crate = dt,
                        Gmt_modified = dt,
                        Content = $"{{\"cmd\": \"adduser\", \"cancel\": true}}"
                    };
                    using (var transaction = _db.Database.BeginTransaction())
                    {
                        try
                        {
                            _db.machine_command.Add(machineCommand);
                            int rowsAffected = _db.SaveChanges();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                            }
                            else
                            {
                                transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    
                   // Ensure changes are saved

                    return true;  // Success
                }
                catch (Exception ex)
                {
                    // Log the error (optional)
                    Console.WriteLine($"Error in SetRemoteUserToDevice: {ex.Message}");
                    return false;  // Error occurred
                }
                finally
                {
                    semaphore.Release(); // Release the semaphore when done
                }
            }
        }




        // }
        public async Task<ResponseModel> UpdateByPrimaryKey(Person person)
        {
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync();  // 异步等待
                string sql = @"
            UPDATE person
            SET
              name = @name,
              roll_id = @roll_id
            WHERE id = @id";
                var parameters = new List<SqlParameter>
            {
               new SqlParameter("@roll_id", person.Roll_id),
               new SqlParameter("@name",person.Name),
              new SqlParameter("@id", person.Id)
            };
                // 执行更新操作
                int i = _db.Database.ExecuteSqlRaw(sql, parameters.ToArray());
                semaphore.Release();
                if (i > 0)
                    return new ResponseModel { Code = 200, Result = "person update success" };

                return new ResponseModel { Code = 0, Result = "person update fail" };
            }
        }
    }
}
