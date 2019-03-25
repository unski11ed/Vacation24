using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Vacation24.Core.ExtensionMethods
{
    public static class MyExtensions
    {
        public static string CalculateStringHash(this HashAlgorithm hash, string input)
        {
            var encoder = new System.Text.ASCIIEncoding();
            byte[] combined = encoder.GetBytes(input);
            var hashed = hash.ComputeHash(combined);
            return Convert.ToBase64String(hashed);
        }

        public static string CalculateMD5Hash(this string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static String ToBase64String(this String source)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(source));
        }

        public static bool IsJson(this string input)
        {
            input = input.Trim();
            return input.StartsWith("{") && input.EndsWith("}")
                   || input.StartsWith("[") && input.EndsWith("]");
        }

        public static Dictionary<string, object> ToObjectDictionary(this Dictionary<string, string> input)
        {
            var output = new Dictionary<string, object>();

            foreach (var e in input)
            {
                dynamic result = new Object();

                decimal decResult;
                if(decimal.TryParse(e.Value, out decResult)){
                    if(e.Value.Contains(',') || e.Value.Contains('.'))
                        result = decResult;
                    else
                        result = (int)decResult;
                }else{
                    result = e.Value;
                }
                output.Add(e.Key, (object)result);
            }

            return output;
        }

        public static void SetBasicAuthentication(this HttpWebRequest request, string user, string password)
        {
            string authInfo = user + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
        }

        public static HttpWebResponse GetResponseNoException(this HttpWebRequest req)
        {
            try
            {
                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
        }

        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}