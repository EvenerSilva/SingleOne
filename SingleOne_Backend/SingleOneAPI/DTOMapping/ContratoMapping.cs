using AutoMapper;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using System;

namespace SingleOneAPI.DTOMapping
{
    public class ContratoMapping : Profile
    {
        public ContratoMapping()
        {
            CreateMap<Contrato, ContratoDTO>()
                .ForMember(dest => dest.Status, m => m.MapFrom(orig => 
                    orig.DTFinalVigencia.HasValue ? 
                    (orig.DTFinalVigencia.Value.Date < DateTime.Now.Date ? "Vencido" : "Vigente") : 
                    "Vigente"))
                .ForMember(dest => dest.Fornecedor, m => m.MapFrom(orig => orig.FornecedorNavigation.Nome))
                .ForMember(dest => dest.FornecedorId, m => m.MapFrom(orig => orig.FornecedorNavigation.Id))
                .ForMember(dest => dest.QtdeRecursos, m => m.MapFrom(orig => orig.Equipamentos.Count))
                .ForMember(dest => dest.Renovavel, m => m.MapFrom(orig => orig.Renovavel))
                .ForMember(dest => dest.DiasParaVencimento, m => m.MapFrom(orig => 
                    orig.DTFinalVigencia.HasValue ? 
                    (orig.DTFinalVigencia.Value.Date - DateTime.Now.Date).Days >= 0 ? 
                    (orig.DTFinalVigencia.Value.Date - DateTime.Now.Date).Days : 0 : 
                    (int?)null))
                .ForMember(dest => dest.ArquivoContrato, m => m.MapFrom(orig => orig.ArquivoContrato))
                .ForMember(dest => dest.NomeArquivoOriginal, m => m.MapFrom(orig => orig.NomeArquivoOriginal))
                .ForMember(dest => dest.DataUploadArquivo, m => m.MapFrom(orig => orig.DataUploadArquivo));

            // Mapeamento manual no service para evitar problemas
        }
    }
}
