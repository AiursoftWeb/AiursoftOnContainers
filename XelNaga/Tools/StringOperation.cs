﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Aiursoft.XelNaga.Tools
{
    public static class StringOperation
    {
        public static string BytesToBase64(this byte[] input)
        {
            return Convert.ToBase64String(input);
        }

        public static byte[] Base64ToBytes(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new byte[0];
            }
            return Convert.FromBase64String(input);
        }

        public static byte[] StringToBytes(this string input)
        {
            return new UTF8Encoding().GetBytes(input);
        }

        public static string BytesToString(this byte[] input)
        {
            return new UTF8Encoding().GetString(input, 0, input.Length);
        }

        public static string StringToBase64(this string input)
        {
            return BytesToBase64(StringToBytes(input));
        }

        public static string Base64ToString(this string input)
        {
            return BytesToString(Base64ToBytes(input));
        }

        public static byte[] ToUTF8WithDom(this string content)
        {
            byte[] encoded = new System.Text.UTF8Encoding().GetBytes(content);
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var all = new byte[bom.Length + encoded.Length];
            Array.Copy(bom, all, bom.Length);
            Array.Copy(encoded, 0, all, bom.Length, encoded.Length);
            return all;
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            foreach (var c in data)
            {
                sBuilder.Append(c.ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string GetMD5(this string sourceString)
        {
            string hash = GetMd5Hash(MD5.Create(), sourceString);
            return hash;
        }

        public static string GetMD5(this byte[] data)
        {
            using MD5 md5 = MD5.Create();
            var hash = md5.ComputeHash(data);
            string hex = BitConverter.ToString(hash);
            return hex.Replace("-", "");
        }

        public static string OTake(this string source, int count)
        {
            if (source.Length <= count)
            {
                return source;
            }
            else
            {
                return source.Substring(0, count - 3) + "...";
            }
        }

        public static bool IsInFollowingExtension(this string filename, params string[] extensions)
        {
            var ext = Path.GetExtension(filename);
            foreach (var extension in extensions)
            {
                if (ext.Trim('.').ToLower() == extension)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsImageMedia(this string filename)
        {
            return filename.IsInFollowingExtension("jpg", "png", "bmp", "jpeg", "gif", "svg", "ico");
        }

        public static bool IsStaticImage(this string filename)
        {
            return filename.IsInFollowingExtension("jpg", "png", "bmp", "jpeg");
        }

        public static bool IsVideo(this string filename)
        {
            return filename.IsInFollowingExtension("mp4", "webm", "ogg");
        }

        public static string ORemoveHTML(this string content)
        {
            string s = string.Empty;
            content = WebUtility.HtmlDecode(content);
            if (!content.Contains(">"))
            {
                return content;
            }
            while (content.Contains(">"))
            {
                s += content.Substring(0, content.IndexOf("<"));
                content = content.Substring(content.IndexOf(">") + 1);
            }
            return s + content;
        }

        private static Random _staticRan { get; set; } = new Random();
        public static string RandomString(int count)
        {
            int number;
            string checkCode = string.Empty;
            var random = new Random(_staticRan.Next());
            for (int i = 0; i < count; i++)
            {
                number = random.Next();
                number %= 36;
                if (number < 10)
                {
                    number += 48;
                }
                else
                {
                    number += 55;
                }
                checkCode += ((char)number).ToString();
            }
            return checkCode;
        }

        public static string EncodePath(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            return input.ToUrlEncoded().Replace("%2F", "/");
        }

        public static string ToUrlEncoded(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            return Uri.EscapeDataString(input);
        }

        public static string AppendPath(this string root, string folder)
        {
            return root == null ? folder : root + "/" + folder;
        }

        public static string DetachPath(this string path)
        {
            if (path == null || !path.Contains("/"))
            {
                return null;
            }
            return path.Replace("/" + path.Split('/').Last(), "");
        }

        public static IEnumerable<string> SplitInParts(this string input, int partLength)
        {
            if (input == null)
            {
                throw new ArgumentNullException("s");
            }
            if (partLength <= 0)
            {
                throw new ArgumentException("Part length has to be positive.", "partLength");
            }
            for (var i = 0; i < input.Length; i += partLength)
            {
                yield return input.Substring(i, Math.Min(partLength, input.Length - i));
            }
        }

        public static string HumanReadableSize(this long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return string.Format("{0:0.##} {1}", size, sizes[order]);
        }
    }
}
