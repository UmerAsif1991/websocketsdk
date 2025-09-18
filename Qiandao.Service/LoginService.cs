using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Qiandao.Model.Response;
using Qiandao.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qiandao.Service
{
    public class LoginService : IScope
    {
        private readonly HRMDb _db;
        private readonly IMapper _mapper;
        private readonly ILogger<LoginService> _logger;
        public LoginService(HRMDb db, IMapper mapper, ILogger<LoginService> logger)
        {
            _logger = logger;
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public void Dispose()
        { }

        public async Task<ResponseModel> Login(LoginViewModel objUser)
        {
            try
            {
                var user = await _db.Users
                    .Include(u => u.Role)
                    .Where(u => u.username == objUser.username && u.password == objUser.password).FirstOrDefaultAsync();

                if (user == null || user.password != objUser.password) 
                {
                    return new ResponseModel
                    {
                        Code = 400,
                        Result = "Invalid username or password"
                    };
                }

                return new ResponseModel
                {
                    Code = 200,
                    Result = "Login successful",
                    Data = new
                    {
                        user.UserId,
                        user.username,
                        Role = user.Role?.Name,
                        user.RoleId,
                        user.Name,
                        user.TenantId
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    Code = 500,
                    Result = $"Error: {ex.Message}"
                };
            }
        }
    }
}
