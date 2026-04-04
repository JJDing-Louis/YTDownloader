using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Tools
{
    public class SecurityTool
    {
        public static string Audience = Guid.NewGuid().ToString();

        public static string DES_Decrypt(string Base64_encText, string key, string iv)
        {
            var Data = Convert.FromBase64String(Base64_encText);
            var Key = Encoding.ASCII.GetBytes(key);
            var IV = Encoding.ASCII.GetBytes(iv);
            var DESalg = DES.Create();
            var msDecrypt = new MemoryStream(Data);
            var csDecrypt = new CryptoStream(msDecrypt,
                DESalg.CreateDecryptor(Key, IV),
                CryptoStreamMode.Read);

            // Create buffer to hold the decrypted data.
            var fromEncrypt = new byte[Data.Length];

            // Read the decrypted data out of the crypto stream
            // and place it into the temporary buffer.
            var count = csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
            var str = Encoding.UTF8.GetString(fromEncrypt, 0, count);
            return str;
        }


        public static byte[] ConvertHexToByte(string hex)
        {
            return Enumerable
                .Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }


        /// <summary>
        /// https://www.ez2o.com/App/Coder/AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="cipherMode"></param>
        /// <param name="paddingMode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string EncryptAES256(string plainText, string key, string iv, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            var P = ParameterTool.GetConfiguration();
            if (key == null)
                key = P[SystemEnum.AES_SK];
            if (iv == null)
                iv = P[SystemEnum.AES_SIV];

            if (string.IsNullOrEmpty(key) || key.Length != 32)
                throw new ArgumentException("Key must be 32 characters (256 bits). current length=" + key?.Length, nameof(key));
            if (string.IsNullOrEmpty(iv) || iv.Length != 16)
                throw new ArgumentException("IV must be 16 characters (128 bits).. current length=" + iv?.Length, nameof(iv));

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = cipherMode;
                aes.Padding = paddingMode;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }


        /// <summary>
        /// https://www.ez2o.com/App/Coder/AES
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="cipherMode"></param>
        /// <param name="paddingMode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string DecryptAES256(string cipherText, string key, string iv, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
            var P = ParameterTool.GetConfiguration();
            if (key == null)
                key = P[SystemEnum.AES_SK];
            if (iv == null)
                iv = P[SystemEnum.AES_SIV];
            if (string.IsNullOrEmpty(key) || key.Length != 32)
                throw new ArgumentException("Key must be 32 characters (256 bits). current length=" + key?.Length, nameof(key));
            if (string.IsNullOrEmpty(iv) || iv.Length != 16)
                throw new ArgumentException("IV must be 16 characters (128 bits).. current length=" + iv?.Length, nameof(iv));


            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = cipherMode;
                aes.Padding = paddingMode;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipherText);
                        cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        return Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
            }
        }
        public static string EncryptJSON(string key, JObject json)
        {
            var md5 = MD5(key);
            return Encrypt(md5, json.ToString());
        }

        public static JObject DecryptJSON(string key, string encodeJsonStr)
        {
            var md5 = MD5(key);
            encodeJsonStr = Decrypt(md5, encodeJsonStr);
            return JObject.Parse(encodeJsonStr);
        }

        /// <summary>
        /// AES
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="rawData"></param>
        /// <param name="cipherMode"></param>
        /// <param name="paddingMode"></param>
        /// <returns></returns>
        [Obsolete("使用EncryptAES256")]
        public static string Encrypt(string strKey, string rawData, CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            var sourceBytes = Encoding.UTF8.GetBytes(rawData);
            var byte_pwdMD5 = Encoding.UTF8.GetBytes(strKey);


            var rDel = new RijndaelManaged();
            rDel.Key = byte_pwdMD5;
            rDel.Mode = cipherMode;
            rDel.Padding = PaddingMode.PKCS7;
            var cTransform = rDel.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="encryptData"></param>
        /// <param name="cipherMode"></param>
        /// <param name="paddingMode"></param>
        /// <returns></returns>
        ///  [Obsolete("使用DecryptAES256")]
        public static string Decrypt(string strKey, string encryptData, CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            var keyArray = Encoding.UTF8.GetBytes(strKey);
            var toEncryptArray = Convert.FromBase64String(encryptData);


            var rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = cipherMode;
            rDel.Padding = paddingMode;

            var cTransform = rDel.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        ///     SHA256
        ///     <para>SQL:select HASHBYTES('SHA2_256',data)</para>
        /// </summary>
        /// <param name="data"></param>
        /// <returns>回傳sha256雜湊</returns>
        public static string SHA256(string data)
        {
            using (var algo = System.Security.Cryptography.SHA256.Create())
            {
                var byteArray = Encoding.UTF8.GetBytes(data);
                var m = new MemoryStream(byteArray);

                var hashValue = algo.ComputeHash(m);

                var str = BitConverter.ToString(hashValue).Replace("-", "");
                return str;
            }
        }

        public static string MD5(string input)
        {
            using (var md5Hash = System.Security.Cryptography.MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sBuilder = new StringBuilder();

                for (var i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));

                return sBuilder.ToString();
            }
        }

        public static bool JWT_ValidateToken(string authToken, bool validateAudience = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = JWT_GetValidationParameters(true, validateAudience, true);
            try
            {
                var principal = tokenHandler.ValidateToken(authToken, validationParameters, out _);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static TokenValidationParameters JWT_GetValidationParameters(bool validateLifetime = true,
            bool validateAudience = true, bool validateIssuer = true)
        {
            var config = ParameterTool.GetConfiguration();
            var issuer = config.GetValue<string>(SystemEnum.JWT_Issuer);
            var signKey = config.GetValue<string>(SystemEnum.JWT_SignKey);
            return new TokenValidationParameters
            {
                ValidateLifetime = validateLifetime, // Because there is no expiration in the generated token
                ValidateAudience = validateAudience, // Because there is no audience in the generated token
                ValidateIssuer = validateIssuer, // Because there is no issuer in the generated token
                ValidIssuer = issuer,
                ValidAudience = Audience,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signKey)) // The same key as the one that generate the token
            };
        }

        public static string JWT_CreateToken(string user, List<string> roles)
        {
            if (roles == null)
                roles = new();

            return JWT_CreateToken(user, roles.ToArray());
        }

        /// <summary>
        /// 登入者與代理人是同一人時
        /// </summary>
        /// <param name="user">登入人,代理人</param>
        /// <param name="roles">授權角色</param>
        /// <returns></returns>
        public static string JWT_CreateToken(string user, params string[] roles) => JWT_CreateToken(user, user, roles?.ToList());


        /// <summary>
        /// 登入者與代理人是不同時
        /// </summary>
        /// <param name="login">登入者</param>
        /// <param name="user">代理人</param>
        /// <param name="roles">代理角色</param>
        /// <returns></returns>
        public static string JWT_CreateToken(string login, string user, List<string> roles)
        {

            var configuration = ParameterTool.GetConfiguration();
            var issuer = configuration.GetValue<string>(SystemEnum.JWT_Issuer);
            var signKey = configuration.GetValue<string>(SystemEnum.JWT_SignKey);
            var num = configuration.GetValue<int>(SystemEnum.JWT_MaxAgeMinutes, 480);
            var claimList = new List<Claim>()
        {
            new Claim( SystemEnum.JWT_Claim_Actor,login),
            new Claim(SystemEnum.JWT_Claim_Name, user),
            new Claim(SystemEnum.JWT_Claim_Jti,new Guid().ToString())
        };
            var _roles = roles.Select(r => new Claim(SystemEnum.JWT_Claim_Role, r));
            claimList.AddRange(_roles);

            var claimsIdentity = new ClaimsIdentity(claimList);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey)), "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = issuer,
                Audience = Audience,
                Subject = claimsIdentity,
                Expires = new DateTime?(DateTime.Now.AddMinutes((double)num)),
                SigningCredentials = signingCredentials
            };
            var securityTokenHandler = new JwtSecurityTokenHandler();

            var token = securityTokenHandler.CreateToken(tokenDescriptor);
            var tokenString = securityTokenHandler.WriteToken(token);
            return tokenString;

        }

        public static List<Claim> JWT_Claims(string tokenString)
        {
            if (string.IsNullOrEmpty(tokenString))
                return new();
            var securityTokenHandler = new JwtSecurityTokenHandler();
            var token = securityTokenHandler.ReadJwtToken(tokenString);


            return token.Claims.ToList();

        }
        public static List<Claim> GetWebClaims()
        {
            // 1. 取得 ApplicationContext Type
            var appContextType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == "TD1.WebSupport.ServiceManagement.ApplicationContext");
            if (appContextType == null)
                throw new InvalidOperationException("找不到 ApplicationContext 類型");

            // 2. 取得 ApplicationContext.Container 靜態屬性
            var containerProperty = appContextType.GetProperty("Container", BindingFlags.Public | BindingFlags.Static);
            if (containerProperty == null)
                throw new InvalidOperationException("找不到 Container 屬性");
            var container = containerProperty.GetValue(null);

            // 3. 取得 Autofac.ResolutionExtensions 類型
            var resolutionExtensionsType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == "Autofac.ResolutionExtensions");
            if (resolutionExtensionsType == null)
                throw new InvalidOperationException("找不到 Autofac.ResolutionExtensions 類型");

            // 4. 取得 Resolve(this IComponentContext, Type) 方法 (靜態擴充方法)
            var resolveMethod = resolutionExtensionsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == "Resolve" &&
                    m.GetParameters().Length == 2 &&
                    m.GetParameters()[0].ParameterType.Name.Contains("IComponentContext") && // 確保第一參數是容器介面
                    m.GetParameters()[1].ParameterType == typeof(Type)
                );
            if (resolveMethod == null)
                throw new InvalidOperationException("找不到 Resolve 方法");

            // 5. 呼叫 Resolve(container, typeof(IHttpContextAccessor))
            // 取得 IHttpContextAccessor Type
            var accessorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "IHttpContextAccessor");
            if (accessorType == null)
                throw new InvalidOperationException("找不到 IHttpContextAccessor 類型");

            var accessorInstance = resolveMethod.Invoke(null, new object[] { container, accessorType });
            if (accessorInstance == null)
                throw new InvalidOperationException("Resolve 取得 IHttpContextAccessor 為 null");

            // 6. 取得 HttpContext 屬性
            var httpContextProp = accessorType.GetProperty("HttpContext");
            var httpContext = httpContextProp.GetValue(accessorInstance);
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext 為 null");

            // 7. 取得 User 屬性
            var userProp = httpContext.GetType().GetProperty("User");
            var user = userProp.GetValue(httpContext);
            if (user == null)
                throw new InvalidOperationException("User 為 null");

            // 8. 取得 Claims 屬性
            var claimsProp = user.GetType().GetProperty("Claims");
            var claimsEnumerable = claimsProp.GetValue(user);

            // 9. 轉成 List<T>
            var claimType = claimsEnumerable.GetType().GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>))
                .GetGenericArguments()[0];

            var toListMethod = typeof(System.Linq.Enumerable)
                .GetMethod("ToList")
                .MakeGenericMethod(claimType);

            var claimsList = toListMethod.Invoke(null, new object[] { claimsEnumerable });

            return claimsList as List<Claim>;
        }

        public static List<Claim> GetCurrentClaims()
        {
            try
            {
                var x = Thread.CurrentPrincipal as ClaimsPrincipal;
                var claims = x.Claims.ToList();
                return claims;
            }
            catch (Exception)
            {
            }


            try
            {
                var claims = ClaimsPrincipal.Current?.Claims.ToList();
                return claims;
            }
            catch (Exception)
            {
            }
            try
            {
                var claims = GetWebClaims();
                return claims;
            }
            catch (Exception)
            {
            }




            return new();
        }

        /// <summary>
        /// 登入人員Login ID
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public static string JWT_GetLoginID(string tokenString)
        {
            var claims = JWT_Claims(tokenString);
            var loginId = GetLoginID(claims);

            return loginId;
        }

        /// <summary>
        /// 登入人員Login ID
        /// </summary>
        /// <returns></returns>
        public static string GetLoginID()
        {
            var claims = GetCurrentClaims();
            return GetLoginID(claims);
        }

        /// <summary>
        /// 登入人員Login ID
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static string GetLoginID(List<Claim> claims)
        {
            if (claims == null)
                return null;

            string friend = null;
            var mapping = JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap;
            mapping.TryGetValue(SystemEnum.JWT_Claim_Actor, out friend);

            var loginId = claims
                .Where(c =>
                {
                    if (c.Type is SystemEnum.JWT_Claim_Actor)
                        return true;
                    if (friend is not null)
                    {
                        if (c.Type == friend)
                            return true;
                    }
                    return false;
                }).FirstOrDefault()?.Value;



            if (string.IsNullOrEmpty(loginId))
                loginId = GetUserID(claims);

            return loginId;

        }

        /// <summary>
        /// 1. 執行使用的權限User ID
        /// 2. 被代理人UserID
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public static string JWT_GetUserID(string tokenString)
        {
            var claims = JWT_Claims(tokenString);
            return GetUserID(claims);
        }

        /// <summary>
        /// 1. 執行使用的權限User ID
        /// 2. 被代理人UserID
        /// </summary>
        /// <returns></returns>
        public static string GetUserID()
        {
            var claims = GetCurrentClaims();
            return GetUserID(claims);
        }

        /// <summary>
        /// 1. 執行使用的權限User ID
        /// 2. 被代理人UserID
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static string GetUserID(List<Claim> claims)
        {

            if (claims == null)
                return null;

            string friend = null;
            var mapping = JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap;
            mapping.TryGetValue(SystemEnum.JWT_Claim_Name, out friend);



            var userId = claims
                .Where(c =>
                {
                    if (c.Type is SystemEnum.JWT_Claim_Name)
                        return true;
                    if (friend is not null)
                    {
                        if (c.Type == friend)
                            return true;
                    }
                    if (friend is "userId")
                        return true;

                    return false;
                }).FirstOrDefault()?.Value;

            return userId;

        }

        /// <summary>
        /// 1. 執行使用的權限角色清單
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public static List<string> JWT_GetUserRoles(string tokenString)
        {
            if (string.IsNullOrEmpty(tokenString))
                return new();
            var claims = JWT_Claims(tokenString);
            return GetUserRoles(claims);

        }

        public static Dictionary<int, string> GetRoleMapping()
        {
            using (var c = ConnectionTool.GetConnection())
            {
                var dict = c.Query<dynamic>("select distinct Role_ID, Name from TRole")
                    .ToDictionary(a => (int)a.Role_ID, a => (string)a.Name);
                return dict;
            }
        }

        public static List<string> GetUserRoleNames(string tokenString)
        {
            if (string.IsNullOrEmpty(tokenString))
                return new();
            var claims = JWT_Claims(tokenString);
            return GetUserRoleNames(claims);

        }

        public static List<string> GetUserRoleNames()
        {


            var claims = GetCurrentClaims();
            return GetUserRoleNames(claims);
        }

        public static List<string> GetUserRoleNames(List<Claim> claims)
        {
            var ret = new List<string>();
            if (claims == null)
                return new();

            var m = GetRoleMapping();



            var list = GetUserRoles(claims);
            var intList = list
                .Select(s => {
                    bool success = int.TryParse(s, out int result);
                    return new { success, result };
                })
                .Where(x => x.success)
                .Select(x => x.result)
                .ToList();

            var allAreNumbers = list.Count == intList.Count;
            if (allAreNumbers)
            {
                foreach (var i in intList)
                {
                    if (m.ContainsKey(i))
                        ret.Add(m[i]);
                }
            }
            else
            {
                return list;
            }





            return ret;
        }

        public static List<int> GetUserRoleIds()
        {
            var claims = GetCurrentClaims();
            return GetUserRoleIds(claims);
        }
        public static List<int> GetUserRoleIds(List<Claim> claims)
        {
            if (claims == null)
                return new();
            var ret = new List<int>();


            var m = GetRoleMapping();

            var list = GetUserRoles(claims);
            var intList = list
                .Select(s => {
                    bool success = int.TryParse(s, out int result);
                    return new { success, result };
                })
                .Where(x => x.success)
                .Select(x => x.result)
                .ToList();

            var allAreNumbers = list.Count == intList.Count;
            if (allAreNumbers)
            {
                foreach (var i in intList)
                {
                    if (m.ContainsKey(i))
                        ret.Add(i);
                }
            }
            else
            {
                foreach (var t in list)
                {
                    foreach (var p in m)
                    {
                        if (p.Value == t)
                        {
                            ret.Add(p.Key);
                            break;
                        }
                    }

                }
            }





            return ret;
        }
        [Obsolete]
        public static List<string> GetUserRoles()
        {
            var claims = GetCurrentClaims();
            return GetUserRoles(claims);
        }
        public static List<string> GetUserRoles(List<Claim> claims)
        {
            if (claims == null)
                return new();
            string friend = null;
            var mapping = JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap;
            mapping.TryGetValue(SystemEnum.JWT_Claim_Role, out friend);

            var roles = claims
                .Where(c =>
                {
                    if (c.Type is SystemEnum.JWT_Claim_Role)
                        return true;
                    if (friend is not null)
                    {
                        if (c.Type == friend)
                            return true;
                    }

                    if (c.Type is "role" or "roles")
                        return true;
                    return false;
                })
                .Select(c => c.Value)
                .ToList();

            return roles;

        }

        /// <summary>
        ///     加密字串
        ///     3DES
        /// </summary>
        /// <param name="content"></param>
        /// <param name="k"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        [Obsolete("Please use AES Encrypt Function")]
        public static string SensitiveStringEncode(string content)
        {
            try
            {
                var P = ParameterTool.GetConfiguration();
                var k = P[SystemEnum.DES_SK];
                var i = P[SystemEnum.DES_SIV];
                var strTDESEncrypt = TDESEncrypt(Encoding.Default.GetBytes(k),
                    Encoding.Default.GetBytes(i),
                    CipherMode.CBC,
                    PaddingMode.PKCS7,
                    Encoding.UTF8.GetBytes(content));
                return StringToBase64(strTDESEncrypt, Encoding.UTF8);
            }
            catch
            {
                return StringToBase64("", Encoding.UTF8);
            }
        }

        /// <summary>
        ///  解密字串
        ///  3DES
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string SensitiveStringDecode(string content, string key = null, string iv = null)
        {
            try
            {
                var P = ParameterTool.GetConfiguration();
                if (string.IsNullOrEmpty(key))
                    key = P[SystemEnum.DES_SK];
                if (string.IsNullOrEmpty(iv))
                    iv = P[SystemEnum.DES_SIV];
                var strTDESDecrypt = TDESDecrypt(Encoding.Default.GetBytes(key),
                    Encoding.Default.GetBytes(iv),
                    CipherMode.CBC,
                    PaddingMode.PKCS7,
                    HexStringToBytesArray(Base64ToString(content, Encoding.UTF8)));
                return Encoding.UTF8.GetString(HexStringToBytesArray(strTDESDecrypt));
            }
            catch
            {
                return StringToBase64("", Encoding.UTF8);
            }
        }

        private static byte[] HexStringToBytesArray(string HexString)
        {
            var byteLength = HexString.Length / 2;
            var bytes = new byte[byteLength];
            string hex;
            var j = 0;
            for (var i = 0; i < bytes.Length; i++)
            {
                hex = new string(new[] { HexString[j], HexString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }

            return bytes;
        }

        private static byte HexToByte(string hex)
        {
            var newByte = byte.Parse(hex, NumberStyles.HexNumber);
            return newByte;
        }

        [Obsolete]
        private static string TDESDecrypt(byte[] privatek, byte[] i, CipherMode ciphermode, PaddingMode paddingmode,
            byte[] content)
        {
            var tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Padding = paddingmode;
            tripleDES.Mode = ciphermode;
            tripleDES.Key = privatek;
            tripleDES.IV = i;
            var transform = tripleDES.CreateDecryptor(tripleDES.Key, tripleDES.IV);
            var decryption = transform.TransformFinalBlock(content, 0, content.Length);
            transform.Dispose();
            return BitConverter.ToString(decryption).Replace("-", "");
        }
        [Obsolete]
        private static string TDESEncrypt(byte[] privatek, byte[] i, CipherMode ciphermode, PaddingMode paddingmode,
            byte[] content)
        {
            var tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Padding = paddingmode;
            tripleDES.Mode = ciphermode;
            tripleDES.Key = privatek;
            tripleDES.IV = i;
            var transform = tripleDES.CreateEncryptor();

            var cipherTextBuffer = transform.TransformFinalBlock(content, 0, content.Length);
            transform.Dispose();

            return BitConverter.ToString(cipherTextBuffer).Replace("-", "");
        }

        private static string StringToBase64(string content, Encoding encode)
        {
            var toEncodeAsBytes = encode.GetBytes(content);
            var returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private static string Base64ToString(string content, Encoding encode)
        {
            var data = Convert.FromBase64String(content);
            var returnValue = encode.GetString(data);
            return returnValue;
        }

        public static byte[] DBEncrypt(string data, string salt)
        {
            using (var c = ConnectionTool.GetConnection())
            {
                var r = c
                    .Query<byte[]>("select  ENCRYPTBYPASSPHRASE(@salt,@data)",
                        new { salt, data })
                    .FirstOrDefault();
                return r;
            }
        }
        public static string DBDecrypt(byte[] data, string salt)
        {
            using (var c = ConnectionTool.GetConnection())
            {
                var r = c
                    .Query<string>("select convert(nvarchar, DECRYPTBYPASSPHRASE(@salt,@data))",
                        new { salt, data })
                    .FirstOrDefault();
                return r;
            }
        }

    }
}
