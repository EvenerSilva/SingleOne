using System.Security.Cryptography;
using System.Text;
using System;
using System.Text.RegularExpressions;

namespace SingleOne.Util
{
    public class Cripto
    {
        private static string myKey = "vIv4?!";
        private static TripleDES des = TripleDES.Create();
		private static MD5 hashmd5 = MD5.Create();

		public static string CriptografarDescriptografar(string valor, bool cripto)
		{
			if (cripto == true)
				return EncodeToBase64(valor);
			else if(IsBase64String(valor)) return DecodeBase64(valor);
            else return valor;
		}

		private static string Criptografar(string valor)
		{
			des.Key = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(myKey));
			des.Mode = CipherMode.ECB;
			ICryptoTransform it = des.CreateEncryptor();
			ASCIIEncoding ai = new ASCIIEncoding();
			byte[] buffer = ASCIIEncoding.ASCII.GetBytes(valor);
			return Convert.ToBase64String(it.TransformFinalBlock(buffer, 0, buffer.Length));
		}

		private static string Descriptografar(string valor)
		{
			des.Key = hashmd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(myKey));
			des.Mode = CipherMode.ECB;
			ICryptoTransform ict = des.CreateDecryptor();
			byte[] buffer = Convert.FromBase64String(valor);
			return ASCIIEncoding.ASCII.GetString(ict.TransformFinalBlock(buffer, 0, buffer.Length));
		}

        public static string EncodeToBase64(string texto)
        {
            try
            {
                byte[] textoAsBytes = Encoding.ASCII.GetBytes(texto);
                string resultado = System.Convert.ToBase64String(textoAsBytes);
                return resultado;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string DecodeBase64(string value)
        {
            var valueBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        public static bool IsBase64String(string base64)
        {
            // Verifica se a string é nula ou vazia
            if (string.IsNullOrEmpty(base64))
            {
                return false;
            }

            // Verifica se a string tem um comprimento múltiplo de 4
            if (base64.Length % 4 != 0)
            {
                return false;
            }

            // Regex para validar caracteres base64
            string base64Pattern = @"^[a-zA-Z0-9\+/]*={0,2}$";
            Regex regex = new Regex(base64Pattern, RegexOptions.None);

            // Verifica se a string corresponde ao padrão base64
            if (!regex.IsMatch(base64))
            {
                return false;
            }

            // Tenta decodificar a string base64
            try
            {
                Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
