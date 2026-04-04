using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class SystemEnum
    {
        public const string IDs = "IDs";
        public const string SPParameter = "Parameter";

        public const string JWT_Issuer = "JwtSettings:Issuer";
        public const string JWT_SignKey = "JwtSettings:SignKey";
        public const string JWT_MaxAgeMinutes = "JwtSettings:MaxAgeMinutes";



        /// <summary>
        /// 真實登入系統的LoginID
        /// </summary>
        public const string JWT_Claim_Actor = ClaimTypes.Actor;
        public const string JWT_Claim_Jti = "Jti";

        /// <summary>
        /// 授予的權限
        /// </summary>
        public const string JWT_Claim_Role = ClaimTypes.Role;

        /// <summary>
        /// 被代理人/執行權限的人
        /// </summary>
        public const string JWT_Claim_Name = ClaimTypes.Name;


        public const string DES_SK = "TDesSetting:SK";
        public const string DES_SIV = "TDesSetting:SIV";

        public const string AES_SK = "TAESSetting:SK";
        public const string AES_SIV = "TAESSetting:SIV";
    }
}
