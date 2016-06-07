using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace CLRH100Demo1WebUI.Server
{
    public class MainHub : Hub
    {
        private MD5 MD5 { get; set; } = MD5.Create();

        public void RequestCurrentState()
        {
            this.Clients.All.requestCurrentState();
        }

        public void UpdateHostState(HostState hostState)
        {
            this.Clients.All.updatedHostState(hostState);
        }

        public string RequestOneTimePass()
        {
            return GeneratePassword(DateTime.UtcNow);
        }

        private string GeneratePassword(DateTime time)
        {
            var salt = ConfigurationManager.AppSettings["salt"] ?? "";
            var text = $"{time:yyyyMMddHHmm}|{salt}";
            var hashedBytes = default(byte[]);
            lock (MD5) hashedBytes = MD5.ComputeHash(Encoding.UTF8.GetBytes(text));
            var ntext = BitConverter.ToUInt16(hashedBytes, 0).ToString("D:04");
            return ntext.Substring(ntext.Length - 4);
        }

        public void RequestUnlock(string password)
        {
            var salt = ConfigurationManager.AppSettings["salt"] ?? "";
            var utcNow = DateTime.UtcNow;
            var pivotTime = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0);

            var passwordCandidates = new[] {
                pivotTime.AddMinutes(-1),
                pivotTime,
                pivotTime.AddMinutes(+1)
            }.Select(time => GeneratePassword(time));

            var isAuthorized = passwordCandidates.Any(pass => pass == password);
            this.Clients.All.requestRemoteUnlock(isAuthorized);
        }
    }
}