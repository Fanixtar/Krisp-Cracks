using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers
{
	public static class CryptoHelper
	{
		public static string ComputeSha256(string input, int bytesToHex = 32)
		{
			string text;
			using (SHA256 sha = SHA256.Create())
			{
				text = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)).Take(bytesToHex).ToArray<byte>()).Replace("-", string.Empty);
			}
			return text;
		}
	}
}
