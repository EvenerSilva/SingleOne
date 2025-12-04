using System;
using System.Linq;

namespace SingleOneIntegrator.Helpers
{
    /// <summary>
    /// Validador de CPF
    /// </summary>
    public static class CpfValidator
    {
        /// <summary>
        /// Valida um CPF
        /// </summary>
        /// <param name="cpf">CPF (apenas números)</param>
        /// <returns>True se válido, False caso contrário</returns>
        public static bool IsValid(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Deve ter exatamente 11 dígitos
            if (cpf.Length != 11)
                return false;

            // CPFs inválidos conhecidos (todos dígitos iguais)
            if (cpf == "00000000000" || cpf == "11111111111" || cpf == "22222222222" ||
                cpf == "33333333333" || cpf == "44444444444" || cpf == "55555555555" ||
                cpf == "66666666666" || cpf == "77777777777" || cpf == "88888888888" ||
                cpf == "99999999999")
                return false;

            // Validação dos dígitos verificadores
            var multiplicador1 = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new int[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpf.Substring(0, 9);
            var soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            var resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            var digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        /// <summary>
        /// Sanitiza CPF removendo caracteres não numéricos
        /// </summary>
        /// <param name="cpf">CPF</param>
        /// <returns>CPF apenas com números</returns>
        public static string Sanitize(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            return new string(cpf.Where(char.IsDigit).ToArray());
        }
    }
}


