using SingleOne.Models;
using System.Collections.Generic;
using System;
using SingleOneAPI.Enumeradores;

namespace SingleOneAPI.Models
{
    public class Contrato
    {
        public int Id { get; set; }
        public int Cliente { get; set; }
        public int Fornecedor { get; set; }
        public int? Numero { get; set; }
        public int? Aditivo { get; set; }
        public string Descricao { get; set; }
        public DateTime DTInicioVigencia { get; set; }
        public DateTime? DTFinalVigencia { get; set; }
        public decimal Valor { get; set; }
        public int Status { get; set; }
        public bool GeraNF { get; set; }
        public DateTime DTCriacao { get; set; }
        public int UsuarioCriacao { get; set; }
        public DateTime? DTExclusao { get; set; }
        public int? UsuarioExclusao { get; set; }
        public bool Renovavel { get; set; }
        public string ArquivoContrato { get; set; }
        public string NomeArquivoOriginal { get; set; }
        public DateTime? DataUploadArquivo { get; set; }
        public int? UsuarioUploadArquivo { get; set; }
        public int? UsuarioRemocaoArquivo { get; set; }
        public DateTime? DataRemocaoArquivo { get; set; }

        public virtual StatusContrato StatusContratoNavigation { get; set; }
        public virtual Cliente ClienteNavigation { get; set; }
        public virtual Fornecedore FornecedorNavigation { get; set; }

        public virtual Usuario UsuarioCriacaoNavigation { get; set; }
        public virtual Usuario UsuarioExclusaoNavigation { get; set; }
        public virtual Usuario UsuarioUploadArquivoNavigation { get; set; }
        public virtual Usuario UsuarioRemocaoArquivoNavigation { get; set; }


        public ICollection<Notasfiscaisiten> Notasfiscaisitens { get; set; }
        public ICollection<Equipamento> Equipamentos { get; set; }

        public static int VerificarStatusContrato(DateTime dataInicioVigencia, DateTime dataFinalVigencia)
        {
            if (dataInicioVigencia > DateTime.UtcNow)
            {
                return (int)StatusContratoEnum.AguardandoInicioVigencia;
            }
            else if (dataInicioVigencia < DateTime.UtcNow && dataFinalVigencia < DateTime.UtcNow)
            {
                return (int)StatusContratoEnum.Vencido;
            }
            else
            {
                return (int)StatusContratoEnum.Vigente;
            }
        }
    }

}
